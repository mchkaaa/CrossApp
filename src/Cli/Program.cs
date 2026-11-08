using System;
using System.IO;
using System.Linq;
using System.Threading; // ДОДАНО: Для роботи з CancellationToken
using System.Collections.Generic;
using System.Diagnostics;
using CrossApp.Core; // ДОДАНО: Щоб бачити DomainException
using CrossApp.Core.Reports;
using CrossApp.Core.Domain;
using Core.Domain; 
using CrossApp.Core.Services;
using Core.Storage;
using Core.Abstractions;
using CrossApp.Core.Events;

// ==========================================
// 1. ІНІЦІАЛІЗАЦІЯ
// ==========================================
Console.OutputEncoding = System.Text.Encoding.UTF8; // Фікс українських літер у консолі

IOrderStore store = StoreFactory.Create(args);

var logsFolder = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logsFolder);

var consoleSink = new ConsoleSink();
var fileSink = new FileSink(Path.Combine(logsFolder, "sink_events.log"));

var service = new OrderService(store, consoleSink);

Console.WriteLine($"Сховище: {store.GetType().Name}\n");

// ==========================================
// 2. ПІДПИСКА НА ПОДІЇ
// ==========================================
var logPath = Path.Combine(logsFolder, "app.log");

void OnOrderChanged(object? sender, OrderChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId} -> товарів: {e.LinesCount}, сума: {e.Total} грн";
    Console.WriteLine($"[ПОДІЯ] {line}");
    File.AppendAllText(logPath, line + Environment.NewLine);
}

int changes = 0;
service.Changed += (_, _) => changes++; 
service.Changed += OnOrderChanged;

// ==========================================
// 3. ДОПОМІЖНИЙ МЕТОД TryDo
// ==========================================
void TryDo(string title, Action action)
{
    Console.WriteLine($"=== {title} ===");
    try { action(); }
    catch (Exception ex) { Console.WriteLine($"БЛОКУВАННЯ / ПОМИЛКА: {ex.Message}"); }
}

// ==========================================
// 4. СЦЕНАРІЇ РОБОТИ (Лабораторна 6)
// ==========================================
string cid = "CUST-777"; 
string o1 = "ORD-" + Guid.NewGuid().ToString("N")[..4];
string o2 = "ORD-" + Guid.NewGuid().ToString("N")[..4];
string o3 = "ORD-" + Guid.NewGuid().ToString("N")[..4];
string o4 = "ORD-" + Guid.NewGuid().ToString("N")[..4];

TryDo("Сценарій 1: Успішне створення замовлень", () =>
{
    service.CreateOrder(o1, cid);
    service.CreateOrder(o2, cid);
});

TryDo("Сценарій 2: Відписка", () =>
{
    service.Changed -= OnOrderChanged;
    service.CreateOrder(o3, cid);
});

TryDo("Сценарій 3: Відмова (Перевищення ліміту)", () =>
{
    service.CreateOrder(o4, cid); 
});

TryDo("Сценарій 4: Action для імпорту", () =>
{
    string[] importData = { "ORD-999", "", "ORD-777" };
    service.ImportOrders(importData, errorMsg => Console.WriteLine($"[ПОМИЛКА ІМПОРТУ]: {errorMsg}"));
});

// ==========================================
// 5. ДЕМОНСТРАЦІЯ ВІДКЛАДЕНОГО ВИКОНАННЯ (Лабораторна 7)
// ==========================================
Console.WriteLine("\n=== Демо відкладеного виконання ===");
var sourceList = store.List().ToList(); 

var query = sourceList.Where(o => o.Total > 1000m); 
Console.WriteLine($"Дорогих замовлень до змін: {query.Count()}");

sourceList.Add(Order.Create("ORD-DEMO", "CUST-001", 5000m));
Console.WriteLine($"Дорогих замовлень після змін: {query.Count()}"); 

var snapshot = query.ToList();
sourceList.Clear();
Console.WriteLine($"У знімку лишилось: {snapshot.Count}\n"); 

// ==========================================
// 6. ЗВІТИ ТА ІМПОРТ ЧЕРЕЗ CLI 
// ==========================================
var items = store.List(); 
var command = args.FirstOrDefault() ?? "report-top";

Console.WriteLine($"=== КОМАНДА: {command} ===");
switch (command)
{
    // ==========================================
    // ЛАБОРАТОРНА 8: АСИНХРОННИЙ ІМПОРТ
    // ==========================================
    case "--file":
    case "import":
    {
        using var userCts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => 
        { 
            e.Cancel = true; 
            userCts.Cancel(); 
        };

        // Створюємо таймаут-токен на 5 секунд
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); 
        
        // Об'єднуємо токен користувача і токен таймауту
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        var dataPath = "orders.json";
        Console.WriteLine("Починаємо імпорт (натисніть Ctrl+C для скасування)...");

        // Реалізація IProgress (малює кількість оброблених записів в одному рядку через \r)
        var progress = new Progress<int>(p => 
        {
            Console.Write($"\r[Імпорт] Завантажено записів: {p}");
        });

        try
        {
            var importer = new JsonOrderImporter();
            var added = await service.ImportAsync(importer, dataPath, linkedCts.Token, progress);
            Console.WriteLine($"\nУспішно завершено. Разом додано: {added}");
        }
        catch (OperationCanceledException) 
        { 
            // Перевіряємо, який саме токен спрацював
            if (timeoutCts.IsCancellationRequested)
                Console.WriteLine("\nІмпорт скасовано автоматично: перевищено ліміт часу (5 сек).");
            else
                Console.WriteLine("\nІмпорт скасовано користувачем (Ctrl+C).");
        }
        catch (DomainException ex) { Console.WriteLine($"\nДані відхилено: {ex.Message}"); }
        catch (FileNotFoundException) { Console.WriteLine($"\nФайл не знайдено: {dataPath}"); }
        break;
    }

    // ==========================================
    // ЗВІТИ LINQ (Лабораторна 7)
    // ==========================================
    case "report-top":
        PrintOrders(ReportService.TopByTotal(items, 5));
        break;
    case "report-expensive":
        PrintOrders(ReportService.ExpensiveOrders(items, 1000m));
        break;
    case "report-groups":
        foreach (var g in ReportService.ByStatus(items))
            Console.WriteLine($"Статус: {g.Status,-10} | позицій: {g.Count,3} | сумарно: {g.TotalSum} грн");
        break;
    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($"Позицій {t.Items}, всього {t.TotalSum} грн, середній чек {t.Average:F2} грн, макс {t.Max} грн");
        break;
        
    case "report-bonus":
        Console.WriteLine("\n--- ЗАВДАННЯ 1: GroupJoin (Клієнти × Замовлення) ---");
        var customers = new[] { 
            new Customer("CUST-001"), 
            new Customer("CUST-002"), 
            new Customer("CUST-999") 
        };
        foreach (var c in ReportService.CustomerStats(customers, items))
            Console.WriteLine($"Клієнт: {c.CustomerId,-10} | Замовлень: {c.OrdersCount,2} | Сума: {c.TotalSum,6} грн");

        Console.WriteLine("\n--- ЗАВДАННЯ 3: Синтаксис запитів (from ... group) ---");
        foreach (var g in ReportService.ByStatusQuerySyntax(items))
            Console.WriteLine($"Статус: {g.Status,-10} | позицій: {g.Count,3} | сумарно: {g.TotalSum} грн");

        Console.WriteLine("\n--- ЗАВДАННЯ 4: Вимір часу Stopwatch (100 000 записів) ---");
        var rand = new Random();
        var massiveData = Enumerable.Range(1, 100_000)
            .Select(i => Order.Create($"O-{i}", $"C-{rand.Next(1, 100)}", rand.Next(10, 5000)))
            .ToList();

        var sw = Stopwatch.StartNew();
        var sum1 = massiveData.Where(o => o.Total > 1000).Sum(o => o.Total);
        sw.Stop();
        Console.WriteLine($"Без ToList в середині: {sw.ElapsedMilliseconds} мс (Сума: {sum1})");

        sw.Restart();
        var sum2 = massiveData.ToList().Where(o => o.Total > 1000).ToList().Sum(o => o.Total);
        sw.Stop();
        Console.WriteLine($"З ToList в середині:   {sw.ElapsedMilliseconds} мс (Сума: {sum2})");
        break;

    default:
        Console.WriteLine("Команди: import | report-top | report-expensive | report-groups | report-sum | report-bonus");
        break;
}

// Допоміжний метод для виводу таблиці
static void PrintOrders(IReadOnlyList<CrossApp.Core.Reports.OrderLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($" {l.OrderId,-10} Клієнт: {l.CustomerId,-12} Сума: {l.Total,8} грн  Статус: {l.Status}");
}

// ==========================================
// 7. ОГОЛОШЕННЯ КЛАСІВ (Має бути в самому кінці файлу!)
// ==========================================
class ConsoleSink : INotificationSink
{
    public void Notify(string message) => Console.WriteLine($"[ConsoleSink] {message}");
}
class FileSink(string filePath) : INotificationSink
{
    public void Notify(string message) => File.AppendAllText(filePath, $"[FileSink] {message}\n");
}
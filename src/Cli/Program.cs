using System;
using System.IO;
using System.Linq;
using CrossApp.Core.Domain;
using CrossApp.Core.Services;
using Core.Storage;
using Core.Abstractions;
using CrossApp.Core.Events;

// ==========================================
// 1. ІНІЦІАЛІЗАЦІЯ
// ==========================================
IOrderStore store = StoreFactory.Create(args);

// Створюємо папку для логів заздалегідь, щоб і події, і FileSink могли туди писати
var logsFolder = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logsFolder);

// Створюємо обидві реалізації (Додаткове завдання 1)
var consoleSink = new ConsoleSink();
var fileSink = new FileSink(Path.Combine(logsFolder, "sink_events.log"));

// Передаємо одну з них у сервіс (наприклад, consoleSink, але можна легко змінити на fileSink)
var service = new OrderService(store, consoleSink);

Console.WriteLine($"Сховище: {store.GetType().Name}\n");

// ==========================================
// 2. ПІДПИСКА НА ПОДІЇ (Основне завдання 6)
// ==========================================
var logPath = Path.Combine(logsFolder, "app.log");

// Підписник 1: Локальний метод
void OnOrderChanged(object? sender, OrderChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId} -> товарів: {e.LinesCount}, сума: {e.Total} грн";
    Console.WriteLine($"[ПОДІЯ] {line}");
    File.AppendAllText(logPath, line + Environment.NewLine);
}

// Підписник 2: Лямбда-лічильник
int changes = 0;
service.Changed += (_, _) => changes++; 

// Підписуємо локальний метод
service.Changed += OnOrderChanged;
// ==========================================
// 3. ДОПОМІЖНИЙ МЕТОД TryDo
// ==========================================
void TryDo(string title, Action action)
{
    Console.WriteLine($"=== {title} ===");
    try
    {
        action();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"БЛОКУВАННЯ / ПОМИЛКА: {ex.Message}");
    }
}

// ==========================================
// 4. СЦЕНАРІЇ РОБОТИ
// ==========================================

// Клієнт ЗАВЖДИ ТОЙ САМИЙ, щоб тестувати читання з файлу
string cid = "CUST-777"; 

// А от замовлення щоразу нові, щоб не було конфлікту ID
string o1 = "ORD-" + Guid.NewGuid().ToString("N")[..4];
string o2 = "ORD-" + Guid.NewGuid().ToString("N")[..4];
string o3 = "ORD-" + Guid.NewGuid().ToString("N")[..4];
string o4 = "ORD-" + Guid.NewGuid().ToString("N")[..4];

TryDo("Сценарій 1: Успішне створення замовлень (є підписка)", () =>
{
    service.CreateOrder(o1, cid);
    service.CreateOrder(o2, cid);
});


Console.WriteLine($"\nПодій отримано лямбдою-лічильником: {changes}\n");

TryDo("Сценарій 2: Відписка", () =>
{
    service.Changed -= OnOrderChanged;
    service.CreateOrder(o3, cid);
    Console.WriteLine($"Замовлення {o3} створено тихо (без логу події)");
});

Console.WriteLine($"\nПодій отримано лямбдою-лічильником: {changes}\n");

TryDo("Сценарій 3: Відмова (Перевищення ліміту)", () =>
{
    Console.WriteLine($"Спроба створити 4-те активне замовлення для {cid}...");
    service.CreateOrder(o4, cid); 
});

Console.WriteLine();

TryDo("Сценарій 4: Додаткове завдання 2 (Action для імпорту)", () =>
{
    string[] importData = { "ORD-999", "", "ORD-777" };
    service.ImportOrders(importData, errorMsg => Console.WriteLine($"[ПОМИЛКА ІМПОРТУ]: {errorMsg}"));
});

// ==========================================
// 5. ОГОЛОШЕННЯ КЛАСІВ (Має бути в самому кінці файлу!)
// ==========================================
class ConsoleSink : INotificationSink
{
    public void Notify(string message) => Console.WriteLine($"[ConsoleSink] {message}");
}
class FileSink(string filePath) : INotificationSink
{
    public void Notify(string message) => File.AppendAllText(filePath, $"[FileSink] {message}\n");
}
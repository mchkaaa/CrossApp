using Core.Dto;
using Core.Import;

// Отримуємо шлях до файлу з аргументів або беремо за замовчуванням
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

// Перевірка існування файлу
if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string fileName = Path.GetFileName(path).ToLower();
string extension = Path.GetExtension(path).ToLower();

// ---------------------------------------------------------
// ВІТКА 1: Демонстрація змішаного імпорту (Додаткове завдання 2)
// ---------------------------------------------------------
if (fileName == "mixed.csv")
{
    var mixedResult = MixedCsvImporter.Load(path);
    
    Console.WriteLine($"Завантажено товарів: {mixedResult.Products.Count}");
    foreach (var p in mixedResult.Products.Take(5))
        Console.WriteLine($" [Товар]  {p.Id,-6} {p.Name,-26} {p.Price, 10:F2}");

    Console.WriteLine($"\nЗавантажено клієнтів: {mixedResult.Customers.Count}");
    foreach (var c in mixedResult.Customers.Take(5))
        Console.WriteLine($" [Клієнт] {c.Id,-6} {c.Name,-26} {c.Email}");

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine($"\nПропущено рядків: {mixedResult.Errors.Count}");
        foreach (var e in mixedResult.Errors)
            Console.WriteLine($" ! {e}");
    }

    // Статистика (Додаткове завдання 3)
    int accMixed = mixedResult.Products.Count + mixedResult.Customers.Count;
    int skipMixed = mixedResult.Errors.Count;
    int totalMixed = accMixed + skipMixed;
    double errPctMixed = totalMixed == 0 ? 0 : (double)skipMixed / totalMixed * 100;
    
    Console.WriteLine($"\n[Статистика] Усього: {totalMixed} | Прийнято: {accMixed} | Пропущено: {skipMixed} | Помилок: {errPctMixed:F1}%");
    return 0;
}

// ---------------------------------------------------------
// ВІТКА 2: Звичайний імпорт (Додаткове завдання 1 - вибір за розширенням)
// ---------------------------------------------------------
ImportResult<ProductDto> result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => throw new NotSupportedException($"Формат {extension} не підтримується")
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-6} {p.Name,-26} {p.Price, 10:F2} {p.Note}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($" ! {e}");
    }
}

// Статистика (Додаткове завдання 3)
int accepted = result.Items.Count;
int skipped = result.Errors.Count;
int total = accepted + skipped;
double errorPercent = total == 0 ? 0 : (double)skipped / total * 100;

Console.WriteLine($"\n[Статистика] Усього: {total} | Прийнято: {accepted} | Пропущено: {skipped} | Помилок: {errorPercent:F1}%");

return 0;
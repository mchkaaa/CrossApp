using Core.Dto;
using Core.Import;

// 1. Беремо шлях з аргументів або використовуємо файл за замовчуванням
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

// 2. Перевіряємо, чи існує файл, щоб уникнути падіння програми
if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1; // Повертаємо код помилки
}

// 3. Викликаємо наш парсер
ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

// 4. Виводимо успішні результати (перші 5 штук)
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
{
    // Цифри після коми (напр. -6) роблять рівні відступи, щоб вийшла гарна табличка
    Console.WriteLine($" {p.Id,-6} {p.Name,-26} {p.Price, 10:F2} {p.Note}");
}

// 5. Виводимо помилки, якщо вони є
if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($" ! {e}");
    }
}

return 0; // Код успішного завершення
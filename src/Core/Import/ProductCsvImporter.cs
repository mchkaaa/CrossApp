using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    // Роздільник — крапка з комою, щоб уникнути конфліктів з комами в назвах чи цінах
    private const char Separator = ';';

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            // Пропускаємо порожні рядки та коментарі
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропускаємо заголовок (якщо він є у першому рядку)
            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            // Викликаємо приватний метод розбору і перевіряємо результат
            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Реляційний патерн властивості: якщо частин менше 3
            { Length: < 3 } => new ParseFailed($"очікую щонайменше 3 колонки, отримав {parts.Length}"),
            
            // Патерн списку + константний патерн: якщо друга колонка (назва) порожня
            [_, "", ..] => new ParseFailed("назва товару порожня"),
            
            // Охоронна умова when: пробуємо спарсити ціну. Якщо не вийшло або вона від'ємна — помилка
            [var id, var name, var priceStr, ..] when !decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p) || p < 0
                => new ParseFailed($"ціна '{priceStr}' не є коректним додатним числом"),
                
            // Успішний розбір 3 колонок (без примітки)
            [var id, var name, var priceStr]
                => new ParseOk(new ProductDto(id, name, decimal.Parse(priceStr, CultureInfo.InvariantCulture))),
                
            // Успішний розбір 4 колонок (з приміткою)
            [var id, var name, var priceStr, var note]
                => new ParseOk(new ProductDto(id, name, decimal.Parse(priceStr, CultureInfo.InvariantCulture), note)),
                
            // Гілка за замовчуванням
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }


// Внутрішня ієрархія типів для результату розбору одного рядка
private abstract record ParseOutcome;
private sealed record ParseOk(ProductDto Value) : ParseOutcome;
private sealed record ParseFailed(string Reason) : ParseOutcome;
}
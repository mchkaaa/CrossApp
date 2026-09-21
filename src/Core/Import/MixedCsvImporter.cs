using System.Globalization;
using Core.Dto;

namespace Core.Import;

// Оскільки ми повертаємо два різних типи даних, створимо для цього окремий record
public record MixedImportResult(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<CustomerDto> Customers,
    IReadOnlyList<string> Errors
);

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var customers = new List<CustomerDto>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Сортуємо результати по різних кошиках
            switch (ParseLine(line))
            {
                case ParseProductOk p:
                    products.Add(p.Value);
                    break;
                case ParseCustomerOk c:
                    customers.Add(c.Value);
                    break;
                case ParseFailed f:
                    errors.Add($"рядок {number}: {f.Reason}");
                    break;
            }
        }

        return new MixedImportResult(products, customers, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // --- ПАТЕРНИ ДЛЯ ТОВАРІВ (префікс "P") ---
            ["P", var id, var name, var priceStr] 
                when decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p) && p >= 0
                => new ParseProductOk(new ProductDto(id, name, p)),
                
            ["P", var id, var name, var priceStr, var note] 
                when decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p) && p >= 0
                => new ParseProductOk(new ProductDto(id, name, p, note)),
                
            ["P", ..] => new ParseFailed("некоректний формат рядка товару"),

            // --- ПАТЕРНИ ДЛЯ КЛІЄНТІВ (префікс "C") ---
            ["C", var id, var name, var email] 
                => new ParseCustomerOk(new CustomerDto(id, name, email)),
                
            ["C", var id, var name, var email, var phone] 
                => new ParseCustomerOk(new CustomerDto(id, name, email, phone)),
                
            ["C", ..] => new ParseFailed("некоректний формат рядка клієнта"),

            // --- НЕВІДОМИЙ ПРЕФІКС ---
            _ => new ParseFailed($"невідомий префікс рядка '{parts[0]}'")
        };
    }

    // Внутрішні типи тепер мають дві гілки успіху
    private abstract record ParseOutcome;
    private sealed record ParseProductOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseCustomerOk(CustomerDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
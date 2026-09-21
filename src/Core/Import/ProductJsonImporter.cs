using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var errors = new List<string>();
        
        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // Якщо файл порожній або null, беремо порожній список []
            var items = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];
            
            return new ImportResult<ProductDto>(items, errors);
        }
        catch (Exception ex)
        {
            // Якщо JSON зламаний, ловимо помилку
            errors.Add($"Помилка читання JSON: {ex.Message}");
            return new ImportResult<ProductDto>([], errors);
        }
    }
}
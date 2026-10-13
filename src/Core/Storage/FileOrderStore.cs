using System.Text.Json;
using Core.Abstractions;
using CrossApp.Core.Domain;
using CrossApp.Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore(string path) : IOrderStore
{
    // Налаштування серіалізації: статичне readonly поле[cite: 9, 10]
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    // Дозавантаження при першому зверненні[cite: 4]
    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (File.Exists(_path))
        {
            var dtos = JsonSerializer.Deserialize<List<OrderDto>>(File.ReadAllText(_path)) ?? [];
            foreach (var dto in dtos) 
            { 
                var order = Order.FromDto(dto); // Мапінг з DTO у Сутність[cite: 5]
                _cache[order.Id] = order; 
            }
        }
        _loaded = true;
    }

    // Запис на диск після кожної зміни[cite: 4]
    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!); // Оператор прощення null[cite: 9]
        File.WriteAllText(_path, 
            JsonSerializer.Serialize(_cache.Values.Select(o => o.ToDto()).ToList(), Options)); // Проєкція в DTO[cite: 10]
    }

    public IReadOnlyList<Order> List() { EnsureLoaded(); return _cache.Values.ToList(); }
    
    public Order? GetById(string id) { EnsureLoaded(); return _cache.GetValueOrDefault(id); }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");
            
        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Order item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }

    // Реалізація ДОДАТКОВОГО ЗАВДАННЯ 2[cite: 12]
    public IReadOnlyList<Order> Find(Func<Order, bool> predicate)
    {
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}
using Core.Abstractions;
using CrossApp.Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    // Словник для зберігання записів у пам'яті[cite: 3, 4]
    private readonly Dictionary<string, Order> _items = 
        (seed ?? []).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List() => _items.Values.ToList();
    
    public Order? GetById(string id) => _items.GetValueOrDefault(id);
    
    public void Update(Order item) => _items[item.Id] = item;
    
    public bool Remove(string id) => _items.Remove(id);

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item); // Захист від null[cite: 8]
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");
            
        _items.Add(item.Id, item);
    }

    // Реалізація ДОДАТКОВОГО ЗАВДАННЯ 2
    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) 
        => _items.Values.Where(predicate).ToList();
}
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using CrossApp.Core.Domain;
using Core.Dto;
using CrossApp.Core; // Додаємо цей using, щоб бачити клас SampleData

namespace Core.Storage;

public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    // Якщо seed передали — використовуємо його. 
    // Якщо ні — беремо 20 готових замовлень із SampleData
    private readonly Dictionary<string, Order> _items = 
        (seed ?? SampleData.Generate()).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List() => _items.Values.ToList();
    
    public Order? GetById(string id) => _items.GetValueOrDefault(id);
    
    public void Update(Order item) => _items[item.Id] = item;
    
    public bool Remove(string id) => _items.Remove(id);

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item); // Захист від null
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");
            
        _items.Add(item.Id, item);
    }

    // Реалізація ДОДАТКОВОГО ЗАВДАННЯ 2
    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) 
        => _items.Values.Where(predicate).ToList();
}
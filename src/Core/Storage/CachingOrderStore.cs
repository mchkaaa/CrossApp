using Core.Abstractions;
using CrossApp.Core.Domain;
using System;
using System.Collections.Generic;

namespace Core.Storage;

// ДОДАТКОВЕ ЗАВДАННЯ 1: Декоратор, що приймає інший IOrderStore
public sealed class CachingOrderStore(IOrderStore innerStore) : IOrderStore
{
    private readonly IOrderStore _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    private IReadOnlyList<Order>? _cachedList;

    public IReadOnlyList<Order> List()
    {
        // Якщо кеш порожній, ідемо у внутрішнє сховище
        _cachedList ??= _innerStore.List();
        return _cachedList;
    }

    public Order? GetById(string id) => _innerStore.GetById(id);

    public void Add(Order item)
    {
        _innerStore.Add(item);
        _cachedList = null; // Скидаємо кеш при зміні
    }

    public void Update(Order item)
    {
        _innerStore.Update(item);
        _cachedList = null;
    }

    public bool Remove(string id)
    {
        var result = _innerStore.Remove(id);
        if (result) _cachedList = null;
        return result;
    }

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) 
        => _innerStore.Find(predicate);

    // Доданий метод для відображення реального сховища в консолі
    public override string ToString()
    {
        return $"CachingOrderStore (обгортає {_innerStore.GetType().Name})";
    }
}
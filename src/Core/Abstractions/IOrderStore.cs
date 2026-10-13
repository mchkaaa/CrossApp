using System;
using System.Collections.Generic;
using CrossApp.Core.Domain;

namespace Core.Abstractions;

// Інтерфейс визначає контракт взаємодії без зазначення способу зберігання.
public interface IOrderStore
{
    IReadOnlyList<Order> List();
    Order? GetById(string id);
    void Add(Order item);
    void Update(Order item);
    bool Remove(string id);
    
    // ДОДАТКОВЕ ЗАВДАННЯ 2: Пошук з параметром Func<Order, bool>
    IReadOnlyList<Order> Find(Func<Order, bool> predicate);
}
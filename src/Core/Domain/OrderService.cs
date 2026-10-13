using System;
using System.Linq; // пошук, фільтрація
using Core.Abstractions; // Твій шлях до інтерфейсу IOrderStore
using CrossApp.Core.Domain;       // Твій шлях до сутності Order (якщо сервіс в іншій папці)

namespace CrossApp.Core.Services; // Зазвичай сервіси тепер лежать у папці Services

// Ін'єкція залежності через первинний конструктор (параметр у дужках)[cite: 7]
public sealed class OrderService(IOrderStore store)
{
    // Зберігаємо посилання на сховище і захищаємо від null[cite: 8, 9]
    // ПУБЛІЧНОГО SET ТУТ БІЛЬШЕ НЕМАЄ!
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(string orderId, string customerId)
    {
        // Звертаємося не до внутрішнього списку, а до Сховища (_store)
        // Використовуємо метод Find з додаткового завдання №2 (або _store.List().Count(...), якщо не робила його)[cite: 12]
        var activeOrdersCount = _store.Find(o => o.CustomerId == customerId && o.IsActive).Count;
        
        if (activeOrdersCount >= 3)
        {
            throw new InvalidOperationException($"Клієнт {customerId} вже має {activeOrdersCount} активних замовлень. Ліміт перевищено, нове замовлення заборонено!");
        }

        var newOrder = Order.Create(orderId, customerId);
        
        // Зберігаємо створене замовлення у файл або в пам'ять (сервіс не знає куди саме, він просто віддає команду)
        _store.Add(newOrder); 
        
        return newOrder;
    }
}
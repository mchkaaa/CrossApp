using System;
using System.Linq;
using Core.Abstractions;
using CrossApp.Core.Domain;
using CrossApp.Core.Events;

namespace CrossApp.Core.Services;

// Додано INotificationSink у первинний конструктор (Додаткове завдання 1)
public sealed class OrderService(IOrderStore store, INotificationSink? sink = null)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly INotificationSink? _sink = sink;

    // Подія (Основне завдання 6)
    public event EventHandler<OrderChangedEventArgs>? Changed;

    // Безпечний виклик події
    private void OnChanged(Order o, OrderChangeKind kind, int linesCount, decimal total) =>
        Changed?.Invoke(this, new OrderChangedEventArgs(o.Id, kind, linesCount, total));

    public Order CreateOrder(string orderId, string customerId)
    {
        // Використання Func для фільтрації
        var activeOrdersCount = _store.Find(o => o.CustomerId == customerId && o.IsActive).Count;
        
        if (activeOrdersCount >= 3)
        {
            throw new InvalidOperationException($"Клієнт {customerId} вже має {activeOrdersCount} активних замовлень. Ліміт перевищено, нове замовлення заборонено!");
        }

        var newOrder = Order.Create(orderId, customerId);
        
        _store.Add(newOrder); 
        
        // Піднімаємо подію ТІЛЬКИ ПІСЛЯ успішного додавання
        OnChanged(newOrder, OrderChangeKind.Created, 1, 100.0m); 

        // Сповіщаємо через додатковий інтерфейс (Додаткове завдання 1)
        _sink?.Notify($"[Sink] Замовлення {orderId} успішно збережено в базу.");

        return newOrder;
    }

    // Метод з Action<string> для сповіщення про помилки (Додаткове завдання 2)
    public void ImportOrders(string[] lines, Action<string> logError)
    {

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {

                logError("Пропущено порожній рядок.");
                continue;
            }      
             // Відправляємо успішне повідомлення 
            _sink?.Notify($"[Імпорт] Успішно оброблено рядок: {line}");
        }
    }
}
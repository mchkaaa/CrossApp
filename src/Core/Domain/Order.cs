using System;

namespace CrossApp.Core.Domain;

public enum OrderStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public class Order
{
    public string Id { get; }
    public string CustomerId { get; }
    public bool IsActive { get; set; } = true;
    
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    // 1. Робимо конструктор приватним (ніхто не зможе викликати new Order)
    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    // 2. Додаємо фабричний метод
    public static Order Create(string id, string customerId)
    {
        return new Order(id, customerId);
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        bool isValidTransition = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            _ => false 
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException($"Неприпустимий перехід статусу замовлення з {Status} у {newStatus}");
        }
        Status = newStatus;
    }
}
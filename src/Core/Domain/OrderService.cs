using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossApp.Core.Domain;

public class OrderService
{
    public List<Order> ExistingOrders { get; set; } = new();

    public Order CreateOrder(string orderId, string customerId)
    {
        var activeOrdersCount = ExistingOrders.Count(o => o.CustomerId == customerId && o.IsActive);
        
        if (activeOrdersCount >= 3)
        {
            throw new InvalidOperationException($"Клієнт {customerId} вже має {activeOrdersCount} активних замовлень. Ліміт перевищено, нове замовлення заборонено!");
        }

        var newOrder = Order.Create(orderId, customerId);
        ExistingOrders.Add(newOrder);
        return newOrder;
    }
}
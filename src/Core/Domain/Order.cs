using System;
using CrossApp.Core.Dto;

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
    
    // НОВЕ: числове поле для звітів LINQ
    public decimal Total { get; private set; }

    private Order(string id, string customerId, decimal total)
    {
        Id = id;
        CustomerId = customerId;
        Total = total;
    }

    // Додано total у фабричний метод
    public static Order Create(string id, string customerId, decimal total = 0m)
    {
        return new Order(id, customerId, total);
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

    public static Order FromDto(OrderDto dto)
    {
        var order = new Order(dto.Id, dto.CustomerId, dto.Total);
        order.IsActive = dto.IsActive;
        order.Status = dto.Status;
        return order;
    }

    public OrderDto ToDto()
    {
        return new OrderDto(Id, CustomerId, IsActive, Status, Total);
    }
}
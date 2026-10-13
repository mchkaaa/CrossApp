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

    // =========================================================
    // НОВЕ: МЕТОДИ ДЛЯ 5-Ї ЛАБОРАТОРНОЇ РОБОТИ (МАПІНГ З/В DTO)
    // =========================================================

    // 3. Відновлення об'єкта з JSON-файлу
    public static Order FromDto(OrderDto dto)
    {
        // Ми викликаємо приватний конструктор, щоб відновити базові дані.
        var order = new Order(dto.Id, dto.CustomerId);
        
        // Відновлюємо поточний стан (навіть private set доступний всередині класу)
        order.IsActive = dto.IsActive;
        order.Status = dto.Status;
        
        return order;
    }

    // 4. Пакування об'єкта для збереження на диск
    public OrderDto ToDto()
    {
        return new OrderDto(Id, CustomerId, IsActive, Status);
    }
}
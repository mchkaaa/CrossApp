using System;

namespace CrossApp.Core.Domain;

public class OrderLine
{
    public string ProductId { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    public OrderLine(string productId, decimal price, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Кількість у рядку замовлення має бути більшою за нуль");
            
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Ціна не може бути від'ємною");

        ProductId = productId;
        Price = price;
        Quantity = quantity;
    }
}
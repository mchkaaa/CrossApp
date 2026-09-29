using Core.Dto;

namespace Core.Domain;

// Перелічування для додаткового завдання 3
public enum ProductStatus
{
    Active,
    Suspended,
    Discontinued
}

public sealed class Product
{
    private int _quantity;

    public string Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public string? Note { get; }
    public int Quantity => _quantity;
    
    // Властивість стану (змінюється лише зсередини класу)
    public ProductStatus Status { get; private set; }

    // Приватний конструктор
    private Product(string id, string name, decimal price, int quantity, string? note)
    {
        Id = id;
        Name = name;
        Price = price;
        _quantity = quantity;
        Note = note;
        Status = ProductStatus.Active; // За замовчуванням товар стає активним при створенні
    }

    // Фабричний метод Create
    public static Product Create(string id, string name, decimal price, int quantity, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));
            
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва не може бути порожньою", nameof(name));
            
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною");

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Початковий залишок не може бути від'ємним");

        return new Product(id.Trim(), name.Trim(), price, quantity, note?.Trim());
    }

    // Методи зміни стану
    public void RegisterArrival(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість приходу має бути більшою за нуль");
            
        _quantity += amount;
    }

    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість видачі має бути більшою за нуль");
            
        if (amount > _quantity)
            throw new InvalidOperationException($"Не можна видати {amount}: залишок товару {Id} = {_quantity}");
            
        _quantity -= amount;
    }

    // Метод для зміни статусу (Додаткове завдання 3)
    public void ChangeStatus(ProductStatus newStatus)
    {
        Status = (Status, newStatus) switch
        {
            (ProductStatus.Active, ProductStatus.Suspended) => newStatus,
            (ProductStatus.Suspended, ProductStatus.Active) => newStatus,
            (ProductStatus.Active, ProductStatus.Discontinued) => newStatus,
            (ProductStatus.Suspended, ProductStatus.Discontinued) => newStatus,
            
            // Якщо намагаються зробити неприпустимий перехід (наприклад, з Discontinued назад в Active)
            _ => throw new InvalidOperationException($"Неможливий перехід товару {Id} зі стану {Status} у {newStatus}")
        };
    }

    // Мапінг DTO
    public ProductDto ToDto() => new(Id, Name, Price, Quantity, Note);
    
    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Name, dto.Price, dto.Quantity, dto.Note);

    public override string ToString() => 
        $"{Id} | {Name} | {Price} грн | Залишок: {Quantity} шт. | Статус: {Status}";
}
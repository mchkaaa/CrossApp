using System;
using System.Collections.Generic;
using Core.Domain;
using Core.Dto; // Додано для доступу до ProductDto та ImportResult
using CrossApp.Core.Domain;

Console.WriteLine("=== Сценарій 1: Успішна робота ===");
Product product = Product.Create("P-001", "Ноутбук Dell", 45000m, 10);
Console.WriteLine(product);

product.RegisterArrival(5); // Додали 5
product.Issue(3);           // Видали 3
Console.WriteLine(product); // Залишок має бути 12
Console.WriteLine();

Console.WriteLine("=== Сценарій 2: Порушення інваріантів ===");

TryDo("Видача більша за залишок", () => product.Issue(100));
TryDo("Порожня назва при створенні", () => Product.Create("P-002", "", 500m, 10));
TryDo("Від'ємна кількість приходу", () => product.RegisterArrival(-5));

// Допоміжний метод для відлову помилок
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"{title}: виняток НЕ спрацював - інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{title}: {ex.GetType().Name} - {ex.Message}");
    }
}

Console.WriteLine("=== Сценарій 3: Перевірка додаткового завдання 1 ===");
// Створюємо фейковий результат імпорту з 3-ї лаби
var fakeImportResult = new ImportResult<ProductDto>(
    new List<ProductDto> 
    { 
        new ProductDto("P-998", "Нормальний товар", 100m, 10, null),
        new ProductDto("P-999", "Поламаний товар", -50m, 5, null)
    },
    new List<string> { "Якась синтаксична помилка парсера (рядок 1)" }
);

// Викликаємо наш метод
var (valid, mapErrors) = MapToDomain(fakeImportResult);

Console.WriteLine($"Успішно створено сутностей: {valid.Count}");
Console.WriteLine($"Загальна кількість помилок: {mapErrors.Count}");
foreach (var err in mapErrors)
{
    Console.WriteLine($" - {err}");
}
Console.WriteLine();

// Додаткове завдання 1
static (List<Product> ValidProducts, List<string> AllErrors) MapToDomain(ImportResult<ProductDto> import)
{
    var products = new List<Product>();
    var errors = new List<string>(import.Errors);

    foreach (var dto in import.Items)
    {
        try
        {
            Product p = Product.FromDto(dto);
            products.Add(p);
        }
        catch (Exception ex)
        {
            errors.Add($"Помилка бізнес-логіки у товарі {dto.Id}: {ex.Message}");
        }
    }

    return (products, errors);
}

Console.WriteLine("=== Сценарій 4: Інваріант для двох сутностей (Domain Service) ===");

var orderService = new OrderService();
var customerId = "CUST-001";

// Імітуємо, що у клієнта в базі вже є 3 активні замовлення
orderService.ExistingOrders.Add(Order.Create("ORD-1", customerId));
orderService.ExistingOrders.Add(Order.Create("ORD-2", customerId));
orderService.ExistingOrders.Add(Order.Create("ORD-3", customerId));

try
{
    Console.WriteLine($"Спроба клієнта {customerId} створити 4-те активне замовлення...");
    var newOrder = orderService.CreateOrder("ORD-4", customerId);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Успішно відловлено помилку сервісу: {ex.Message}");
}
Console.WriteLine();

Console.WriteLine("=== Сценарій 5: Перевірка станів (switch expression) ===");
var statusOrder = Order.Create("ORD-99", "CUST-002");
Console.WriteLine($"Початковий статус: {statusOrder.Status}");

// Успішний перехід
statusOrder.ChangeStatus(OrderStatus.Confirmed);
Console.WriteLine($"Змінили статус на: {statusOrder.Status}");

try
{
    Console.WriteLine("Спроба перевести Confirmed назад у Draft...");
    // Цей перехід заборонений правилами у switch
    statusOrder.ChangeStatus(OrderStatus.Draft);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Успішно відловлено помилку: {ex.Message}");
}
Console.WriteLine();


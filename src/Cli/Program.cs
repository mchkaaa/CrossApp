using System;
using System.IO;
using System.Linq;
using CrossApp.Core.Domain;
using CrossApp.Core.Services;
using Core.Storage;
using Core.Abstractions;

// 1. Читаємо аргументи під час запуску (чи є прапорець --file)
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

// 2. Composition Root: Ручне впровадження залежностей (DI)
// Якщо консоль запущена з --file, створюємо FileOrderStore, інакше - InMemoryOrderStore
IOrderStore baseStore = useFile 
    ? new FileOrderStore(dataPath) 
    : new InMemoryOrderStore();

// Обертаємо сховище в Декоратор (якщо робимо Додаткове завдання 1)[cite: 12]
IOrderStore store = new CachingOrderStore(baseStore);

// Передаємо створене сховище в наш сервіс[cite: 6]
var service = new OrderService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

// ==========================================
// СЦЕНАРІЙ РОБОТИ (Замість старого TryDo)
// ==========================================

Console.WriteLine("\n=== Сценарій 1: Успішне створення замовлень ===");
try
{
    var order1 = service.CreateOrder("ORD-001", "CUST-777");
    Console.WriteLine($"Створено замовлення: {order1.Id} для клієнта CUST-777");
    
    var order2 = service.CreateOrder("ORD-002", "CUST-777");
    Console.WriteLine($"Створено замовлення: {order2.Id} для клієнта CUST-777");
    
    var order3 = service.CreateOrder("ORD-003", "CUST-777");
    Console.WriteLine($"Створено замовлення: {order3.Id} для клієнта CUST-777");
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

// Крок 6: Сценарій відмови (перевірка бізнес-правила на 4-му замовленні)[cite: 6]
Console.WriteLine("\n=== Сценарій 2: Відмова (Перевищення ліміту) ===");
try
{
    Console.WriteLine("Спроба створити 4-те активне замовлення для CUST-777...");
    service.CreateOrder("ORD-004", "CUST-777"); // Цей рядок має викликати виняток
}
catch (Exception ex) // Перехоплюємо виняток у Cli[cite: 6]
{
    Console.WriteLine($"БЛОКУВАННЯ: {ex.Message}"); // Друкуємо зрозумілий текст[cite: 6]
}
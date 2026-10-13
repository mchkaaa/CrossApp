using System;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Core.Storage;

namespace Cli;
// ... далі твій код без змін
// ДОДАТКОВЕ ЗАВДАННЯ 3: Фабрика[cite: 12]
public static class StoreFactory
{
    public static IOrderStore Create(string[] args)
    {
        bool useFile = args.Contains("--file"); // Перемикач режиму[cite: 6]
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

        IOrderStore baseStore = useFile 
            ? new FileOrderStore(dataPath) 
            : new InMemoryOrderStore(); // Тут можна передати SeedData, якщо в тебе є

        // Обертаємо базове сховище у Декоратор (Додаткове завдання 1)[cite: 12]
        return new CachingOrderStore(baseStore);
    }
}
using System;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Core.Storage;

namespace CrossApp.Core.Services;

public static class StoreFactory
{
    public static IOrderStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache");

        // ПОВЕРНУЛИ ТВІЙ ПРАВИЛЬНИЙ ШЛЯХ З 5-Ї ЛАБИ!
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");
        
        // Переконуємося, що папка data існує (якщо її видалили)
        Directory.CreateDirectory(Path.GetDirectoryName(dataPath)!);

        // Тернарний оператор для бази (тепер з правильним шляхом)
        IOrderStore baseStore = useFile 
            ? new FileOrderStore(dataPath) 
            : new InMemoryOrderStore();

        // Тернарний оператор для кешу
        return useCache 
            ? new CachingOrderStore(baseStore) 
            : baseStore;
    }
}
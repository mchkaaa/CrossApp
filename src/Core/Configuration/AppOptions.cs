namespace Core.Configuration;

public sealed class AppOptions
{
    public StorageOptions Storage { get; init; } = new();
    public string EnvironmentName { get; init; } = "Development";
    public OrdersOptions Orders { get; init; } = new();
}

// Налаштування для домену (Замовлення)
public sealed class OrdersOptions
{
    public string Currency { get; init; } = "UAH";
}

public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "orders.json";

    public string ResolveFullPath(string baseDir)
    {
        // Path.Combine використовується для кросплатформності замість зліплювання рядків через "\"
        var root = Path.IsPathRooted(DataPath) ? DataPath : Path.Combine(baseDir, DataPath);
        Directory.CreateDirectory(root);
        return Path.GetFullPath(Path.Combine(root, FileName));
    }
}
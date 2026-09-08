using System.Runtime.InteropServices;
using System.Text.Json; // Обов'язково додаємо для роботи з JSON
using System.Text.Encodings.Web;
// Збираємо всі дані в один об'єкт
var envInfo = new
{
    Student = "Долошецька Соломія Миколаївна, група ФЕІ-36",
    Domain = "Замовлення (клієнти, товари, замовлення, рядок замовлення)",
    OSDescription = RuntimeInformation.OSDescription,
    EnvironmentOS = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory
};

// Перевіряємо, чи є прапорець --json в аргументах (args)
if (args.Contains("--json"))
{
    // Форматуємо об'єкт у гарний JSON (з відступами та правильним кодуванням)
    var options = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    string jsonString = JsonSerializer.Serialize(envInfo, options);
    Console.WriteLine(jsonString);
}
else
{
    // Дані беремо з об'єкта envInfo
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine($"Студентка: {envInfo.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {envInfo.OSDescription}");
    Console.WriteLine($"ОС (Environment) : {envInfo.EnvironmentOS}");
    Console.WriteLine($"Архітектура процесу : {envInfo.Architecture}");
    Console.WriteLine($"Версія .NET (CLR) : {envInfo.DotNetVersion}");
    Console.WriteLine($"Runtime : {envInfo.Runtime}");
    Console.WriteLine($"Каталог застосунку : {envInfo.AppDirectory}");
    Console.WriteLine($"Поточний каталог : {envInfo.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {envInfo.Domain}");
}
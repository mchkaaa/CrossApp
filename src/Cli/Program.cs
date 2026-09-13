using System.Text.Json;
using System.Text.Encodings.Web;
using Core; // Підключаємо нашу нову бібліотеку

// 1. Отримуємо всі дані з бібліотеки Core ОДНИМ викликом
EnvironmentReport report = EnvironmentInfo.Collect();

// 2. Форматуємо вивід залежно від наявності прапорця --json
if (args.Contains("--json"))
{
    var options = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    string jsonString = JsonSerializer.Serialize(report, options);
    Console.WriteLine(jsonString);
}
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine($"Студентка: {report.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {report.OsDescription}");
    Console.WriteLine($"ОС (Environment) : {report.EnvironmentOs}");
    Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {report.DotNetVersion}");
    Console.WriteLine($"Runtime : {report.FrameworkDescription}");
    
    // Нові поля з лабораторної 2:
    Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    
    Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}");
    Console.WriteLine($"Поточний каталог : {report.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {report.Domain}");
}
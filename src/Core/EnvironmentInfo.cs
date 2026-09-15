using System.Runtime.InteropServices;

namespace Core;

public sealed record EnvironmentReport(
    string Student,
    string Domain,
    string OsDescription,
    string EnvironmentOs,
    string ProcessArchitecture,
    string DotNetVersion,
    string FrameworkDescription,
    string BaseDirectory,
    string CurrentDirectory,
    string DetectedRid,
    string ReportedRid,
    string BuildNote // <--- 1. ДОДАНО НОВЕ ПОЛЕ
);

public static class EnvironmentInfo
{
    // 2. ДОДАНА ДИРЕКТИВА КОМПІЛЯТОРА
#if NET10_0_OR_GREATER
    const string BuildNote = "збірка під net10.0";
#else
    const string BuildNote = "збірка під net8.0";
#endif

    public static EnvironmentReport Collect() => new(
        "Долошецька Соломія Миколаївна, група ФЕІ-36",
        "Замовлення (клієнти, товари, замовлення, рядок замовлення)",
        RuntimeInformation.OSDescription,
        Environment.OSVersion.ToString(),
        RuntimeInformation.ProcessArchitecture.ToString(),
        Environment.Version.ToString(),
        RuntimeInformation.FrameworkDescription,
        AppContext.BaseDirectory,
        Environment.CurrentDirectory,
        DetectRid(),
        RuntimeInformation.RuntimeIdentifier,
        BuildNote // <--- 3. ПЕРЕДАЄМО ЗНАЧЕННЯ
    );

    private static string DetectRid()
    {
        // ... (твій попередній код DetectRid залишається без змін)
        string os =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
        };

        return $"{os}-{arch}";
    }
}
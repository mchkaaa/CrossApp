using System.Runtime.InteropServices;

namespace Core; // Корінний простір імен для бібліотеки

// Record відповідає лише за ЗБЕРЕЖЕННЯ даних. Це як контейнер.
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
    string ReportedRid
);

// Static class відповідає за ПОВЕДІНКУ (збір цих даних).
public static class EnvironmentInfo
{
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
        RuntimeInformation.RuntimeIdentifier
    );

    // Ручне визначення RID (Runtime Identifier) за допомогою тернарних операторів та switch
    private static string DetectRid()
    {
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
            _ => "unknown" // Гілка за замовчуванням (дискерд)
        };

        return $"{os}-{arch}";
    }
}
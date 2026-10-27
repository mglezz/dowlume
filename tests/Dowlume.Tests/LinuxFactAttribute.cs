namespace Dowlume.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux() || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64)
            Skip = "Linux-native contract; executed by the Linux CI job.";
    }
}

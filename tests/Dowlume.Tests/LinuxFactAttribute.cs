namespace Dowlume.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
            Skip = "Linux-native contract; executed by the Linux CI job.";
    }
}

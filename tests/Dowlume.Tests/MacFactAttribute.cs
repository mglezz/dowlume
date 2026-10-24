using System.Runtime.InteropServices;

namespace Dowlume.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MacFactAttribute : FactAttribute
{
    public MacFactAttribute()
    {
        if (!OperatingSystem.IsMacOSVersionAtLeast(15) || RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            Skip = "macOS 15+ Apple Silicon native contract; executed by the macOS CI job.";
        }
    }
}

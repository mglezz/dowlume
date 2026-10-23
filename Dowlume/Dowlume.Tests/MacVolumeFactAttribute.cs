namespace Dowlume.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MacVolumeFactAttribute : FactAttribute
{
    public MacVolumeFactAttribute()
    {
        if (!OperatingSystem.IsMacOS() || Environment.GetEnvironmentVariable("DOWLUME_TEST_OWNED_VOLUME") is null)
            Skip = "Disposable APFS volume contract; executed by Test-MacFileReads.py on macOS CI.";
    }
}

[CollectionDefinition("Mac filesystem", DisableParallelization = true)]
public sealed class MacFileSystemCollection
{
}

namespace FileOperationAcceptance;

internal static class RecycleProbe
{
    public static Task RunAsync(string sourcePath, string expectedHash, string reportPath) =>
        Task.FromException(new NotSupportedException("Native recycling probes are retired. Preserve the existing evidence; do not rerun capacity tests."));
}

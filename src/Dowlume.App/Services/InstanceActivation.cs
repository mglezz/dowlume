using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Dowlume.App.Services;

// The profile file lock remains the authority for ownership. This channel only requests UI activation.
internal sealed class InstanceActivation : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _listener;

    internal InstanceActivation(string profile, Action activate, Action<Exception> reportError)
    {
        string name = PipeName(profile);
        _listener = Task.Run(async () =>
        {
            try
            {
                while (!_stop.IsCancellationRequested)
                {
                    using var pipe = new NamedPipeServerStream(name, PipeDirection.InOut, 1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                    await pipe.WaitForConnectionAsync(_stop.Token).ConfigureAwait(false);
                    using var request = CancellationTokenSource.CreateLinkedTokenSource(_stop.Token);
                    request.CancelAfter(TimeSpan.FromSeconds(5));
                    try
                    {
                        await pipe.WriteAsync(BitConverter.GetBytes(Environment.ProcessId), request.Token).ConfigureAwait(false);
                        byte[] command = new byte[1];
                        await pipe.ReadExactlyAsync(command, request.Token).ConfigureAwait(false);
                        if (command[0] != 1)
                            continue;
                        activate();
                        await pipe.WriteAsync(new byte[] { 1 }, request.Token).ConfigureAwait(false);
                    }
                    catch (Exception error) when (error is IOException or OperationCanceledException)
                    {
                        // A crashed, cancelled or stalled launcher must not disable future activation.
                    }
                }
            }
            catch (OperationCanceledException) when (_stop.IsCancellationRequested) { }
            catch (Exception error) { reportError(error); }
        });
    }

    internal static async Task<bool> TryActivateAsync(string profile, TimeSpan? timeout = null)
    {
        using var deadline = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(5));
        try
        {
            while (!deadline.IsCancellationRequested)
            {
                try
                {
                    using var pipe = new NamedPipeClientStream(".", PipeName(profile), PipeDirection.InOut,
                        PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                    await pipe.ConnectAsync(deadline.Token).ConfigureAwait(false);
                    byte[] process = new byte[sizeof(int)];
                    await pipe.ReadExactlyAsync(process, deadline.Token).ConfigureAwait(false);
                    if (OperatingSystem.IsWindows())
                        _ = AllowSetForegroundWindow(BitConverter.ToInt32(process));
                    await pipe.WriteAsync(new byte[] { 1 }, deadline.Token).ConfigureAwait(false);
                    byte[] acknowledgement = new byte[1];
                    await pipe.ReadExactlyAsync(acknowledgement, deadline.Token).ConfigureAwait(false);
                    return acknowledgement[0] == 1;
                }
                catch (IOException)
                {
                    // The previous connection may be closing while this launcher connects.
                    await Task.Delay(50, deadline.Token).ConfigureAwait(false);
                }
            }
            return false;
        }
        catch (Exception error) when (error is IOException or OperationCanceledException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static string PipeName(string profile)
    {
        string path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(profile));
        if (OperatingSystem.IsWindows())
            path = path.ToUpperInvariant();
        string key = Environment.UserName + "\0" + path;
        // Unix domain sockets have a short path limit, especially under macOS temporary directories.
        return "dwl-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..32];
    }

    [DllImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowSetForegroundWindow(int processId);

    public void Dispose()
    {
        _stop.Cancel();
        _listener.GetAwaiter().GetResult();
        _stop.Dispose();
    }
}

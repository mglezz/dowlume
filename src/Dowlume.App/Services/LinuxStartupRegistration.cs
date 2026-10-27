using System.Text;
using Dowlume.Infrastructure;

namespace Dowlume.App.Services;

public sealed class LinuxStartupRegistration : IStartupRegistration
{
    private readonly string? _executable;
    private readonly string _path;

    public LinuxStartupRegistration() : this(Environment.ProcessPath, ConfigHome()) { }

    internal LinuxStartupRegistration(string? executable, string configHome)
    {
        _executable = executable;
        _path = Path.Combine(configHome, "autostart", "com.mglezz.dowlume.desktop");
    }

    internal static string ConfigHome()
    {
        string? configured = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        return configured is not null && Path.IsPathFullyQualified(configured)
            ? configured : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
    }

    public bool IsSupported => OperatingSystem.IsLinux() && _executable is not null &&
        Path.IsPathFullyQualified(_executable) && Path.GetFileName(_executable) == "Dowlume.App" &&
        File.Exists(_executable) && !_executable.Any(c => char.IsControl(c) || c is '=' or '%');

    public bool IsEnabled
    {
        get
        {
            if (!IsSupported)
                return false;
            Dictionary<string, string>? entry = ReadOwned();
            return entry is not null && entry.GetValueOrDefault("Hidden") != "true" &&
                entry.GetValueOrDefault("X-GNOME-Autostart-enabled") != "false" &&
                entry.GetValueOrDefault("Exec") == QuoteExecutable(_executable!);
        }
    }

    internal static string QuoteExecutable(string executable)
    {
        if (!executable.StartsWith('/') || executable.Any(c => char.IsControl(c) || c is '=' or '%'))
            throw new ArgumentException("The application path cannot be represented safely in a desktop entry.");
        var result = new StringBuilder("\"");
        foreach (char c in executable)
        {
            // Desktop-entry string unescaping precedes Exec argument unquoting.
            if (c == '\\')
                result.Append("\\\\\\\\");
            else if (c is '"' or '`' or '$')
                result.Append("\\\\").Append(c);
            else
                result.Append(c);
        }
        return result.Append('"').ToString();
    }

    private Dictionary<string, string>? ReadOwned()
    {
        PathSafety.RejectLinks(_path);
        if (!File.Exists(_path))
            return null;
        if (new FileInfo(_path).Length > 65536)
            throw Unmanaged();
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        bool group = false;
        foreach (string line in File.ReadAllLines(_path))
        {
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            if (line == "[Desktop Entry]" && !group)
            {
                group = true;
                continue;
            }
            int equal = line.IndexOf('=');
            if (!group || equal < 1 || !values.TryAdd(line[..equal], line[(equal + 1)..]))
                throw Unmanaged();
        }
        string[] allowed = ["Type", "Name", "Exec", "Icon", "Terminal", "X-Dowlume-Managed", "Hidden", "X-GNOME-Autostart-enabled", "TryExec"];
        if (values.Keys.Except(allowed).Any() || values.GetValueOrDefault("Type") != "Application" ||
            values.GetValueOrDefault("Name") != "Dowlume" || values.GetValueOrDefault("X-Dowlume-Managed") != "1" ||
            !values.ContainsKey("Exec"))
            throw Unmanaged();
        return values;
    }

    public void SetEnabled(bool enabled)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("Login startup requires the installed Dowlume application.");
        _ = ReadOwned();
        if (!enabled)
        {
            if (File.Exists(_path))
                File.Delete(_path);
            return;
        }
        PathSafety.RejectLinks(_executable!);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        PathSafety.RejectLinks(_path);
        string text = "[Desktop Entry]\nType=Application\nName=Dowlume\nExec=" + QuoteExecutable(_executable!) +
            "\nTryExec=" + _executable!.Replace("\\", "\\\\", StringComparison.Ordinal) +
            "\nIcon=com.mglezz.dowlume\nTerminal=false\nX-Dowlume-Managed=1\n";
        string temporary = _path + "." + Guid.NewGuid().ToString("N");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(Encoding.UTF8.GetBytes(text));
                stream.Flush(true);
            }
            _ = ReadOwned();
            File.Move(temporary, _path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static IOException Unmanaged() => new("The existing login configuration is not managed by Dowlume.");
}

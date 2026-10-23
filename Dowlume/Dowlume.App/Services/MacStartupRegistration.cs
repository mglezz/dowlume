using System.Xml;
using System.Xml.Linq;
using Dowlume.Infrastructure;

namespace Dowlume.App.Services;

public sealed class MacStartupRegistration : IStartupRegistration
{
    private const string Label = "com.mglezz.dowlume.login";
    private readonly string? _executable;
    private readonly string _path;

    public MacStartupRegistration() : this(Environment.ProcessPath,
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "LaunchAgents"))
    {
    }

    internal MacStartupRegistration(string? executable, string directory)
    {
        _executable = executable;
        _path = Path.Combine(directory, Label + ".plist");
    }

    public bool IsSupported => OperatingSystem.IsMacOS() && _executable is not null &&
        Path.IsPathFullyQualified(_executable) && File.Exists(_executable) &&
        _executable.EndsWith(".app/Contents/MacOS/Dowlume.App", StringComparison.Ordinal) &&
        !_executable.Contains("/AppTranslocation/", StringComparison.Ordinal);

    public bool IsEnabled => IsSupported && ReadOwnedExecutable() == _executable;

    private string? ReadOwnedExecutable()
    {
        PathSafety.RejectLinks(_path);
        if (!File.Exists(_path))
            return null;
        if (new FileInfo(_path).Length > 65536)
            throw new IOException("The existing login configuration is not managed by Dowlume.");
        using var reader = XmlReader.Create(_path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null });
        XElement[] nodes = XDocument.Load(reader).Root?.Element("dict")?.Elements().ToArray() ?? [];
        var values = new Dictionary<string, XElement>();
        for (int index = 0; index + 1 < nodes.Length; index += 2)
        {
            if (nodes[index].Name != "key" || !values.TryAdd(nodes[index].Value, nodes[index + 1]))
                throw new IOException("The existing login configuration is not managed by Dowlume.");
        }
        if (nodes.Length % 2 != 0 || !values.TryGetValue("Label", out XElement? label) || label.Value != Label ||
            !values.TryGetValue("DowlumeManagedJob", out XElement? marker) || marker.Value != "1" ||
            !values.TryGetValue("ProgramArguments", out XElement? args) || args.Elements("string").Count() != 1)
            throw new IOException("The existing login configuration is not managed by Dowlume.");
        return args.Element("string")!.Value;
    }

    public void SetEnabled(bool enabled)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("Login startup requires the installed Dowlume application.");
        _ = ReadOwnedExecutable(); // Refuse to replace or remove an unrelated job.
        if (!enabled)
        {
            if (File.Exists(_path))
                File.Delete(_path);
            return;
        }
        PathSafety.RejectLinks(_executable!);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        PathSafety.RejectLinks(_path);
        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null),
            new XElement("plist", new XAttribute("version", "1.0"), new XElement("dict",
                new XElement("key", "Label"), new XElement("string", Label),
                new XElement("key", "DowlumeManagedJob"), new XElement("integer", "1"),
                new XElement("key", "ProgramArguments"), new XElement("array", new XElement("string", _executable)),
                new XElement("key", "RunAtLoad"), new XElement("true"),
                new XElement("key", "ProcessType"), new XElement("string", "Interactive"),
                new XElement("key", "LimitLoadToSessionType"), new XElement("string", "Aqua"))));
        string temporary = _path + "." + Guid.NewGuid().ToString("N");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                document.Save(stream);
                stream.Flush(flushToDisk: true);
            }
            PathSafety.RejectLinks(_path);
            _ = ReadOwnedExecutable();
            File.Move(temporary, _path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}

using System.Text;
using System.Text.RegularExpressions;

namespace Dowlume.Infrastructure;

internal static partial class XdgUserDirectories
{
    // Read a narrowly defined assignment; this file is data and is never sourced by a shell.
    internal static string? ParseDownloads(string contents, string home)
    {
        if (!Path.IsPathFullyQualified(home) || contents.Length > 65536)
            return null;
        string? result = null;
        foreach (string line in contents.Split('\n'))
        {
            Match match = DownloadAssignment().Match(line.TrimEnd('\r'));
            if (!match.Success)
                continue;
            string encoded = match.Groups[1].Value;
            var decoded = new StringBuilder();
            bool valid = true;
            for (int index = 0; index < encoded.Length; index++)
            {
                char value = encoded[index];
                if (value == '\\')
                {
                    if (++index == encoded.Length || encoded[index] is not ('\\' or '"' or '$' or '`'))
                    {
                        valid = false;
                        break;
                    }
                    decoded.Append(encoded[index]);
                }
                else if (value == '$')
                {
                    if (index == 0 && encoded.StartsWith("$HOME/", StringComparison.Ordinal))
                    {
                        decoded.Append(Path.TrimEndingDirectorySeparator(home));
                        index += 4;
                    }
                    else if (encoded == "$HOME")
                    {
                        decoded.Append(home);
                        index += 4;
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }
                else if (value == '`' || char.IsControl(value))
                {
                    valid = false;
                    break;
                }
                else
                    decoded.Append(value);
            }
            if (!valid)
                continue;
            string candidate = decoded.ToString();
            if (Path.IsPathFullyQualified(candidate))
                result = Path.GetFullPath(candidate);
        }
        return result;
    }

    [GeneratedRegex("^\\s*XDG_DOWNLOAD_DIR=\"((?:\\\\.|[^\"\\\\])*)\"\\s*(?:#.*)?$")]
    private static partial Regex DownloadAssignment();
}

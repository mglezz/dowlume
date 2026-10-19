using System.Collections;
using System.Globalization;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using Dowlume.Core;

namespace Dowlume.App.Localization;

public sealed class Localizer : ObservableObject
{
    private static readonly ResourceManager Resources = new("Dowlume.App.Localization.Strings", typeof(Localizer).Assembly);
    private static readonly IReadOnlyDictionary<string, string> EnglishKeys = Resources
        .GetResourceSet(CultureInfo.InvariantCulture, true, true)!.Cast<DictionaryEntry>()
        .ToDictionary(entry => NormalizeNewlines((string)entry.Value!), entry => (string)entry.Key, StringComparer.Ordinal);
    private string _language = "en";

    public string Language => _language;
    public CultureInfo Culture => CultureInfo.GetCultureInfo(_language == "es" ? "es-ES" : "en-US");
    public string this[string key] => Resources.GetString(key, Culture) ?? key;

    public void SetLanguage(string? language)
    {
        string supported = language == "es" ? "es" : "en";
        if (_language == supported)
        {
            return;
        }
        _language = supported;
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged("Item");
        OnPropertyChanged("Item[]");
    }

    // Journal messages remain unchanged on disk; known messages are translated only for display.
    public string Translate(string? english) => english is null ? string.Empty :
        EnglishKeys.TryGetValue(NormalizeNewlines(english), out string? key) ? this[key] : english;

    private static string NormalizeNewlines(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal);

    public string Format(string english, params object[] arguments) => string.Format(Culture, Translate(english), arguments);
    public string Describe(OperationRecord operation) => $"{Translate(operation.Kind.ToString())}: {Path.GetFileName(operation.SourcePath)}";
}

using CommunityToolkit.Mvvm.ComponentModel;
using Dowlume.App.Localization;

namespace Dowlume.App.ViewModels;

// Popup items have their own visual root: translations belong to the item, not a Window ancestor.
public sealed class SettingChoice(object value, Localizer text, string key) : ObservableObject
{
    public object Value { get; } = value;
    public string Label => text[key];
    public void Refresh() => OnPropertyChanged(nameof(Label));
}

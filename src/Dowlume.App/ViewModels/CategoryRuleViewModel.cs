using CommunityToolkit.Mvvm.ComponentModel;
using Dowlume.App.Localization;
using Dowlume.Core;

namespace Dowlume.App.ViewModels;

public partial class CategoryRuleViewModel : ObservableObject
{
    public FileCategory Category
    {
        get;
    }
    private readonly Localizer _text;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(EditorTitle))] private string _folderName;
    public string EditorTitle => string.IsNullOrWhiteSpace(FolderName) ? _text["NewFolder"] : FolderName;
    [ObservableProperty] private string _extensions;
    [ObservableProperty] private int _reviewAfterDays;
    public Guid? Id
    {
        get;
    }
    public bool IsCustom => Id is not null;
    public bool CanChooseMatchKind => IsCustom || Category != FileCategory.Other;
    public object DisplayCategory => IsCustom ? "Custom folder" : Category;
    public static IReadOnlyList<RuleMatchKind> MatchKinds { get; } = Enum.GetValues<RuleMatchKind>();
    public IReadOnlyList<SettingChoice> MatchOptions
    {
        get;
    }
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsExtensionRule)), NotifyPropertyChangedFor(nameof(IsNameRule)), NotifyPropertyChangedFor(nameof(SelectedMatchChoice))]
    private RuleMatchKind _matchKind;
    public SettingChoice? SelectedMatchChoice
    {
        get => MatchOptions.FirstOrDefault(option => Equals(option.Value, MatchKind));
        set
        {
            if (value?.Value is RuleMatchKind kind)
                MatchKind = kind;
        }
    }
    [ObservableProperty] private string _namePattern;
    public bool IsExtensionRule => MatchKind == RuleMatchKind.Extension;
    public bool IsNameRule => !IsExtensionRule;

    public CategoryRuleViewModel(CategoryRule rule, Localizer? text = null)
    {
        text ??= new Localizer();
        _text = text;
        MatchOptions = MatchKinds.Select(kind => new SettingChoice(kind, text, "RuleMatch" + kind)).ToArray();
        Category = rule.Category;
        Id = rule.Id;
        _matchKind = rule.MatchKind;
        _namePattern = rule.NamePattern;
        _folderName = rule.FolderName;
        _extensions = string.Join(", ", rule.Extensions);
        _reviewAfterDays = rule.ReviewAfterDays;
    }

    public void RefreshLabels()
    {
        OnPropertyChanged(nameof(EditorTitle));
        foreach (SettingChoice option in MatchOptions)
            option.Refresh();
    }

    public CategoryRule ToRule() => new(Category, FolderName.Trim().Normalize(), IsExtensionRule ? ParseExtensions(Extensions) : [], ReviewAfterDays)
    {
        Id = Id,
        MatchKind = MatchKind,
        NamePattern = IsNameRule ? NamePattern.Trim().Normalize() : string.Empty
    };

    public static string[] ParseExtensions(string value) => value
        .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(extension => extension.ToLowerInvariant()).ToArray();
}

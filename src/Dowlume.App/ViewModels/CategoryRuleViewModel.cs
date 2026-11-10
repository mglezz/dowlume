using CommunityToolkit.Mvvm.ComponentModel;
using Dowlume.Core;

namespace Dowlume.App.ViewModels;

public partial class CategoryRuleViewModel : ObservableObject
{
    public FileCategory Category
    {
        get;
    }
    [ObservableProperty] private string _folderName;
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
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsExtensionRule)), NotifyPropertyChangedFor(nameof(IsNameRule))]
    private RuleMatchKind _matchKind;
    [ObservableProperty] private string _namePattern;
    public bool IsExtensionRule => MatchKind == RuleMatchKind.Extension;
    public bool IsNameRule => !IsExtensionRule;

    public CategoryRuleViewModel(CategoryRule rule)
    {
        Category = rule.Category;
        Id = rule.Id;
        _matchKind = rule.MatchKind;
        _namePattern = rule.NamePattern;
        _folderName = rule.FolderName;
        _extensions = string.Join(", ", rule.Extensions);
        _reviewAfterDays = rule.ReviewAfterDays;
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

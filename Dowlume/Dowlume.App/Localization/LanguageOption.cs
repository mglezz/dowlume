namespace Dowlume.App.Localization;

public sealed record LanguageOption(string Code, string Name)
{
    public override string ToString() => Name;
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dowlume.App.Services;
using Dowlume.App.ViewModels;
using Dowlume.App.Views;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

[Collection("Desktop")]
public sealed class SettingsNavigationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FirstWindowDoesNotPersistAnUnsetPopupSelection(bool existingProfile)
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(DesktopWorkflowTests.TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            var saved = new AppSettings { DownloadsPath = workspace.Downloads, Theme = "Dark" };
            saved.Categories[0] = saved.Categories[0] with
            {
                MatchKind = RuleMatchKind.NameContains,
                Extensions = [],
                NamePattern = "contract"
            };
            if (existingProfile)
                store.SaveSettings(saved);
            var window = new MainWindow();
            var model = new MainViewModel(store,
                new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness()),
                new DesktopDialogs(window), new DiagnosticLog(Path.Combine(workspace.Root, "log.jsonl")),
                suggestedDownloads: workspace.Downloads);
            window.DataContext = model;
            window.Show();
            try
            {
                await model.RefreshAsync();
                Assert.Equal(string.Empty, model.ErrorMessage);
                Assert.Equal(existingProfile ? "Dark" : "System", model.SelectedTheme);
                window.FindControl<TabControl>("MainTabs")!.SelectedIndex = 4;
                TabControl sections = window.FindControl<TabControl>("SettingsTabs")!;
                sections.SelectedIndex = 2;
                window.UpdateLayout();
                Assert.Same(model.SelectedThemeChoice, window.GetVisualDescendants().OfType<ComboBox>()
                    .Single(combo => ReferenceEquals(combo.ItemsSource, model.ThemeOptions)).SelectedItem);
                sections.SelectedIndex = 0;
                model.Rules[0].IsExpanded = true;
                window.UpdateLayout();
                Assert.Equal(existingProfile ? RuleMatchKind.NameContains : RuleMatchKind.Extension, model.Rules[0].MatchKind);
                Assert.Same(model.Rules[0].SelectedMatchChoice, window.GetVisualDescendants().OfType<ComboBox>()
                    .Single(combo => ReferenceEquals(combo.ItemsSource, model.Rules[0].MatchOptions)).SelectedItem);
                if (existingProfile)
                    Assert.True(saved.HasSameValuesAs(store.LoadSettings()));
                else
                    Assert.Null(store.LoadSettings());
            }
            finally { await DesktopWindowTest.CloseAsync(window); }
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task SettingsStartWithFoldersAndPopupChoicesRemainVisibleAndPersistAcrossLanguages()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(DesktopWorkflowTests.TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads });
            var window = new MainWindow();
            var model = new MainViewModel(store,
                new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness()),
                new DesktopDialogs(window), new DiagnosticLog(Path.Combine(workspace.Root, "log.jsonl")));
            window.DataContext = model;
            window.Width = window.MinWidth;
            window.Height = window.MinHeight;
            window.Show();
            try
            {
                await model.RefreshAsync();
                TabControl main = window.FindControl<TabControl>("MainTabs")!;
                TabControl settings = window.FindControl<TabControl>("SettingsTabs")!;
                main.SelectedIndex = 4;
                window.UpdateLayout();
                Assert.Equal(0, settings.SelectedIndex);
                Assert.All(model.Rules, rule => Assert.False(rule.IsExpanded));
                foreach (string language in new[] { "en", "es" })
                {
                    model.SelectedLanguage = MainViewModel.Languages.Single(item => item.Code == language);
                    foreach (string theme in new[] { "Light", "Dark" })
                    {
                        model.SelectedTheme = theme;
                        for (int section = 0; section < 4; section++)
                        {
                            settings.SelectedIndex = section;
                            window.UpdateLayout();
                            Capture($"settings-{section}-{language}-{theme}", window);
                        }
                        settings.SelectedIndex = 2;
                        window.UpdateLayout();
                        ComboBox themes = window.GetVisualDescendants().OfType<ComboBox>()
                            .Single(combo => ReferenceEquals(combo.ItemsSource, model.ThemeOptions));
                        CheckPopup(themes, model.ThemeOptions, $"theme-popup-{language}-{theme}");
                        themes.SelectedIndex = 2;
                        Assert.Equal("Dark", model.SelectedTheme);
                        Assert.Equal("Dark", store.LoadSettings()!.Theme);
                        model.SelectedTheme = theme;

                        settings.SelectedIndex = 0;
                        model.Rules[0].IsExpanded = true;
                        window.UpdateLayout();
                        ComboBox matches = window.GetVisualDescendants().OfType<ComboBox>()
                            .Single(combo => ReferenceEquals(combo.ItemsSource, model.Rules[0].MatchOptions));
                        CheckPopup(matches, model.Rules[0].MatchOptions, $"rule-popup-{language}-{theme}");
                        matches.SelectedIndex = 1;
                        Assert.Equal(RuleMatchKind.NameContains, model.Rules[0].MatchKind);
                        model.Rules[0].NamePattern = "contract";
                        await model.SaveSettingsCommand.ExecuteAsync(null);
                        Assert.Equal(RuleMatchKind.NameContains, store.LoadSettings()!.Categories[0].MatchKind);
                        model.Rules[0].IsExpanded = false;
                    }
                }
                settings.SelectedIndex = 3;
                main.SelectedIndex = 0;
                main.SelectedIndex = 4;
                window.UpdateLayout();
                Assert.Equal(0, settings.SelectedIndex);
                Assert.Empty(store.GetOperations());
                Assert.False(store.LoadSettings()!.OrganizeFolders);
                Assert.False(store.LoadSettings()!.MonitoringConsentGranted);

                // Simulate a previously confirmed periodic profile; appearance must not stop its monitor.
                model.UsePeriodicOrganization = true;
                store.SaveSettings(store.LoadSettings()! with
                {
                    UsePeriodicOrganization = true,
                    ResumeMonitoringOnStartup = true,
                    MonitoringConsentGranted = true
                });
                await model.ResumeOnStartupAsync();
                Assert.True(model.IsMonitoring, model.ErrorMessage);
                settings.SelectedIndex = 2;
                window.UpdateLayout();
                ComboBox activeTheme = window.GetVisualDescendants().OfType<ComboBox>()
                    .Single(combo => ReferenceEquals(combo.ItemsSource, model.ThemeOptions));
                Assert.True(activeTheme.IsEffectivelyEnabled);
                activeTheme.SelectedIndex = 1;
                Assert.True(model.IsMonitoring);
                Assert.Equal("Light", store.LoadSettings()!.Theme);
                settings.SelectedIndex = 0;
                window.UpdateLayout();
                Assert.All(window.GetVisualDescendants().OfType<TextBox>()
                    .Where(box => box.IsEffectivelyVisible), box => Assert.False(box.IsEffectivelyEnabled));
                Assert.False(window.GetVisualDescendants().OfType<Button>()
                    .Single(button => button.Command == model.SaveSettingsCommand).IsEffectivelyEnabled);
                await model.PauseCommand.ExecuteAsync(null);
            }
            finally { await DesktopWindowTest.CloseAsync(window); }
            return true;
        }, timeout.Token);
    }

    private static void CheckPopup(ComboBox combo, IReadOnlyList<SettingChoice> options, string capture)
    {
        combo.BringIntoView();
        TopLevel.GetTopLevel(combo)!.UpdateLayout();
        combo.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        var popup = combo.GetVisualDescendants().OfType<Popup>().Single();
        Assert.True(popup.IsOpen);
        Control root = popup.Child!;
        root.UpdateLayout();
        foreach (SettingChoice option in options)
        {
            TextBlock label = Assert.Single(root.GetVisualDescendants().OfType<TextBlock>(), block => block.Text == option.Label);
            Assert.True(label.IsEffectivelyVisible);
            Assert.True(label.Bounds.Width > 0);
            Assert.True(label.Bounds.Height > 0);
        }
        Capture(capture, TopLevel.GetTopLevel(root)!);
        combo.IsDropDownOpen = false;
    }

    private static void Capture(string name, TopLevel root)
    {
        using var frame = root.CaptureRenderedFrame();
        Assert.NotNull(frame);
        string? directory = Environment.GetEnvironmentVariable("DOWLUME_SETTINGS_SCREENSHOTS");
        if (directory is null)
            return;
        Directory.CreateDirectory(directory);
        frame.Save(Path.Combine(directory, name + ".png"), Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
    }
}

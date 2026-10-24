using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.VisualTree;
using Dowlume.App.Localization;
using Dowlume.App.Services;
using Dowlume.App.ViewModels;
using Dowlume.App.Views;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

[Collection("Desktop")]
public sealed class LocalizationTests
{
    [Fact]
    public void ResourcesHaveMatchingKeysAndFormatArguments()
    {
        var manager = new ResourceManager("Dowlume.App.Localization.Strings", typeof(Localizer).Assembly);
        var english = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!.Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
        var spanish = manager.GetResourceSet(CultureInfo.GetCultureInfo("es"), true, false)!.Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
        Assert.Equal(english.Keys.Order(), spanish.Keys.Order());
        foreach ((string key, string value) in english)
        {
            Assert.False(string.IsNullOrWhiteSpace(spanish[key]));
            var localized = new Localizer();
            localized.SetLanguage("es");
            Assert.Equal(spanish[key], localized.Translate(value.Replace("\r\n", "\n", StringComparison.Ordinal)));
            Assert.Equal(spanish[key], localized.Translate(value.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\n", "\r\n", StringComparison.Ordinal)));
            Assert.Equal(Regex.Matches(value, @"\{\d+(?::[^}]+)?\}").Select(match => match.Value).Order(),
                Regex.Matches(spanish[key], @"\{\d+(?::[^}]+)?\}").Select(match => match.Value).Order());
        }
        var text = new Localizer();
        text.SetLanguage("es");
        foreach (string name in Enum.GetNames<FileCategory>().Concat(Enum.GetNames<LifecycleStatus>())
                     .Concat(Enum.GetNames<OperationKind>()).Concat(Enum.GetNames<OperationStatus>()))
        {
            Assert.NotEqual(name, text.Translate(name));
        }
    }

    [Fact]
    public void OldProfilesAndUnsupportedLanguagesFallBackToEnglish()
    {
        AppSettings settings = JsonSerializer.Deserialize<AppSettings>("{}")!;
        Assert.Equal("en", settings.Language);
        var text = new Localizer();
        text.SetLanguage("es");
        Assert.Equal("Ajustes", text.Translate("Settings"));
        text.SetLanguage("unknown");
        Assert.Equal("Settings", text.Translate("Settings"));
        Assert.Equal("Historical custom message", text.Translate("Historical custom message"));
    }

    [Fact]
    public async Task LanguageSelectionUpdatesExistingViewsAndTrayAndPreservesFilesAndUnsavedRules()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(DesktopWorkflowTests.TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            var settings = new AppSettings { DownloadsPath = workspace.Downloads };
            store.SaveSettings(settings);
            string source = workspace.CreateFile("report.pdf");
            string hash = FixtureFileMover.Create(workspace).Hash(source);
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
            await organizer.OrganizeAsync(source, settings);
            OperationRecord original = Assert.Single(store.GetOperations());
            var text = new Localizer();
            var window = new MainWindow();
            var model = new MainViewModel(store, organizer, new DesktopDialogs(window, text),
                new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")), localizer: text);
            window.DataContext = model;
            window.Show();
            using var tray = new DesktopTray(Application.Current!, window, text);
            await model.RefreshAsync();
            model.SelectedOperation = Assert.Single(model.Activity);
            model.SelectedActivityKind = model.ActivityKinds.Single(filter => filter.Value == OperationKind.Organize);
            string previewSource = workspace.CreateFile("next.txt");
            await model.PreviewCommand.ExecuteAsync(null);
            model.Rules[0].FolderName = "Unsaved folder";
            model.DownloadsPath = "invalid unsaved path";
            TabControl tabs = window.GetVisualDescendants().OfType<TabControl>().Single();
            tabs.SelectedIndex = 4;
            window.UpdateLayout();
            ComboBox selector = window.GetVisualDescendants().OfType<ComboBox>().Single(combo => combo.IsEffectivelyVisible);
            selector.SelectedItem = MainViewModel.Languages[1];
            window.UpdateLayout();
            Assert.Equal("es", text.Language);
            Assert.Equal("es", store.LoadSettings()!.Language);
            Assert.Equal(settings.DownloadsPath, store.LoadSettings()!.DownloadsPath);
            Assert.Equal("Documents", store.LoadSettings()!.Categories[0].FolderName);
            Assert.Equal("Unsaved folder", model.Rules[0].FolderName);
            Assert.Equal(OperationKind.Organize, model.SelectedActivityKind.Value);
            Assert.Equal(original.Id, model.SelectedOperation?.Id);
            Assert.True(model.HasPreview);
            Assert.Contains("Archivos completos listos: 1", model.StatusMessage);
            Assert.Equal("Abrir Dowlume", ((NativeMenuItem)Assert.Single(TrayIcon.GetIcons(Application.Current!)!).Menu!.Items[0]).Header);
            Assert.Equal(["Resumen", "Vista previa", "Revisión", "Actividad", "Ajustes"],
                tabs.Items.Cast<TabItem>().Select(tab => tab.Header?.ToString() ?? string.Empty).ToArray());
            for (int index = 0; index < 5; index++)
            {
                tabs.SelectedIndex = index;
                window.Width = window.MinWidth;
                window.Height = window.MinHeight;
                window.UpdateLayout();
                using var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
                string? capture = Environment.GetEnvironmentVariable("DOWLUME_LOCALIZATION_SCREENSHOTS");
                if (capture is not null)
                {
                    Directory.CreateDirectory(capture);
                    frame.Save(Path.Combine(capture, $"es-{index}.png"), Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                }
            }
            tabs.SelectedIndex = 3;
            window.UpdateLayout();
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), block => block.Text == "Organizar: report.pdf");
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), block => block.Text == "Completada");
            await model.RecycleCommand.ExecuteAsync(null);
            Assert.Contains("El envío a la papelera está desactivado", model.ErrorMessage);
            model.SelectedLanguage = MainViewModel.Languages[0];
            Assert.Equal(WindowsRecycleBin.DisabledMessage, model.ErrorMessage);
            Assert.Equal("Settings", ((TabItem)tabs.Items[4]!).Header);
            model.SelectedLanguage = MainViewModel.Languages[1];
            Assert.Equal(original, Assert.Single(store.GetOperations()));
            Assert.Equal(hash, FixtureFileMover.Create(workspace).Hash(original.DestinationPath!));
            Assert.True(File.Exists(previewSource));
            var restarted = new MainViewModel(store, organizer, new DesktopDialogs(window),
                new DiagnosticLog(Path.Combine(workspace.Root, "restart.jsonl")));
            Assert.Equal("es", restarted.SelectedLanguage.Code);
            Assert.False(restarted.IsMonitoring);
            Assert.Equal("Documents", restarted.Rules[0].FolderName);
            await restarted.ShutdownAsync();
            await DesktopWindowTest.CloseAsync(window);
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task FailedLanguageSaveRetainsThePreviousPreferenceAndAllowsRetry()
    {
        using var workspace = new TemporaryWorkspace();
        var database = new SqliteStateStore(workspace.Database);
        database.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads, Language = "es" });
        var store = new FailingSettingsStore(database);
        var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness());
        var model = new MainViewModel(store, organizer, new UnusedDialogs(),
            new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")));
        store.RefuseSave = true;
        model.SelectedLanguage = MainViewModel.Languages[0];
        Assert.Equal("es", model.Text.Language);
        Assert.Equal("es", model.SelectedLanguage.Code);
        Assert.Equal("es", database.LoadSettings()!.Language);
        Assert.Contains("Acceso denegado", model.ErrorMessage);
        store.RefuseSave = false;
        model.SelectedLanguage = MainViewModel.Languages[0];
        Assert.Equal("en", database.LoadSettings()!.Language);
        Assert.Equal("en", model.Text.Language);
        Assert.Empty(database.GetOperations());
        await model.ShutdownAsync();
    }

    [Fact]
    public async Task SpanishConfirmationCanBeCanceledWithoutMovingFiles()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(DesktopWorkflowTests.TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            string source = workspace.CreateFile("consent.pdf");
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads });
            var text = new Localizer();
            var window = new MainWindow();
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
            var model = new MainViewModel(store, organizer, new DesktopDialogs(window, text),
                new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")), localizer: text);
            window.DataContext = model;
            window.Show();
            await model.PreviewCommand.ExecuteAsync(null);
            model.SelectedLanguage = MainViewModel.Languages[1];
            Task pending = model.StartCommand.ExecuteAsync(null);
            Window dialog = Assert.Single(window.OwnedWindows);
            dialog.UpdateLayout();
            Assert.Equal("¿Empezar a organizar las descargas?", dialog.Title);
            Assert.Contains(dialog.GetVisualDescendants().OfType<Button>(), button => button.Content as string == "Cancelar");
            Assert.Contains("No se eliminarán archivos automáticamente.", string.Join("\n", dialog.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text)));
            dialog.Close(false);
            await pending;
            Assert.True(File.Exists(source));
            Assert.Empty(store.GetOperations());
            Assert.False(model.IsMonitoring);
            model.Rules[0].FolderName = "CON";
            await model.SaveSettingsCommand.ExecuteAsync(null);
            Assert.Equal("Los nombres de categoría deben ser válidos entre plataformas.", model.ErrorMessage);
            await DesktopWindowTest.CloseAsync(window);
            return true;
        }, timeout.Token);
    }
    private sealed class FailingSettingsStore(IStateStore inner) : IStateStore
    {
        public bool RefuseSave
        {
            get; set;
        }
        public AppSettings? LoadSettings() => inner.LoadSettings();
        public void SaveSettings(AppSettings settings)
        {
            if (RefuseSave)
            {
                throw new UnauthorizedAccessException();
            }
            inner.SaveSettings(settings);
        }
        public IReadOnlyList<TrackedFile> GetFiles() => inner.GetFiles();
        public IReadOnlyList<OperationRecord> GetOperations() => inner.GetOperations();
        public OperationHistoryPage GetOperationHistory(OperationHistoryQuery query) => inner.GetOperationHistory(query);
        public int CountOperationsRequiringAttention() => inner.CountOperationsRequiringAttention();
        public void SaveFile(TrackedFile file) => inner.SaveFile(file);
        public void SaveOperation(OperationRecord operation) => inner.SaveOperation(operation);
        public void CompleteOperation(OperationRecord operation) => inner.CompleteOperation(operation);
    }

    private sealed class UnusedDialogs : IDesktopDialogs
    {
        public Task<string?> ChooseFolderAsync(string title) => throw new InvalidOperationException();
        public Task<bool> ConfirmAsync(string title, string message, string action) => throw new InvalidOperationException();
        public void RevealFile(string path) => throw new InvalidOperationException();
    }

}

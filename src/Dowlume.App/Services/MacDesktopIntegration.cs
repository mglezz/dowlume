using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Dowlume.App.Localization;
using Dowlume.App.Views;

namespace Dowlume.App.Services;

internal sealed class MacDesktopIntegration : IDisposable
{
    private readonly Application _application;
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly IActivatableLifetime? _activation;
    private readonly MainWindow _window;
    private readonly Localizer _text;
    private readonly NativeMenuItem _open, _settings, _dockOpen;
    private readonly Dictionary<NativeMenuItem, string> _systemItems;

    internal MacDesktopIntegration(Application application, IClassicDesktopStyleApplicationLifetime desktop, MainWindow window, Localizer text)
    {
        _application = application;
        _desktop = desktop;
        _window = window;
        _text = text;
        var open = new RelayCommand(ShowWindow);
        _open = new NativeMenuItem { Command = open };
        _dockOpen = new NativeMenuItem { Command = open };
        _settings = new NativeMenuItem
        {
            Gesture = new KeyGesture(Key.OemComma, KeyModifiers.Meta),
            Command = new RelayCommand(() =>
        {
            ShowWindow();
            window.FindControl<TabControl>("MainTabs")!.SelectedIndex = 4;
        })
        };
        // Avalonia exports the application menu before framework initialization completes.
        // Mutate that exported instance; replacing the attached property leaves the old native menu visible.
        var menu = NativeMenu.GetMenu(application)!;
        _systemItems = menu.Items.OfType<NativeMenuItem>().Where(item => item.Header is not null)
            .ToDictionary(item => item, item => item.Header!);
        menu.Items.Insert(0, _open);
        menu.Items.Insert(1, _settings); // Avalonia supplies native Hide, Services and Quit.
        var dock = new NativeMenu();
        dock.Items.Add(_dockOpen);
        NativeDock.SetMenu(application, dock);
        _activation = application.TryGetFeature<IActivatableLifetime>();
        if (_activation is not null)
            _activation.Activated += OnActivated;
        desktop.ShutdownRequested += OnShutdownRequested;
        text.PropertyChanged += OnLanguageChanged;
        UpdateLanguage();
    }

    private void ShowWindow()
    {
        if (_window.IsShutdownComplete)
            return;
        _activation?.TryLeaveBackground();
        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void OnActivated(object? sender, ActivatedEventArgs args)
    {
        if (args.Kind == ActivationKind.Reopen)
            Dispatcher.UIThread.Post(ShowWindow);
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs args)
    {
        if (_window.IsShutdownComplete)
            return;
        args.Cancel = true;
        _window.Close(); // MainWindow drains work and closes dialogs before allowing shutdown.
    }

    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(Localizer.Language))
            UpdateLanguage();
    }

    private void UpdateLanguage()
    {
        _open.Header = _dockOpen.Header = _text.Translate("Open Dowlume");
        _settings.Header = _text.Translate("Settings");
        foreach (var item in _systemItems)
            item.Key.Header = _text.Translate(item.Value);
    }

    public void Dispose()
    {
        _desktop.ShutdownRequested -= OnShutdownRequested;
        if (_activation is not null)
            _activation.Activated -= OnActivated;
        _text.PropertyChanged -= OnLanguageChanged;
        NativeMenu.SetMenu(_application, null);
        NativeDock.SetMenu(_application, null);
    }
}

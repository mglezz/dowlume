using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using Dowlume.App.Localization;

namespace Dowlume.App.Services;

public sealed class DesktopTray : IDisposable
{
    private readonly Application _application;
    private readonly TrayIcon _icon;

    private readonly Localizer _text;
    private readonly NativeMenuItem _openItem;
    private readonly NativeMenuItem _exitItem;

    public DesktopTray(Application application, Window window, Localizer? localizer = null)
    {
        _application = application;
        _text = localizer ?? new Localizer();
        var open = new RelayCommand(() =>
        {
            window.Show();
            window.WindowState = WindowState.Normal;
            window.Activate();
        });
        var menu = new NativeMenu();
        _openItem = new NativeMenuItem { Command = open };
        menu.Items.Add(_openItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        _exitItem = new NativeMenuItem { Command = new RelayCommand(window.Close) };
        menu.Items.Add(_exitItem);
        using Stream iconStream = AssetLoader.Open(new Uri("avares://Dowlume.App/Assets/dowlume.ico"));
        _icon = new TrayIcon
        {
            Icon = new WindowIcon(iconStream),
            Menu = menu,
            Command = open,
            IsVisible = true
        };
        UpdateLanguage();
        _text.PropertyChanged += OnLanguageChanged;
        // Avalonia creates the native icon only when it belongs to this collection.
        TrayIcon.SetIcons(application, new TrayIcons { _icon });
    }

    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(Localizer.Language))
        {
            UpdateLanguage();
        }
    }

    private void UpdateLanguage()
    {
        _openItem.Header = _text.Translate("Open Dowlume");
        _exitItem.Header = _text.Translate("Exit");
        _icon.ToolTipText = _text.Translate("Dowlume — your downloads, under control");
    }

    public void Dispose()
    {
        _text.PropertyChanged -= OnLanguageChanged;
        TrayIcon.SetIcons(_application, null);
        _icon.Dispose();
    }
}

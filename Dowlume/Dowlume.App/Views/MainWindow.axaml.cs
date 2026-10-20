using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Dowlume.App.ViewModels;

namespace Dowlume.App.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromSeconds(3) };
    private bool _closing;
    private bool _canClose;
    private bool _canHideToTray;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _refreshTimer.Tick += async (_, _) =>
        {
            if (DataContext is MainViewModel model)
            {
                await model.RefreshAsync(skipIfRefreshing: true);
            }
        };
        Opened += async (_, _) =>
        {
            if (DataContext is MainViewModel model)
            {
                await model.RefreshAsync();
            }
            _refreshTimer.Start();
        };
        Closing += OnClosing;
    }

    public void ConfigureTray(bool available)
    {
        this.FindControl<Button>("HideToTrayButton")!.IsVisible = available;
        _canHideToTray = available;
    }

    private void OnHideToTray(object? sender, RoutedEventArgs args)
    {
        if (_canHideToTray)
            Hide();
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs args)
    {
        if (_canClose)
        {
            return;
        }
        args.Cancel = true;
        if (_closing)
        {
            return;
        }
        _closing = true;
        // Closing a pending confirmation returns its default false result before shutdown waits for it.
        foreach (Window dialog in OwnedWindows.ToArray())
        {
            dialog.Close();
        }
        IsEnabled = false;
        _refreshTimer.Stop();
        if (DataContext is MainViewModel model)
        {
            await model.ShutdownAsync();
        }
        _canClose = true;
        Close();
    }
}

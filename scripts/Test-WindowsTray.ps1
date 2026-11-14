function Test-WindowsLoginPresentation([string]$Executable, [string]$Profile) {
    # Test-WindowsTray initializes the native tray probe first. Only this disposable
    # package process/profile is used; no real sign-out or personal profile is needed.
    $launch = 0
    foreach ($showWindow in @($false, $true, $false)) {
        $process = Start-Process -FilePath $Executable -ArgumentList @('--login-startup', '--data-directory', """$Profile""") -WindowStyle Hidden -PassThru
        try {
            $deadline = [DateTime]::UtcNow.AddSeconds(30)
            do {
                Start-Sleep -Milliseconds 100
                $process.Refresh()
                if ($process.HasExited) { throw 'Login startup exited unexpectedly.' }
                $icon = [DowlumeTrayAcceptance]::FindIconWindow([uint32]$process.Id)
            } while ($icon -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline)
            if ($icon -eq [IntPtr]::Zero) { throw 'Login startup did not register a tray icon.' }
            Start-Sleep -Milliseconds 1500
            $process.Refresh()
            if ($showWindow) {
                if ($process.MainWindowHandle -eq [IntPtr]::Zero) { throw 'The saved visible-login preference was ignored.' }
            } else {
                if ($process.MainWindowHandle -ne [IntPtr]::Zero) { throw 'Background login startup displayed a window.' }
                if (-not [DowlumeTrayAcceptance]::ClickIcon($icon)) { throw 'Could not reopen background startup from the tray.' }
            }
            $deadline = [DateTime]::UtcNow.AddSeconds(10)
            do {
                Start-Sleep -Milliseconds 100
                $process.Refresh()
            } while ($process.MainWindowHandle -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline)
            if ($process.MainWindowHandle -eq [IntPtr]::Zero) { throw 'Login window did not become accessible.' }
            $root = [Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
            $condition = New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::NameProperty, 'Settings')
            $tab = $root.FindFirst([Windows.Automation.TreeScope]::Descendants, $condition)
            if ($null -eq $tab) { throw 'Settings tab is unavailable.' }
            $tab.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
            $condition = New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::AutomationIdProperty, 'SettingsApplicationTab')
            $deadline = [DateTime]::UtcNow.AddSeconds(10)
            do {
                Start-Sleep -Milliseconds 100
                $applicationTab = $root.FindFirst([Windows.Automation.TreeScope]::Descendants, $condition)
            } while ($null -eq $applicationTab -and [DateTime]::UtcNow -lt $deadline)
            if ($null -eq $applicationTab) { throw 'Application settings tab is unavailable.' }
            $applicationTab.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
            $condition = New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::AutomationIdProperty, 'ShowWindowAtWindowsLogin')
            $deadline = [DateTime]::UtcNow.AddSeconds(10)
            do {
                Start-Sleep -Milliseconds 100
                $checkbox = $root.FindFirst([Windows.Automation.TreeScope]::Descendants, $condition)
            } while ($null -eq $checkbox -and [DateTime]::UtcNow -lt $deadline)
            if ($null -eq $checkbox) { throw 'Windows login preference is unavailable.' }
            $toggle = $checkbox.GetCurrentPattern([Windows.Automation.TogglePattern]::Pattern)
            $expected = if ($showWindow) { [Windows.Automation.ToggleState]::On } else { [Windows.Automation.ToggleState]::Off }
            if ($toggle.Current.ToggleState -ne $expected) { throw 'Login preference did not persist.' }
            # The first two launches verify off -> on -> off across real restarts.
            if ($launch -lt 2) {
                $toggle.Toggle()
                $deadline = [DateTime]::UtcNow.AddSeconds(10)
                while ($toggle.Current.ToggleState -eq $expected -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 100 }
                if ($toggle.Current.ToggleState -eq $expected) { throw 'Login preference could not be changed.' }
            }
            if (-not $process.CloseMainWindow() -or -not $process.WaitForExit(30000) -or $process.ExitCode -ne 0) {
                throw 'Login startup did not shut down gracefully.'
            }
        } finally {
            if (-not $process.HasExited) { $process.Kill(); $process.WaitForExit() }
            $process.Dispose()
        }
        $launch++
    }
    Write-Output 'Background default, tray reopen, visible preference and restart persistence passed.'
}

function Test-WindowsTray([Diagnostics.Process]$Application) {
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    if (-not ('DowlumeTrayAcceptance' -as [type])) {
        Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

public static class DowlumeTrayAcceptance
{
    [StructLayout(LayoutKind.Sequential)]
    private struct IconIdentifier
    {
        public uint Size;
        public IntPtr Window;
        public uint Id;
        public Guid Guid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rectangle { public int Left, Top, Right, Bottom; }

    private delegate bool WindowCallback(IntPtr window, IntPtr parameter);

    public static IntPtr FindIconWindow(uint processId)
    {
        IntPtr found = IntPtr.Zero;
        WindowCallback callback = (window, parameter) =>
        {
            uint owner;
            GetWindowThreadProcessId(window, out owner);
            if (owner != processId) return true;
            // Avalonia assigns the first tray icon ID 1 within each application process.
            var icon = new IconIdentifier { Size = (uint)Marshal.SizeOf(typeof(IconIdentifier)), Window = window, Id = 1 };
            Rectangle rectangle;
            if (Shell_NotifyIconGetRect(ref icon, out rectangle) != 0) return true;
            found = window;
            return false;
        };
        EnumWindows(callback, IntPtr.Zero);
        GC.KeepAlive(callback);
        return found;
    }

    public static bool ClickIcon(IntPtr window) { return PostMessage(window, 0x0400 + 1024, new IntPtr(1), new IntPtr(0x0202)); }
    public static void Minimize(IntPtr window) { ShowWindow(window, 6); }

    [DllImport("user32.dll")] private static extern bool EnumWindows(WindowCallback callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("shell32.dll")] private static extern int Shell_NotifyIconGetRect(ref IconIdentifier icon, out Rectangle rectangle);
}
'@
    }
    $Application.Refresh()
    $window = $Application.MainWindowHandle
    if ($window -eq [IntPtr]::Zero -or $Application.MainWindowTitle -ne 'Dowlume') {
        throw 'Tray acceptance requires the disposable Dowlume main window.'
    }
    $iconWindow = [DowlumeTrayAcceptance]::FindIconWindow([uint32]$Application.Id)
    if ($iconWindow -eq [IntPtr]::Zero) { throw 'Windows has no registered Dowlume notification-area icon.' }
    $automation = [Windows.Automation.AutomationElement]::FromHandle($window)
    $condition = New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::NameProperty, 'Hide to tray')
    $hide = $automation.FindFirst([Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $hide) { throw 'The Hide to tray button is unavailable to Windows UI Automation.' }
    foreach ($minimized in @($false, $true)) {
        if ($minimized) { [DowlumeTrayAcceptance]::Minimize($window) }
        $hide.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        $deadline = [DateTime]::UtcNow.AddSeconds(5)
        while ([DowlumeTrayAcceptance]::IsWindowVisible($window) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 50 }
        if ([DowlumeTrayAcceptance]::IsWindowVisible($window)) { throw 'Hide to tray did not hide the window.' }
        if (-not [DowlumeTrayAcceptance]::ClickIcon($iconWindow)) { throw 'Could not deliver the tray click.' }
        $deadline = [DateTime]::UtcNow.AddSeconds(5)
        while ((-not [DowlumeTrayAcceptance]::IsWindowVisible($window) -or [DowlumeTrayAcceptance]::IsIconic($window)) -and [DateTime]::UtcNow -lt $deadline) {
            Start-Sleep -Milliseconds 50
        }
        if (-not [DowlumeTrayAcceptance]::IsWindowVisible($window) -or [DowlumeTrayAcceptance]::IsIconic($window)) {
            throw "The registered tray icon did not restore a normal window (minimized test: $minimized; visible: $([DowlumeTrayAcceptance]::IsWindowVisible($window)); minimized: $([DowlumeTrayAcceptance]::IsIconic($window)))."
        }
    }
    Write-Output 'Native notification-area registration and hidden/minimized window restoration passed.'
}

# UI Automation checks against the actual packaged application and its isolated profile.
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

function Assert-LanguageFiles([string]$Directory) {
    foreach ($file in @('es/Dowlume.App.resources.dll', 'START-HERE.txt', 'START-HERE.es.txt')) {
        if (-not (Test-Path -LiteralPath (Join-Path $Directory $file) -PathType Leaf)) {
            throw "Missing packaged language file: $file"
        }
    }
}

function Find-LanguageElement([Diagnostics.Process]$Application, [string]$Name, [switch]$ById) {
    $property = if ($ById) { [Windows.Automation.AutomationElement]::AutomationIdProperty } else { [Windows.Automation.AutomationElement]::NameProperty }
    $condition = New-Object Windows.Automation.PropertyCondition($property, $Name)
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $Application.Refresh()
        if ($Application.HasExited) { throw 'The application exited during language acceptance.' }
        $window = [Windows.Automation.AutomationElement]::FromHandle($Application.MainWindowHandle)
        $element = $window.FindFirst([Windows.Automation.TreeScope]::Descendants, $condition)
        if ($null -ne $element) { return $element }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "The packaged UI did not expose the expected language element: $Name"
}

function Assert-TestLanguage([Diagnostics.Process]$Application, [string]$Language) {
    $settingsName = if ($Language -eq 'Español') { 'Ajustes' } else { 'Settings' }
    $overviewName = if ($Language -eq 'Español') { 'Resumen' } else { 'Overview' }
    $null = Find-LanguageElement $Application $overviewName
    $settings = Find-LanguageElement $Application $settingsName
    $settings.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $selector = Find-LanguageElement $Application 'LanguageSelector' -ById
    $selected = $selector.GetCurrentPattern([Windows.Automation.SelectionPattern]::Pattern).Current.GetSelection()
    if ($selected.Count -ne 1 -or $selected[0].Current.Name -ne $Language) {
        throw "The language selector did not retain $Language."
    }
}

function Set-TestLanguage([Diagnostics.Process]$Application, [string]$From, [string]$To) {
    Assert-TestLanguage $Application $From
    $selector = Find-LanguageElement $Application 'LanguageSelector' -ById
    $selector.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    # Avalonia can expose the popup in a separate native window; constrain the search to this process.
    $processCondition = New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::ProcessIdProperty, $Application.Id)
    $nameCondition = New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::NameProperty, $To)
    $selectableCondition = New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::IsSelectionItemPatternAvailableProperty, $true)
    $optionCondition = New-Object Windows.Automation.AndCondition($nameCondition, $selectableCondition)
    $condition = New-Object Windows.Automation.AndCondition($processCondition, $optionCondition)
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $option = [Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Descendants, $condition)
        if ($null -ne $option) { break }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    if ($null -eq $option) { throw "The language option is unavailable: $To" }
    $option.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Assert-TestLanguage $Application $To
}

function Resolve-DocumentationLinks {
    param(
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][ValidatePattern('^[0-9a-f]{40}$')][string]$SourceCommit
    )

    $documentDirectory = Split-Path ([IO.Path]::GetFullPath($SourcePath)) -Parent
    $rootPrefix = [IO.Path]::GetFullPath($RepositoryRoot).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    $content = [IO.File]::ReadAllText($SourcePath)
    # Standalone release notes and acceptance kits have no repository-relative tree.
    return [regex]::Replace($content, '\]\(([^\s)]+)\)', {
        param($match)
        $target = $match.Groups[1].Value
        if ($target -match '^(?:[a-zA-Z][a-zA-Z0-9+.-]*:|//|#)') { return $match.Value }
        $parts = $target -split '#', 2
        $resolved = [IO.Path]::GetFullPath((Join-Path $documentDirectory ([Uri]::UnescapeDataString($parts[0]))))
        if (-not $resolved.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'A documentation link escapes the repository.'
        }
        if (-not (Test-Path -LiteralPath $resolved)) { throw "Missing documentation link: $target" }
        $relative = $resolved.Substring($rootPrefix.Length).Replace('\', '/')
        $escaped = ($relative.Split('/') | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/'
        $fragment = if ($parts.Count -gt 1) { '#' + $parts[1] } else { '' }
        return "](https://github.com/mglezz/dowlume/blob/$SourceCommit/$escaped$fragment)"
    })
}

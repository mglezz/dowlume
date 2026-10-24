$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Resolve-DocumentationLinks.ps1')
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$revision = 'a' * 40

# Exercise actual standalone package inputs, including parent-relative translations.
foreach ($relative in @('docs/releases/0.4.0.md', 'docs/es/releases/0.4.0.md', 'docs/development/windows-acceptance.md')) {
    $result = Resolve-DocumentationLinks -SourcePath (Join-Path $repositoryRoot $relative) -RepositoryRoot $repositoryRoot -SourceCommit $revision
    $unresolved = [regex]::Matches($result, '\]\((?![a-zA-Z][a-zA-Z0-9+.-]*:|//|#)[^\s)]+\)')
    if ($unresolved.Count -gt 0) { throw "Unresolved standalone links in $relative" }
    if (-not $result.Contains("https://github.com/mglezz/dowlume/blob/$revision/")) {
        throw "Revision-specific documentation links are missing from $relative"
    }
}
$notes = Resolve-DocumentationLinks -SourcePath (Join-Path $repositoryRoot 'docs/releases/0.4.0.md') -RepositoryRoot $repositoryRoot -SourceCommit $revision
if (-not $notes.Contains("/blob/$revision/docs/es/releases/0.4.0.md") -or
    -not $notes.Contains("/blob/$revision/docs/user/guide.md#macos-040") -or
    -not $notes.Contains('https://github.com/mglezz/dowlume/releases/tag/v0.4.0')) {
    throw 'Standalone notes lost a translation link, fragment, or external release URL.'
}
Write-Output 'Standalone package documentation links passed.'

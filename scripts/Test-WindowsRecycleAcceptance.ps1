param(
    [string]$WorkerDirectory = (Join-Path $PSScriptRoot 'recycle-worker'),
    [string]$WorkerArchive = '',
    [string]$WorkerArchiveSha256 = ''
)

throw 'Native recycling probes are retired for v0.1.0. Preserve the evidence VM and checkpoints. Do not rerun these tests.'

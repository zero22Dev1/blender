[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Feature,

    [ValidatePattern('^[a-z0-9]+(?:-[a-z0-9]+)*$')]
    [string]$Slug,

    [string]$BaseRef = 'HEAD',

    [string]$WorktreeParent,

    [switch]$AllowDirtySource,

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & git @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($exitCode -ne 0) {
        $message = ($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
        throw "git command failed: git $($Arguments -join ' ')`n$message"
    }

    return $output
}

function Get-TrimmedOutput {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Value
    )

    return (($Value | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ }) -join '')
}

function Resolve-FeatureSlug {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FeatureName,

        [string]$ExplicitSlug
    )

    if (-not [string]::IsNullOrWhiteSpace($ExplicitSlug)) {
        return $ExplicitSlug.Trim().ToLowerInvariant()
    }

    $slug = $FeatureName.Trim().ToLowerInvariant()
    $slug = [regex]::Replace($slug, '[^a-z0-9]+', '-')
    $slug = $slug.Trim('-')

    if ($slug -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') {
        throw "Feature name '$FeatureName' does not produce an ASCII slug. Specify -Slug using lowercase ASCII letters, numbers, and hyphens, for example -Slug 'user-authentication'."
    }

    return $slug
}

$workspace = (Get-Location).Path
$repoRoot = $null

try {
    # --show-toplevel may be decoded incorrectly by Windows PowerShell 5.1
    # when the path contains non-ASCII characters. --show-cdup is a relative
    # ASCII path, so resolve the absolute path locally instead.
    $repoRelativePath = Get-TrimmedOutput -Value (Invoke-Git -Arguments @('-C', $workspace, 'rev-parse', '--show-cdup'))
    if ([string]::IsNullOrWhiteSpace($repoRelativePath)) {
        $repoRoot = [System.IO.Path]::GetFullPath($workspace)
    }
    else {
        $repoRoot = [System.IO.Path]::GetFullPath((Join-Path $workspace $repoRelativePath))
    }
}
catch {
    throw "The current directory is not inside a Git repository. Clone a repository or initialize one with 'git init' after user confirmation."
}

$isBare = Get-TrimmedOutput -Value (Invoke-Git -Arguments @('-C', $repoRoot, 'rev-parse', '--is-bare-repository'))
if ($isBare -eq 'true') {
    throw "Bare repositories cannot be used as the source worktree. Open a non-bare checkout."
}

$sourceStatus = @(Invoke-Git -Arguments @('-C', $repoRoot, 'status', '--porcelain'))
if ($sourceStatus.Count -gt 0 -and -not $AllowDirtySource) {
    throw "The source worktree has uncommitted changes. Commit or stash them first, or rerun with -AllowDirtySource after confirming that only committed files will be used."
}

$slug = Resolve-FeatureSlug -FeatureName $Feature -ExplicitSlug $Slug
$branchName = "feature/$slug"
$repoName = Split-Path -Leaf $repoRoot.TrimEnd('\', '/')

if ([string]::IsNullOrWhiteSpace($WorktreeParent)) {
    $WorktreeParent = Join-Path (Split-Path -Parent $repoRoot) "$repoName-worktrees"
}

$worktreeParentFullPath = [System.IO.Path]::GetFullPath($WorktreeParent)
$worktreePath = Join-Path $worktreeParentFullPath $slug

Invoke-Git -Arguments @('-C', $repoRoot, 'check-ref-format', '--branch', $branchName) | Out-Null
Invoke-Git -Arguments @('-C', $repoRoot, 'rev-parse', '--verify', $BaseRef) | Out-Null

$branchExists = $false
& git -C $repoRoot show-ref --verify --quiet "refs/heads/$branchName"
if ($LASTEXITCODE -eq 0) {
    $branchExists = $true
}

$worktreeAlreadyRegistered = $false
$worktreeList = Invoke-Git -Arguments @('-C', $repoRoot, 'worktree', 'list', '--porcelain')
if (($worktreeList -join [Environment]::NewLine) -match "(?m)^branch refs/heads/$([regex]::Escape($branchName))$") {
    $worktreeAlreadyRegistered = $true
}

if (Test-Path -LiteralPath $worktreePath) {
    throw "The target worktree path already exists: $worktreePath"
}

if ($worktreeAlreadyRegistered) {
    throw "The branch is already checked out by another worktree: $branchName"
}

$operation = if ($branchExists) {
    "git -C `"$repoRoot`" worktree add `"$worktreePath`" $branchName"
}
else {
    "git -C `"$repoRoot`" worktree add -b $branchName `"$worktreePath`" $BaseRef"
}

if ($DryRun) {
    [pscustomobject]@{
        dryRun       = $true
        feature      = $Feature
        slug         = $slug
        branch       = $branchName
        baseRef      = $BaseRef
        repository   = $repoRoot
        worktreePath = $worktreePath
        command      = $operation
    } | ConvertTo-Json -Depth 3
    exit 0
}

if (-not (Test-Path -LiteralPath $worktreeParentFullPath)) {
    New-Item -ItemType Directory -Path $worktreeParentFullPath -Force | Out-Null
}

if ($branchExists) {
    Invoke-Git -Arguments @('-C', $repoRoot, 'worktree', 'add', $worktreePath, $branchName) | Out-Null
}
else {
    Invoke-Git -Arguments @('-C', $repoRoot, 'worktree', 'add', '-b', $branchName, $worktreePath, $BaseRef) | Out-Null
}

[pscustomobject]@{
    dryRun       = $false
    feature      = $Feature
    slug         = $slug
    branch       = $branchName
    baseRef      = $BaseRef
    repository   = $repoRoot
    worktreePath = $worktreePath
} | ConvertTo-Json -Depth 3
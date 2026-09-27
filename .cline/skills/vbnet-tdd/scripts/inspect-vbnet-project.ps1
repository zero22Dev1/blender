[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Workspace,

    [switch]$Json
)

$ErrorActionPreference = 'Stop'

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,

        [Parameter(Mandatory = $true)]
        [string]$TargetPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath).TrimEnd('\') + '\'
    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri($baseFullPath)
    $targetUri = New-Object System.Uri($targetFullPath)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', '\')
}

function Get-ProjectXmlValue {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Xml,

        [Parameter(Mandatory = $true)]
        [string]$LocalName
    )

    $node = $Xml.SelectSingleNode("//*[local-name()='$LocalName']")
    if ($null -eq $node) {
        return $null
    }

    return $node.InnerText.Trim()
}

function Test-ProjectIsSdkStyle {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Xml
    )

    $projectNode = $Xml.SelectSingleNode("/*[local-name()='Project']")
    if ($null -eq $projectNode) {
        return $false
    }

    return -not [string]::IsNullOrWhiteSpace($projectNode.GetAttribute('Sdk'))
}

function Get-PackageIds {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Xml
    )

    $nodes = $Xml.SelectNodes("//*[local-name()='PackageReference']")
    $ids = @()
    foreach ($node in $nodes) {
        $include = $node.GetAttribute('Include')
        if ([string]::IsNullOrWhiteSpace($include)) {
            $include = $node.GetAttribute('Update')
        }

        if (-not [string]::IsNullOrWhiteSpace($include)) {
            $ids += $include
        }
    }

    return @($ids | Sort-Object -Unique)
}

function Test-IsTestProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectName,

        [AllowEmptyCollection()]
        [Parameter(Mandatory = $true)]
        [string[]]$PackageIds,

        [string]$IsTestProjectValue
    )

    if ($IsTestProjectValue -eq 'true') {
        return $true
    }

    if ($ProjectName -match '(?i)(\.tests?|test)$') {
        return $true
    }

    foreach ($packageId in $PackageIds) {
        if ($packageId -match '(?i)(mstest|nunit|xunit|test\.sdk|testadapter)') {
            return $true
        }
    }

    return $false
}

$workspacePath = [System.IO.Path]::GetFullPath($Workspace)
if (-not (Test-Path -LiteralPath $workspacePath -PathType Container)) {
    throw "Workspace directory does not exist: $workspacePath"
}

$solutions = @(Get-ChildItem -LiteralPath $workspacePath -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -in @('.sln', '.slnx') } | Sort-Object FullName)
$projects = @(Get-ChildItem -LiteralPath $workspacePath -Recurse -File -Filter '*.vbproj' -ErrorAction SilentlyContinue | Sort-Object FullName)

$projectResults = @()
foreach ($project in $projects) {
    try {
        $xml = [xml](Get-Content -LiteralPath $project.FullName -Raw -Encoding UTF8)
    }
    catch {
        $projectResults += [pscustomobject]@{
            path = Get-RelativePath -BasePath $workspacePath -TargetPath $project.FullName
            name = $project.BaseName
            parseError = $_.Exception.Message
        }
        continue
    }

    $packageIds = @(Get-PackageIds -Xml $xml)
    $targetFrameworks = @()
    foreach ($propertyName in @('TargetFramework', 'TargetFrameworks', 'TargetFrameworkVersion')) {
        $value = Get-ProjectXmlValue -Xml $xml -LocalName $propertyName
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            $targetFrameworks += $value -split ';'
        }
    }

    $projectReferences = @(
        $xml.SelectNodes("//*[local-name()='ProjectReference']") |
            ForEach-Object { $_.GetAttribute('Include') } |
            Where-Object { $_ }
    )

    $projectResults += [pscustomobject]@{
        path = Get-RelativePath -BasePath $workspacePath -TargetPath $project.FullName
        name = $project.BaseName
        sdkStyle = Test-ProjectIsSdkStyle -Xml $xml
        targetFrameworks = @($targetFrameworks | Where-Object { $_ } | Sort-Object -Unique)
        isTestProject = Test-IsTestProject -ProjectName $project.BaseName -PackageIds $packageIds -IsTestProjectValue (Get-ProjectXmlValue -Xml $xml -LocalName 'IsTestProject')
        packageReferences = $packageIds
        projectReferences = $projectReferences
        parseError = $null
    }
}

$frameworks = @($projectResults | ForEach-Object { $_.targetFrameworks } | Where-Object { $_ } | Sort-Object -Unique)
$testProjects = @($projectResults | Where-Object { $_.isTestProject })
$testPackages = @(
    $projectResults |
        ForEach-Object { $_.packageReferences } |
        Where-Object { $_ -match '(?i)(mstest|nunit|xunit|test\.sdk|testadapter)' } |
        Sort-Object -Unique
)

$result = [pscustomobject]@{
    workspace = $workspacePath
    solutions = @($solutions | ForEach-Object { Get-RelativePath -BasePath $workspacePath -TargetPath $_.FullName })
    visualBasicProjects = $projectResults
    testProjects = @($testProjects | ForEach-Object { $_.path })
    detectedTargetFrameworks = $frameworks
    detectedTestPackages = $testPackages
    recommendedInspection = @(
        'Prefer the existing test project and test framework.',
        'Use dotnet test for SDK-style .NET projects; consider Test Explorer or vstest.console.exe for legacy .NET Framework projects.',
        'Before implementation, add one Red test and verify that it fails for the expected reason.'
    )
}

if ($Json) {
    $result | ConvertTo-Json -Depth 8
}
else {
    Write-Output "Workspace: $($result.workspace)"
    Write-Output "Solutions: $($result.solutions -join ', ')"
    Write-Output "VB projects: $($result.visualBasicProjects.Count)"
    foreach ($project in $result.visualBasicProjects) {
        Write-Output "- $($project.path) | SDK=$($project.sdkStyle) | Test=$($project.isTestProject) | TFM=$($project.targetFrameworks -join ',') | Packages=$($project.packageReferences -join ',')"
    }
    Write-Output "Test projects: $($result.testProjects -join ', ')"
    Write-Output "Detected test packages: $($result.detectedTestPackages -join ', ')"
}
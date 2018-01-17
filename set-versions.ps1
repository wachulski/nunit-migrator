<#
.SYNOPSIS
    Replaces version properties of the VSIX project manifest and csprojs.
	This must be done before the VSIX is built as the manifest is packaged into the VSIX.
.EXAMPLE
    ./set-versions.ps1 2.0.7-alpha72743 2.0.7.72743
#>

param(
    [Parameter(Mandatory=$true,Position=1)][string]$semVer,
    [Parameter(Mandatory=$true,Position=2)][string]$dllVer
)

if (!$dllVer) {
    $dllVer = $semVer + '.0'
}

function Replace-Version($filePath, $findPattern, $value) {
    (Get-Content $filePath) -replace $findPattern, $value | Out-File $filePath -Encoding "UTF8"
}

Replace-Version "$PSScriptRoot/nunit.migrator.vsix/source.extension.vsixmanifest" `
    'Version="0\.0\.0\.0"' "Version=""$dllVer"""

Replace-Version "$PSScriptRoot/nunit.migrator/nunit.migrator.csproj" `
    '<Version>.*</Version>' "<Version>$dllVer</Version>"

Replace-Version "$PSScriptRoot/nunit.migrator/nunit.migrator.csproj" `
    '<PackageVersion>.*</PackageVersion>' "<PackageVersion>$semVer</PackageVersion>"
<#
.SYNOPSIS
    Replaces version properties of the VSIX project manifest and csprojs.
	This must be done before the VSIX is built as the manifest is packaged into the VSIX.
.EXAMPLE
    ./set-versions.ps1 2.0.7.72743
#>

param(
    [Parameter(Mandatory=$true,Position=1)]
    [string]$version
)

$path = './nunit.migrator.vsix/source.extension.vsixmanifest'
(Get-Content $path) -replace 'Version="0\.0\.0\.0"', "Version=""$version""" | Out-File $path -Encoding "UTF8"

$path = './nunit.migrator/nunit.migrator.csproj'
(Get-Content $path) -replace '<Version>.*</Version>', "<Version>$version</Version>" | Out-File $path -Encoding "UTF8"
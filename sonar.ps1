<#
.SYNOPSIS
    Initializes or finalizes SonarQube scanner for MSBuild to be executed in between
.EXAMPLE
    ./sonar.ps1 begin YOUR_SONAR_AUTH_TOKEN 3.1.0.14792
    ./sonar.ps1 end YOUR_SONAR_AUTH_TOKEN
#>

param(
    [Parameter(Mandatory=$true,Position=1)]
    [string]$step,
    [Parameter(Mandatory=$true,Position=2)]
    [string]$sonarLogin,
    [Parameter(Mandatory=$false,Position=3)]
    [string]$version = 'unknown-version'
)

if ($step -eq 'begin') {
    SonarQube.Scanner.MSBuild begin `
    /k:MarWac_NUnit_Migrator `
    /n:"nunit.migrator" `
    /v:"$version" `
    /d:"sonar.host.url=https://sonarqube.com" `
    /d:"sonar.organization=wachulski-github" `
    /d:"sonar.login=$sonarLogin" `
    /d:"sonar.cs.dotcover.reportsPaths=dotCover.html"
}
elseif ($step -eq 'end') {
    SonarQube.Scanner.MSBuild end /d:"sonar.login=$sonarLogin"
} else {
    throw "Invalid step provided ($step). Valid are: 'begin', 'end'"
}
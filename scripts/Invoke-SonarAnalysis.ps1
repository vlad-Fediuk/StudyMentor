param(
    [string]$ServerUrl = 'http://localhost:9000',
    [string]$ProjectKey = 'study-mentor',
    [string]$Label = 'baseline'
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    if (-not $env:SONAR_TOKEN) { throw 'Set SONAR_TOKEN in this PowerShell session before running the analysis.' }
    dotnet tool restore
    if ($LASTEXITCODE) { throw 'Tool restore failed.' }
    dotnet sonarscanner begin "/k:$ProjectKey" "/v:$Label" "/d:sonar.host.url=$ServerUrl" "/d:sonar.token=$env:SONAR_TOKEN" '/d:sonar.exclusions=**/Migrations/**,**/bin/**,**/obj/**' '/d:sonar.coverage.exclusions=**/Migrations/**,**/Program.cs' '/d:sonar.cs.opencover.reportsPaths=StudyMentor.QualityTests/TestResults/**/coverage.opencover.xml'
    if ($LASTEXITCODE) { throw 'Scanner begin failed.' }
    dotnet build StudyMentor.sln --no-incremental
    if ($LASTEXITCODE) { throw 'Build failed.' }
    dotnet test StudyMentor.sln --no-build --settings coverage.runsettings --collect:'XPlat Code Coverage'
    if ($LASTEXITCODE) { throw 'Tests failed.' }
    dotnet sonarscanner end "/d:sonar.token=$env:SONAR_TOKEN"
    if ($LASTEXITCODE) { throw 'Scanner end failed.' }
} finally { Pop-Location }

param(
    [string]$ServerUrl = 'http://localhost:9000',
    [string]$ProjectKey = 'study-mentor',
    [string]$Label = 'baseline'
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    if (-not $env:SONAR_TOKEN) { throw 'Set SONAR_TOKEN in this PowerShell session before running the analysis.' }
    $results = 'StudyMentor.QualityTests/TestResults/sonar-' + [Guid]::NewGuid().ToString('N')
    dotnet tool restore
    if ($LASTEXITCODE) { throw 'Tool restore failed.' }
    dotnet sonarscanner begin "/k:$ProjectKey" "/v:$Label" "/d:sonar.host.url=$ServerUrl" "/d:sonar.token=$env:SONAR_TOKEN" '/d:sonar.scanner.scanAll=false' '/d:sonar.exclusions=**/Migrations/**,**/bin/**,**/obj/**' '/d:sonar.coverage.exclusions=**/Migrations/**,**/Program.cs' "/d:sonar.cs.opencover.reportsPaths=$results/**/coverage.opencover.xml"
    if ($LASTEXITCODE) { throw 'Scanner begin failed.' }
    dotnet build StudyMentor.sln --no-incremental
    if ($LASTEXITCODE) { throw 'Build failed.' }
    dotnet test StudyMentor.sln --no-build --settings coverage.runsettings --results-directory $results --collect:'XPlat Code Coverage'
    if ($LASTEXITCODE) { throw 'Tests failed.' }
    dotnet sonarscanner end "/d:sonar.token=$env:SONAR_TOKEN"
    if ($LASTEXITCODE) { throw 'Scanner end failed.' }
    $taskFile = Get-ChildItem .sonarqube -Recurse -Force -Filter report-task.txt | Select-Object -First 1
    if (-not $taskFile) { throw 'Scanner did not produce report-task.txt.' }
    $taskUrl = (Get-Content $taskFile.FullName | Where-Object { $_ -like 'ceTaskUrl=*' }) -replace '^ceTaskUrl=', ''
    $deadline = (Get-Date).AddMinutes(10)
    do {
        $task = (Invoke-RestMethod $taskUrl -Headers @{Authorization="Bearer $env:SONAR_TOKEN"}).task
        if ($task.status -eq 'SUCCESS') { break }
        if ($task.status -in @('FAILED', 'CANCELED')) { throw "Server processing failed: $($task.status)" }
        if ((Get-Date) -gt $deadline) { throw 'Timed out waiting for server processing.' }
        Start-Sleep -Seconds 2
    } while ($true)
    Write-Output "Server processing complete: $($task.analysisId)"
} finally { Pop-Location }

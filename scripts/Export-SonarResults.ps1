param([string]$Label = 'baseline', [string]$ServerUrl = 'http://127.0.0.1:9000')
$ErrorActionPreference = 'Stop'
if (-not $env:SONAR_TOKEN) { throw 'Set SONAR_TOKEN first.' }
$headers = @{ Authorization = "Bearer $env:SONAR_TOKEN" }
$destination = Join-Path (Split-Path $PSScriptRoot -Parent) "doc/module-3/sonar/$Label"
New-Item -ItemType Directory -Force $destination | Out-Null
$metrics = 'bugs,vulnerabilities,code_smells,security_hotspots,coverage,duplicated_lines_density,sqale_rating,reliability_rating,security_rating,ncloc'
Invoke-RestMethod "$ServerUrl/api/measures/component?component=study-mentor&metricKeys=$metrics" -Headers $headers | ConvertTo-Json -Depth 30 | Set-Content "$destination/metrics.json" -Encoding utf8
$issues = @()
$page = 1
do {
    $result = Invoke-RestMethod "$ServerUrl/api/issues/search?componentKeys=study-mentor&ps=500&p=$page&resolved=false" -Headers $headers
    $issues += $result.issues
    $page++
} while ($issues.Count -lt $result.paging.total)
$issues | ConvertTo-Json -Depth 30 | Set-Content "$destination/issues.json" -Encoding utf8
Invoke-RestMethod "$ServerUrl/api/hotspots/search?project=study-mentor&ps=500" -Headers $headers | ConvertTo-Json -Depth 30 | Set-Content "$destination/hotspots.json" -Encoding utf8
Invoke-RestMethod "$ServerUrl/api/qualitygates/project_status?projectKey=study-mentor" -Headers $headers | ConvertTo-Json -Depth 30 | Set-Content "$destination/quality-gate.json" -Encoding utf8
Invoke-RestMethod "$ServerUrl/api/project_analyses/search?project=study-mentor&ps=1" -Headers $headers | ConvertTo-Json -Depth 30 | Set-Content "$destination/analysis.json" -Encoding utf8
$qualityMetrics = 'software_quality_reliability_issues,software_quality_maintainability_issues,software_quality_security_issues,software_quality_reliability_rating,software_quality_maintainability_rating,software_quality_security_rating'
Invoke-RestMethod "$ServerUrl/api/measures/component?component=study-mentor&metricKeys=$qualityMetrics" -Headers $headers | ConvertTo-Json -Depth 30 | Set-Content "$destination/software-quality.json" -Encoding utf8
Write-Output "Exported $($issues.Count) issues to $destination"

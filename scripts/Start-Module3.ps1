param([switch]$PrepareOnly)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    docker info --format '{{.ServerVersion}}' | Out-Null
    if ($LASTEXITCODE) { throw 'Start Docker Desktop and try again.' }
    docker start studymentor-sonarqube | Out-Null
    if ($LASTEXITCODE) { throw 'The configured studymentor-sonarqube container was not found.' }
    $settings = Get-Content study-mentor-api/appsettings.json -Raw | ConvertFrom-Json
    $connection = $env:ConnectionStrings__DefaultConnection
    if (-not $connection) { $connection = $settings.ConnectionStrings.DefaultConnection }
    $db = [System.Data.Common.DbConnectionStringBuilder]::new()
    $db.set_ConnectionString($connection)
    if ($db['Host'] -notin @('localhost', '127.0.0.1')) { throw 'This helper only starts a local development database.' }
    $port = [int]$db['Port']
    $container = 'studymentor-db'
    $exists = docker ps -a --filter "name=^/$container$" --format '{{.Names}}'
    if (-not $exists) {
        docker run -d --name $container --label studymentor.module3.local=true -p "127.0.0.1:${port}:5432" -e "POSTGRES_USER=$($db['Username'])" -e "POSTGRES_PASSWORD=$($db['Password'])" -e "POSTGRES_DB=$($db['Database'])" -v studymentor-module3-db-data:/var/lib/postgresql/data pgvector/pgvector:pg17 | Out-Null
        if ($LASTEXITCODE) { throw 'Could not create the local database container. Check the configured port.' }
    } else {
        $owned = docker inspect $container --format '{{index .Config.Labels "studymentor.module3.local"}}'
        if ($owned -ne 'true') { throw 'An unrelated container has the same name. It was not changed.' }
        docker start $container | Out-Null
        if ($LASTEXITCODE) { throw 'Could not start the database container.' }
    }
    $deadline = (Get-Date).AddMinutes(2)
    do {
        docker exec $container pg_isready -U $db['Username'] -d $db['Database'] 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { break }
        if ((Get-Date) -gt $deadline) { throw 'Database startup timed out.' }
        Start-Sleep -Seconds 2
    } while ($true)
    dotnet tool restore
    if ($LASTEXITCODE) { throw 'Tool restore failed.' }
    dotnet restore StudyMentor.sln
    if ($LASTEXITCODE) { throw 'Package restore failed.' }
    dotnet ef database update --project study-mentor-api
    if ($LASTEXITCODE) { throw 'Database migration failed.' }
    Write-Host 'Database ready. SonarQube: http://127.0.0.1:9000'
    if (-not $PrepareOnly) {
        Write-Host 'Starting API and Swagger: http://localhost:5132. Press Ctrl+C to stop the API.'
        dotnet run --project study-mentor-api --launch-profile http
        if ($LASTEXITCODE) { throw 'API process exited with an error.' }
    }
} finally { Pop-Location }

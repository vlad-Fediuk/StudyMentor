# Script to run all tests with coverage and generate HTML report
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;MarkdownSummary"
Write-Host "Code coverage report generated successfully at CoverageReport\index.html" -ForegroundColor Green

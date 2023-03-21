$RootPath = Split-Path $PSScriptRoot -Parent

Remove-Item -Path $RootPath\TestResults\* -Recurse -Force

dotnet test --environment ENVIRONMENT=testing --collect:"XPlat Code Coverage;Include=[OnspringCLI]*;ExcludeByFile=**/program.cs"

reportgenerator `
-reports:$RootPath\TestResults\*\coverage.cobertura.xml `
-targetdir:$RootPath\TestResults\coveragereport `
-reporttypes:Html_Dark;

$TestsProjectDirectory = Split-Path (Split-Path $PSScriptRoot -parent) -Leaf
$TestsDirectory = Split-Path (Split-Path (Split-Path $PSScriptRoot -parent) -parent) -Leaf

live-server --open=$TestsDirectory\$TestsProjectDirectory\TestResults\coveragereport\
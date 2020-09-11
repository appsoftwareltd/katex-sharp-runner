REM https://www.nuget.org/packages/AppSoftware.KatexSharpRunner

REM Replace version and API key in nuget_push.bat (VCS ignored)

dotnet build AppSoftware.KatexSharpRunner/AppSoftware.KatexSharpRunner.csproj --configuration Release

REM TODO: Replace with equivalent dotnet nuget commands

nuget pack AppSoftware.KatexSharpRunner/AppSoftware.KatexSharpRunner.csproj -OutputDirectory packages -Properties Configuration=Release

nuget push packages/AppSoftware.KatexSharpRunner.9.9.9.nupkg -Source https://api.nuget.org/v3/index.json -SkipDuplicate -ApiKey xxxxxxxxxxxxxxxxxxxxx

PAUSE
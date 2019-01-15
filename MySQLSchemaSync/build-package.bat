@echo off
cd %~dp0
dotnet clean MySQLSchemaSync.csproj -c Release
rd /s /Q bin\Release
dotnet build MySQLSchemaSync.csproj -c Release

dotnet nuget push bin\Release\*.nupkg -s http://nuget.xiaobao100.cn/nuget/xiaobao
pause
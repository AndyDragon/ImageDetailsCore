@echo off
dotnet publish -c Release -r win-x64 --self-contained true
dotnet publish -c Release -r win10-x64 --self-contained true
dotnet publish -c Release -r linux-x64 --self-contained true
dotnet publish -c Release -r linux-arm --self-contained true
dotnet publish -c Release -r osx-x64 --self-contained true
dotnet publish -c Release -r osx.10.12-x64 --self-contained true
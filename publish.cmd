@echo off

dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
copy .\bin\Release\net7.0\win-x64\publish\ImageDetailsCore.exe .\ImageDetailsCore.exe
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
copy .\bin\Release\net7.0\linux-x64\publish\ImageDetailsCore .\ImageDetailsCore-linux
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
copy .\bin\Release\net7.0\osx-x64\publish\ImageDetailsCore .\ImageDetailsCore-mac

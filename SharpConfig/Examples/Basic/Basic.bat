@echo off

%~dp0..\..\bin\SharpConfig.exe --config-source Source\Values.csv --output-directory ConfigOutput --base-directory %~dp0


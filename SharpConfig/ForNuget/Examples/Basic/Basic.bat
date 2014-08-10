@echo off

"%~dp0..\..\bin\SharpConfig.exe" --config-source ..\ConfigSource\ConfigValues.csv --output-directory ConfigOutput --base-directory "%~dp0\ "


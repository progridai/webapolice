@echo off
setlocal

echo Iniciando infraestrutura de testes do WebApolice...
powershell -ExecutionPolicy Bypass -NoProfile -File "%~dp0scripts\run-tests-local.ps1"

endlocal

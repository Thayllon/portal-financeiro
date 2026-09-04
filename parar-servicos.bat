@echo off
setlocal
title Portal Financeiro - Encerrar servicos

echo ============================================
echo  Encerrando os servicos do Portal Financeiro (por porta)...
echo ============================================

call :parar_porta 5178 "API (Portal)"
call :parar_porta 4200 "Frontend (Portal)"
call :parar_porta 4201 "Frontend (Portal)"

echo.
echo  Ligando trocas adicionais (dotnet/node genericos de janelas "Portal")...
taskkill /F /FI "WINDOWTITLE eq Portal-API*" 2>nul >nul
taskkill /F /FI "WINDOWTITLE eq Portal-Web*" 2>nul >nul

echo.
echo  Finalizado.
timeout /t 3 /nobreak >nul
endlocal
exit /b

:parar_porta
set porta=%~1
set nome=%~2
set pid=
for /f "tokens=5" %%a in ('netstat -ano ^| findstr "LISTENING" ^| findstr ":%porta% "') do set "pid=%%a"
if defined pid (
    echo   [%nome%] porta %porta% - encerrando PID %pid%
    taskkill /F /PID %pid% 2>nul >nul
    if errorlevel 1 echo       (falha ao encerrar PID %pid%)
) else (
    echo   [%nome%] porta %porta% - nenhum processo ativo
)
exit /b
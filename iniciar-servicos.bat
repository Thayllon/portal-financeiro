@echo off
setlocal
title Portal Financeiro - Iniciar servicos

echo ============================================
echo  Iniciando a API (dotnet run)
echo ============================================
start "Portal-API" cmd /k "cd /d C:\Projetos\POC\portal-financeiro\src\PortalFinanceiro.API && dotnet run --launch-profile http"

echo.
echo Aguardando a API subir na porta 5178...
timeout /t 5 /nobreak >nul

echo ============================================
echo  Iniciando o Frontend (npm start)
echo ============================================
start "Portal-Web" cmd /k "cd /d C:\Projetos\POC\portal-financeiro\src\PortalFinanceiro.Web && npm start"

echo.
echo Aguardando o front subir na porta 4200...
timeout /t 12 /nobreak >nul

echo.
echo ============================================
echo  Abrindo o navegador...
echo ============================================
start "" http://localhost:4200

echo.
echo Para encerrar, feche as janelas Portal-API e Portal-Web
echo ou rode o script parar-servicos.bat
timeout /t 5 /nobreak >nul
endlocal
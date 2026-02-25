@echo off
echo ==========================================
echo  EPOS Bridge - Iniciar en Modo Desarrollo
echo ==========================================
echo.
echo Verificando instalacion de .NET...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        echo [INFO] .NET encontrado en C:\Program Files\dotnet pero no en el PATH.
        echo Agregando temporalmente al PATH...
        set "PATH=%PATH%;C:\Program Files\dotnet"
    ) else (
        echo [ERROR] No se encontro .NET SDK.
        echo Asegurate de reiniciar Visual Studio Code o la terminal despues de instalar.
        echo O prueba reiniciando la PC.
        pause
        exit /b
    )
)

echo Restaurando paquetes...
dotnet restore

echo Iniciando aplicacion...
echo NOTA: Si es la primera vez, pedira permisos de Administrador para los certificados.
set "DOTNET_ENVIRONMENT=Development"
dotnet run
pause

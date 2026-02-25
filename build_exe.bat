@echo off
echo ==========================================
echo  EPOS Bridge - Compilar Ejecutable (EXE)
echo ==========================================
echo.
echo Compilando para Windows x64 (Unico Archivo)...
echo Esto puede tardar unos segundos...

echo.
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        echo [INFO] Usando ruta directa a .NET...
        set "PATH=%PATH%;C:\Program Files\dotnet"
    )
)

dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o Publish

if %errorlevel% equ 0 (
    echo.
    echo [EXITO] Compilacion completada.
    echo El ejecutable esta en la carpeta "Publish".
    echo.
    echo Abriendo carpeta de destino...
    explorer Publish
) else (
    echo.
    echo [ERROR] Hubo un error en la compilacion.
)
pause

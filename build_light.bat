@echo off
echo ==========================================
echo  EPOS Bridge - Compilar Version Ligera
echo ==========================================
echo.
echo Esta version NO incluye .NET. Requiere instalar el ".NET Desktop Runtime 8.0".
echo Compilando...
echo.

dotnet publish -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o PublishLight

if %errorlevel% equ 0 (
    echo.
    echo [EXITO] Compilacion completada.
    echo El ejecutable ligero esta en la carpeta "PublishLight".
    echo.
    echo Abriendo carpeta de destino...
    explorer PublishLight
) else (
    echo.
    echo [ERROR] Hubo un error en la compilacion.
)
pause

# EPOS Bridge

## 📋 Descripción

**EPOS Bridge** es una aplicación de escritorio diseñada para actuar como un "puente" seguro entre un sistema de Punto de Venta web (E-POS) y el hardware local (impresoras térmicas y cajones de dinero). El objetivo es permitir que una aplicación web pueda comunicarse directamente de manera local con dispositivos físicos (lo cual es bloqueado por las políticas de seguridad de los navegadores web habituales).

## 🚀 Funciones y Características

- **Impresión Local Ininterrumpida:** Recibe solicitudes estructuradas para enviar trabajos de impresión a impresoras térmicas (conectadas por USB u otra vía) mapeadas en Windows.
- **Apertura de Cajón Diaria:** Ejecuta comandos (ESC/POS) directos para la apertura de cajones de dinero (cash drawers).
- **Procesamiento de Trabajos por API/WebSocket:** Ofrece tanto un servidor `HTTPS` con una API REST y como un servidor `WSS` de WebSockets (recomendado) para encolar rápidamente los trabajos.
- **Historial y Pruebas Reales:** Interfaz visual para examinar un log unificado de todas las impresiones recientes y un módulo de pruebas directas para testear la conectividad con la impresora.
- **Permisivo por Defecto (CORS):** El servidor local está diseñado para permitir conexiones desde cualquier origen en la red (`*`) de modo que no interfiera con tu webapp de Punto de Venta.

## ⚙️ ¿Cómo Funciona?

La aplicación funciona en segundo plano, mostrando únicamente un icono (`📦`) en la Bandeja del Sistema (System Tray) cerca de tu reloj en Windows. Al iniciar, el programa hace lo siguiente:
1. Crea y registra automáticamente un **Certificado SSL Local** usando Microsoft Certificate Store. Esto permite que el navegador de tu POS autorice conexiones `localhost`.
2. Habilita una instancia unificada utilizando un `Mutex` para asegurar que el servidor no duplique sus escuchas y colisione.
3. Levanta el servidor local Kestrel en el puerto fijo `8000` para producción o `8001` si estás en entorno de desarrollo.
4. Espera instrucciones JSON (por REST o WebSockets) donde se le envía el contenido en Base64 o comandos crudos.
5. Gestiona una cola central y se comunica con el Spooler ("Cola de Impresión") de Windows.

## 🏃‍♂️ Cómo Iniciarlo

1. Asegúrate de tener los drivers de tus impresoras instalados adecuadamente en tu sistema y de que impriman la página de prueba de Windows.
2. Ejecuta `EposBridge.exe`. 
   * **⚠️ IMPORTANTE:** La **primera vez** debes ejecutarlo como **Administrador**. Esto se requiere solo la primera vez para que pueda generar e instalar su propio certificado de seguridad local.
3. Al abrirse la ventana de bienvenida, se recomienda hacer clic en **"Activar inicio con Windows"** para que EPOS Bridge siempre esté escuchando sin necesidad de que tú abras manualmente el programa todos los días.
4. Finalizado el proceso, el icono aparecerá en la barra inferior. Puedes hacer clic derecho y presionar "Abrir Dashboard" (o ir a `https://localhost:8000`) para ver el estado, registrar o testear las impresoras.

## 🔨 Cómo Compilarlo y Entornos

Debes tener **.NET 8 SDK** instalado.
Si prefieres no utilizar los *scripts bat* que vienen preconfigurados, puedes abrir cualquier terminal en la raíz de esta carpeta.

Existen 2 maneras recomendadas de realizar la compilación:

### 1. Compilación Autocontenida (Recomendada)
Para generar el ejecutable final (`.exe`) completamente independiente (no requerir que la PC del cliente tenga instalado el Runtime de .NET), ejecuta el comando:
```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o Publish
```
*Puedes encontrar el resultado final autoejecutable en la carpeta `Publish/EposBridge.exe`.*

### 2. Compilación Ligera
Para evitar archivos tan grandes o si tienes garantizado que el cliente ya tiene el .NET Desktop Runtime 8.0 al menos. Utiliza el script `build_light.bat` (o ejecuta el comando sin el atributo autocontenido):
```powershell
dotnet publish -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o PublishLight
```
*Puedes encontrar el resultado final en la carpeta `PublishLight/EposBridge.exe`.*

### Entorno de Desarrollo
Puedes ejecutar fácilmente el proyecto desde su código fuente de la siguiente forma o utilizando el archivo `run_dev.bat`:
```powershell
dotnet run
```
*Esto levantará el servidor en `https://localhost:8001`. Requiere Visual Studio o VS Code.*

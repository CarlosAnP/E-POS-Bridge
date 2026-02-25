# EPOS Bridge - Manual de Usuario y Configuración

## Descripción General

**EPOS Bridge** es una aplicación de escritorio diseñada para actuar como un "puente" seguro entre su sistema de Punto de Venta web (E-POS) y el hardware local (impresoras térmicas y cajones de dinero).

Permite que una aplicación web (HTTPS) se comunique con dispositivos USB/Serial locales, algo que los navegadores bloquean por seguridad.

---

## Instalación y Primer Uso

### 1. Requisitos Previos

- Windows 10 o Windows 11 (64 bits).
- Drivers de la impresora térmica instalados y configurados.
- .NET 8 Runtime (si no usa la versión autocontenida).

### 2. Primera Ejecución

La primera vez que abra `EposBridge.exe`, es posible que vea una ventana pidiendo **permisos de Administrador**. Esto es necesario una única vez para:

1.  Generar e instalar un **Certificado SSL Local** (para permitir conexiones seguras HTTPS/WSS).
2.  Registrar el puerto `8000` (Producción) o `8001` (Desarrollo).

> **Nota:** Si su antivirus detecta la aplicación como desconocida, debe agregar una excepción, ya que es una herramienta de uso interno.

### 3. Pantalla de Bienvenida

Verá una ventana de bienvenida. Se recomienda encarecidamente hacer clic en **"Activar inicio con Windows"** para que el puente se inicie automáticamente al encender la PC, garantizando que siempre pueda imprimir.

---

## Funcionalidades del Programa

### Bandeja del Sistema (System Tray)

La aplicación se ejecuta en segundo plano. Verá un icono (📦) en la barra de tareas, cerca del reloj.

- **Clic Derecho**: Abre el menú contextual.
  - **Abrir Dashboard**: Abre la ventana de gestión y pruebas.
  - **Iniciar con Windows**: Activa/Desactiva el auto-inicio (verá una marca ✓ si está activo).
  - **Salir**: Cierra totalmente el puente (dejará de imprimir).

### Dashboard (Panel de Control)

Accesible desde el menú de la bandeja.

#### Pestaña "Estado y Pruebas"

- **Estado del Servicio**: Indica si el servidor interno está escuchando (Verde = OK).
- **Selector de Impresora**: Lista todas las impresoras instaladas en Windows.
- **Botón "Prueba de Impresión"**: Envía un ticket de prueba a la impresora seleccionada.
- **Botón "Abrir Cajón"**: Envía el comando estándar ESC/POS para abrir el cajón de dinero conectado a la impresora.

#### Pestaña "Historial de Impresión"

Muestra una tabla con los últimos **50 trabajos** procesados.

- **Columnas**: Hora, Impresora, Estado (Éxito/Error), Mensaje de Error.
- **Reimpresión**: Seleccione una fila y haga clic en **"Reimprimir Seleccionado"** para enviar el mismo ticket nuevamente a la cola.

---

## Integración Técnica (Para Desarrolladores)

### API REST

- **Base URL**: `https://localhost:8000` (Prod) o `https://localhost:8001` (Dev).
- **CORS**: Habilitado para todos los orígenes (`*`).

| Método | Endpoint                    | Descripción                                          |
| :----- | :-------------------------- | :--------------------------------------------------- |
| `GET`  | `/api/status`               | Verifica si el puente está activo. Devuelve versión. |
| `GET`  | `/api/printers`             | Lista impresoras instaladas (JSON).                  |
| `GET`  | `/api/history`              | Obtiene el historial reciente de impresiones.        |
| `POST` | `/api/history/reprint/{id}` | Re-encola un trabajo por su ID.                      |

### WebSocket (Recomendado)

- **URL**: `wss://localhost:8000/ws`
- **Protocolo**: Mensajes JSON.

#### Enviar Impresión

```json
{
  "type": "print",
  "printer": "Nombre_Impresora_Windows",
  "data": "BASE64_ENCODED_BYTES"
}
```

#### Abrir Cajón

```json
{
  "type": "open_drawer",
  "printer": "Nombre_Impresora_Windows"
}
```

#### Respuestas

El servidor responderá con mensajes de estado:

```json
{
  "status": "success",
  "message": "Job queued"
}
```

O notificaciones de error si falla la validación inicial.

---

## Solución de Problemas

**No imprime:**

1. Verifique que el icono esté en la bandeja del sistema.
2. Abra el Dashboard y haga una "Prueba de Impresión".
3. Si la prueba falla, verifique que la impresora esté encendida, tenga papel y no tenga errores en Windows ("Dispositivos e Impresoras").

**Error de Conexión en el Navegador:**

1. Asegúrese de que el certificado se instaló correctamente (puede requerir reiniciar el navegador).
2. Verifique que no hay antivirus bloqueando el puerto 8000.
3. Intente acceder a `https://localhost:8000/api/status` directamente. Debería ver un JSON con el estado.

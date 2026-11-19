# AutoRenta – Prototipo funcional (HU-01 Reservar vehículo)

Prototipo funcional del Entregable 3 de Diseño de Sistemas de Información (Equipo 04-07, ITM).
Implementa el flujo principal de la historia **HU-01 Reservar vehículo**:

1. Catálogo con filtros y disponibilidad en tiempo real.
2. Cálculo del costo estimado (tarifa estándar o con seguro todo riesgo).
3. Registro de la reserva con bloqueo del vehículo durante 15 minutos.
4. Pago en línea (pasarela simulada en modo de pruebas).
5. Confirmación con código único, comprobante PDF, factura XML y correo de confirmación (simulado).

## Demostración

![Demostración del flujo de reserva](docs/demo_autorenta.gif)

**Tecnologías:** C# · ASP.NET Core 8 (API REST + Blazor) · Entity Framework Core · SQL Server LocalDB.

---

## Requisitos

- Windows con **Visual Studio 2022** (versión 17.8 o superior).
- En el instalador de Visual Studio, la carga de trabajo **"Desarrollo de ASP.NET y web"**. Esa carga ya incluye .NET 8 y **SQL Server Express LocalDB**.

## Cómo ejecutarlo

1. Descomprime la carpeta.
2. Abre el archivo **`AutoRenta.sln`** con doble clic. Se abre en Visual Studio.
3. Espera a que Visual Studio descargue los paquetes (NuGet). Lo indica en la barra inferior y la primera vez tarda uno o dos minutos.
4. Presiona el botón verde **▶ AutoRenta** (o la tecla **F5**).
5. Se abre el navegador en `http://localhost:5136` con el catálogo.

La primera vez, la aplicación crea sola la base de datos **AutoRentaDB** en LocalDB y carga 9 vehículos de ejemplo.

## Recorrido para las capturas del documento

| Paso | Qué hacer | Qué se ve |
|---|---|---|
| 1 | Abrir la app y usar los filtros | Catálogo con los vehículos disponibles |
| 2 | Clic en **Reservar**, elegir fechas y tarifa, escribir nombre y correo | Costo calculado automáticamente |
| 3 | Clic en **Confirmar reserva** | Pantalla de pago con el temporizador de 15 minutos |
| 4 | Llenar la tarjeta y clic en **Pagar** | Pago aprobado |
| 5 | — | Confirmación con el código, más los botones del PDF y del correo |

**Tarjetas de prueba (pasarela simulada):**

- `4242 4242 4242 4242`: **aprobada** (flujo principal).
- `4000 0000 0000 0002`: **rechazada** (el vehículo se libera; sirve para mostrar la regla RN-01).

Vencimiento y CVV pueden ser cualquiera, por ejemplo `12/28` y `123`.

Los comprobantes PDF, las facturas XML y los correos simulados se guardan en la carpeta `AutoRenta.Web/Salidas/`.

## Dónde está cada patrón en el código

| Patrón | Archivos |
|---|---|
| **State** | `Dominio/Vehiculo.cs`, `Dominio/Estados/` |
| **Strategy** | `Dominio/Tarifas/Tarifas.cs`, `Dominio/Reserva.cs` (método `CalcularCosto`) |
| **Adapter** | `Aplicacion/Pagos/` (`IPasarelaPago`, `WompiPagoAdapter`, `WompiClient`) |
| **Observer** | `Aplicacion/Eventos/EventosReserva.cs`, `Aplicacion/FacturacionService.cs`, `Aplicacion/NotificacionService.cs` |
| **Factory Method** | `Aplicacion/Documentos/Documentos.cs` |

Otras piezas:

- **Orquestación del caso de uso:** `Aplicacion/ReservaService.cs`
- **API REST:** `Controllers/ReservaController.cs`
- **Acceso a datos:** `Infraestructura/`
- **Pantallas:** `Components/Pages/`
- **Registro de dependencias (inyección):** `Program.cs`

## Fotos reales de los vehículos (opcional)

Por defecto cada vehículo se muestra con una ilustración según su carrocería. Para usar fotos reales, guarda una imagen por vehículo en `AutoRenta.Web/wwwroot/img/vehiculos/` con el nombre de la **placa** (por ejemplo `YZA987.png`). Se aceptan `.png`, `.jpg` y `.webp`; se ven mejor en PNG con fondo transparente. Usa imágenes propias o con permiso de uso.

## Problemas comunes

- **Error de conexión a la base de datos (LocalDB):** abre el *Visual Studio Installer* → *Modificar* → pestaña *Componentes individuales*, busca **"SQL Server Express LocalDB"**, márcalo e instálalo.
- **Usar SQLite sin instalar nada:** en `AutoRenta.Web/appsettings.json` cambia `"Proveedor": "SqlServer"` por `"Proveedor": "Sqlite"`. La base de datos se crea como un archivo `AutoRenta.db`.
- **El puerto 5136 está ocupado:** cámbialo en `AutoRenta.Web/Properties/launchSettings.json`.

## Subir a GitHub

El archivo `.gitignore` ya está incluido para que no se suban las carpetas `bin`, `obj` ni los archivos generados.

En Visual Studio: menú **Git → Crear repositorio Git… → GitHub**, inicia sesión, escribe el nombre del repositorio y clic en **Crear e insertar**.

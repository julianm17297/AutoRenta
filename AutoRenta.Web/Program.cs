using AutoRenta.Web.Aplicacion;
using AutoRenta.Web.Aplicacion.Documentos;
using AutoRenta.Web.Aplicacion.Eventos;
using AutoRenta.Web.Aplicacion.Pagos;
using AutoRenta.Web.Components;
using AutoRenta.Web.Dominio.Tarifas;
using AutoRenta.Web.Frontend;
using AutoRenta.Web.Infraestructura;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Frontend (Blazor) y API REST ----------
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ReservaApiClient>();

// ---------- Base de datos (Entity Framework Core) ----------
// SqlServer = LocalDB de Visual Studio (por defecto). Sqlite = solo para pruebas sin SQL Server.
var proveedor = builder.Configuration["BaseDeDatos:Proveedor"] ?? "SqlServer";
builder.Services.AddDbContext<AutoRentaDbContext>(o =>
{
    if (proveedor == "Sqlite") o.UseSqlite(builder.Configuration.GetConnectionString("AutoRentaSqlite"));
    else o.UseSqlServer(builder.Configuration.GetConnectionString("AutoRenta"));
});
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

// ---------- Patrón Strategy: tarifas ----------
builder.Services.AddScoped<ICalculoTarifaStrategy, TarifaEstandarStrategy>();
builder.Services.AddScoped<ICalculoTarifaStrategy, TarifaConSeguroTodoRiesgoStrategy>();

// ---------- Patrón Adapter: pasarela de pagos ----------
builder.Services.AddSingleton<WompiClient>();
builder.Services.AddScoped<IPasarelaPago, WompiPagoAdapter>();

// ---------- Patrón Factory Method: documentos ----------
builder.Services.AddSingleton<DocumentoFactory, ComprobantePdfFactory>();
builder.Services.AddSingleton<DocumentoFactory, FacturaXmlFactory>();

// ---------- Patrón Observer: reacciones a la reserva confirmada ----------
builder.Services.AddScoped<FacturacionService>();
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddScoped<IReservaObserver>(sp => sp.GetRequiredService<FacturacionService>());   // 1.º genera documentos
builder.Services.AddScoped<IReservaObserver>(sp => sp.GetRequiredService<NotificacionService>());  // 2.º envía el correo
builder.Services.AddScoped<EventosReserva>();

builder.Services.AddScoped<ReservaService>();

var app = builder.Build();

// Crea la base de datos AutoRentaDB y carga los vehículos de ejemplo la primera vez.
using (var scope = app.Services.CreateScope())
{
    DatosIniciales.Inicializar(scope.ServiceProvider.GetRequiredService<AutoRentaDbContext>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

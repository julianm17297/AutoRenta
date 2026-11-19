using AutoRenta.Web.Aplicacion;
using AutoRenta.Web.Dominio;
using Microsoft.AspNetCore.Mvc;

namespace AutoRenta.Web.Controllers;

/// <summary>
/// API REST (interfaz IReservaAPI): catálogo, cotización, reservas y pagos.
/// Es delgado: recibe la petición, delega en ReservaService y devuelve JSON.
/// </summary>
[ApiController]
[Route("api")]
public class ReservaController : ControllerBase
{
    private readonly ReservaService servicio;
    private readonly FacturacionService facturacion;
    private readonly NotificacionService notificacion;

    public ReservaController(ReservaService servicio, FacturacionService facturacion, NotificacionService notificacion)
    {
        this.servicio = servicio;
        this.facturacion = facturacion;
        this.notificacion = notificacion;
    }

    [HttpGet("vehiculos")]
    public ActionResult<List<VehiculoDto>> GetDisponibles([FromQuery] string? categoria, [FromQuery] string? transmision, [FromQuery] string? marca) =>
        servicio.ConsultarDisponibles(new FiltrosVehiculo(categoria, transmision, marca)).Select(Mapeo.ADto).ToList();

    [HttpGet("vehiculos/{id:int}")]
    public ActionResult<VehiculoDto> GetVehiculo(int id) => Ejecutar(() => Mapeo.ADto(servicio.ObtenerVehiculo(id)));

    [HttpGet("tarifas")]
    public ActionResult<List<TarifaDto>> GetTarifas() =>
        servicio.TarifasDisponibles.Select(t => new TarifaDto(t.Clave, t.Nombre, t.Descripcion)).ToList();

    [HttpPost("reservas/cotizacion")]
    public ActionResult<CotizacionDto> PostCotizacion(CotizacionRequest req) => Ejecutar(() =>
    {
        var (r, estrategia) = servicio.Cotizar(req.VehiculoId, req.FechaInicio, req.FechaFin, req.TipoTarifa);
        var subtotal = r.Vehiculo.TarifaDiaria * r.Dias;
        return new CotizacionDto(r.Dias, r.Vehiculo.TarifaDiaria, subtotal, r.CostoEstimado - subtotal, r.CostoEstimado, estrategia.Nombre);
    });

    [HttpPost("reservas")]
    public ActionResult<ReservaDto> PostReserva(ReservaRequest req) => Ejecutar(() => Mapeo.ADto(servicio.CrearReserva(req)));

    [HttpPost("reservas/{id:int}/pago")]
    public ActionResult<ReservaDto> PostPago(int id, PagoRequest req) => Ejecutar(() => Mapeo.ADto(servicio.ProcesarPago(id, req)));

    [HttpGet("reservas/{id:int}")]
    public ActionResult<ReservaDto> GetReserva(int id) => Ejecutar(() => Mapeo.ADto(servicio.ObtenerReserva(id)));

    [HttpGet("reservas/{id:int}/comprobante")]
    public IActionResult GetComprobante(int id)
    {
        var r = servicio.ObtenerReserva(id);
        var ruta = r.Codigo is null ? null : facturacion.ObtenerComprobante(r.Codigo);
        return ruta is null ? NotFound() : PhysicalFile(ruta, "application/pdf", Path.GetFileName(ruta));
    }

    [HttpGet("reservas/{id:int}/correo")]
    public IActionResult GetCorreo(int id)
    {
        var r = servicio.ObtenerReserva(id);
        var ruta = r.Codigo is null ? null : notificacion.ObtenerCorreo(r.Codigo);
        return ruta is null ? NotFound() : PhysicalFile(ruta, "text/html; charset=utf-8");
    }

    // Traduce las reglas de negocio violadas a una respuesta 400 con mensaje claro.
    private ActionResult<T> Ejecutar<T>(Func<T> accion)
    {
        try { return accion(); }
        catch (ReglaNegocioException ex) { return BadRequest(new ErrorDto(ex.Message)); }
    }
}

public static class Mapeo
{
    public static VehiculoDto ADto(Vehiculo v) =>
        new(v.Id, v.Placa, v.Marca, v.Modelo, v.Categoria, v.Transmision, v.Puestos, v.TarifaDiaria, v.EstadoActual);

    public static ReservaDto ADto(Reserva r) =>
        new(r.Id, r.Codigo, r.Estado.ToString(), r.Vehiculo.EstadoActual, ADto(r.Vehiculo), r.Cliente.Nombre, r.Cliente.Correo,
            r.FechaInicio, r.FechaFin, r.Dias, r.TipoTarifa, r.CostoEstimado, r.VenceBloqueo, r.ReferenciaPago, r.Observacion);
}

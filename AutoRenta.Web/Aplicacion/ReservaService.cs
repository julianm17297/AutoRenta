using AutoRenta.Web.Aplicacion.Eventos;
using AutoRenta.Web.Aplicacion.Pagos;
using AutoRenta.Web.Dominio;
using AutoRenta.Web.Dominio.Tarifas;
using AutoRenta.Web.Infraestructura;

namespace AutoRenta.Web.Aplicacion;

/// <summary>
/// Orquesta el caso de uso HU-01 Reservar vehículo.
/// Depende solo de abstracciones (DIP): repositorios, estrategias de tarifa,
/// pasarela de pagos (Adapter) y eventos de reserva (Observer).
/// </summary>
public class ReservaService
{
    public const int MinutosBloqueo = 15; // RN-01

    private readonly IVehiculoRepository vehiculos;
    private readonly IReservaRepository reservas;
    private readonly IPasarelaPago pasarela;
    private readonly EventosReserva eventos;
    private readonly IEnumerable<ICalculoTarifaStrategy> tarifas;

    public ReservaService(IVehiculoRepository vehiculos, IReservaRepository reservas, IPasarelaPago pasarela,
        EventosReserva eventos, IEnumerable<ICalculoTarifaStrategy> tarifas)
    {
        this.vehiculos = vehiculos;
        this.reservas = reservas;
        this.pasarela = pasarela;
        this.eventos = eventos;
        this.tarifas = tarifas;
    }

    public IEnumerable<ICalculoTarifaStrategy> TarifasDisponibles => tarifas;

    /// <summary>F1: catálogo con filtros y disponibilidad en tiempo real (RF-01).</summary>
    public List<Vehiculo> ConsultarDisponibles(FiltrosVehiculo filtros)
    {
        LiberarBloqueosVencidos();
        return vehiculos.ObtenerDisponibles(filtros);
    }

    public Vehiculo ObtenerVehiculo(int id) =>
        vehiculos.ObtenerPorId(id) ?? throw new ReglaNegocioException("El vehículo no existe.");

    /// <summary>F2: costo estimado con la estrategia de tarifa elegida (Strategy).</summary>
    public (Reserva Reserva, ICalculoTarifaStrategy Estrategia) Cotizar(int idVehiculo, DateTime inicio, DateTime fin, string tipoTarifa)
    {
        ValidarFechas(inicio, fin);
        var estrategia = SeleccionarTarifa(tipoTarifa);
        var reserva = new Reserva { Vehiculo = ObtenerVehiculo(idVehiculo), FechaInicio = inicio, FechaFin = fin };
        reserva.CalcularCosto(estrategia);
        return (reserva, estrategia);
    }

    /// <summary>F2: registra la reserva y bloquea el vehículo 15 minutos (State, RN-01).</summary>
    public Reserva CrearReserva(ReservaRequest datos)
    {
        ValidarFechas(datos.FechaInicio, datos.FechaFin);
        if (string.IsNullOrWhiteSpace(datos.NombreCliente) || string.IsNullOrWhiteSpace(datos.CorreoCliente) || !datos.CorreoCliente.Contains('@'))
            throw new ReglaNegocioException("Ingresa tu nombre y un correo válido.");

        LiberarBloqueosVencidos();
        var vehiculo = ObtenerVehiculo(datos.VehiculoId);
        if (!vehiculo.EstaDisponible)
            throw new ReglaNegocioException("El vehículo ya no está disponible. Elige otro del catálogo.");

        var ahora = DateTime.Now;
        var reserva = new Reserva
        {
            Vehiculo = vehiculo,
            Cliente = reservas.ObtenerOCrearCliente(datos.NombreCliente.Trim(), datos.CorreoCliente.Trim()),
            FechaInicio = datos.FechaInicio.Date,
            FechaFin = datos.FechaFin.Date,
            CreadaEn = ahora,
            VenceBloqueo = ahora.AddMinutes(MinutosBloqueo)
        };
        reserva.CalcularCosto(SeleccionarTarifa(datos.TipoTarifa));
        vehiculo.Bloquear();                 // Disponible -> Reservado
        reservas.Guardar(reserva);
        return reserva;
    }

    /// <summary>F3 y F4: cobra con la pasarela (Adapter), confirma y publica el evento (Observer).</summary>
    public Reserva ProcesarPago(int idReserva, PagoRequest pago)
    {
        var reserva = ObtenerReserva(idReserva);
        if (reserva.Estado != EstadoReserva.Pendiente)
            throw new ReglaNegocioException($"La reserva está {reserva.Estado.ToString().ToLower()} y no admite pagos.");

        if (DateTime.Now > reserva.VenceBloqueo)
        {
            Expirar(reserva);
            reservas.Guardar(reserva);
            throw new ReglaNegocioException("Venció el tiempo de bloqueo de 15 minutos. Vuelve a reservar el vehículo.");
        }

        var resultado = pasarela.Cobrar(reserva.CostoEstimado,
            new DatosPago(pago.Titular, pago.NumeroTarjeta, pago.Vencimiento, pago.Cvv, reserva.Cliente.Correo, $"RES-{reserva.Id}"));

        if (!resultado.Aprobado)
        {
            reserva.Vehiculo.LiberarBloqueo();       // Reservado -> Disponible
            reserva.Cancelar(resultado.Mensaje);
            reservas.Guardar(reserva);
            return reserva;
        }

        reserva.ReferenciaPago = resultado.Referencia;
        reserva.Vehiculo.Confirmar();               // Reservado -> Confirmado
        reserva.Confirmar(GenerarCodigo());
        reservas.Guardar(reserva);

        eventos.Publicar(reserva);                  // Observer: factura + correo
        return reserva;
    }

    public Reserva ObtenerReserva(int id) =>
        reservas.ObtenerPorId(id) ?? throw new ReglaNegocioException("La reserva no existe.");

    /// <summary>RN-01: libera los vehículos cuyas reservas pendientes vencieron.</summary>
    public void LiberarBloqueosVencidos()
    {
        var vencidas = reservas.ObtenerPendientesVencidas(DateTime.Now);
        if (vencidas.Count == 0) return;
        foreach (var r in vencidas) Expirar(r);
        reservas.GuardarCambios();
    }

    private static void Expirar(Reserva r)
    {
        if (r.Vehiculo.EstadoActual == Dominio.Estados.EstadoReservado.NombreEstado)
            r.Vehiculo.LiberarBloqueo();
        r.Expirar();
    }

    private ICalculoTarifaStrategy SeleccionarTarifa(string clave) =>
        tarifas.FirstOrDefault(t => t.Clave == clave) ?? throw new ReglaNegocioException("Tipo de tarifa no válido.");

    private static void ValidarFechas(DateTime inicio, DateTime fin)
    {
        if (inicio.Date < DateTime.Today) throw new ReglaNegocioException("La fecha de inicio no puede ser anterior a hoy.");
        if (fin.Date <= inicio.Date) throw new ReglaNegocioException("La fecha de devolución debe ser posterior a la de inicio.");
    }

    private static string GenerarCodigo() =>
        $"AR-{DateTime.Now:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
}

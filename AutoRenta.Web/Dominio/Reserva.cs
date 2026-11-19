using AutoRenta.Web.Dominio.Tarifas;

namespace AutoRenta.Web.Dominio;

/// <summary>Reserva de un vehículo por parte de un cliente.</summary>
public class Reserva
{
    public int Id { get; set; }
    public string? Codigo { get; private set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string TipoTarifa { get; set; } = "";
    public decimal CostoEstimado { get; private set; }
    public EstadoReserva Estado { get; private set; } = EstadoReserva.Pendiente;
    public DateTime CreadaEn { get; set; }
    /// <summary>Momento en que vence el bloqueo temporal del vehículo (RN-01).</summary>
    public DateTime VenceBloqueo { get; set; }
    public string? ReferenciaPago { get; set; }
    public string? Observacion { get; private set; }

    public int Dias => Math.Max(1, (FechaFin.Date - FechaInicio.Date).Days);

    /// <summary>El costo se calcula con la estrategia recibida (patrón Strategy).</summary>
    public decimal CalcularCosto(ICalculoTarifaStrategy estrategia)
    {
        CostoEstimado = estrategia.CalcularTotal(Vehiculo.TarifaDiaria, Dias);
        TipoTarifa = estrategia.Clave;
        return CostoEstimado;
    }

    public void Confirmar(string codigo)
    {
        if (Estado != EstadoReserva.Pendiente)
            throw new ReglaNegocioException("Solo se puede confirmar una reserva pendiente.");
        Codigo = codigo;
        Estado = EstadoReserva.Confirmada;
    }

    public void Cancelar(string motivo)
    {
        Estado = EstadoReserva.Cancelada;
        Observacion = motivo;
    }

    public void Expirar()
    {
        Estado = EstadoReserva.Expirada;
        Observacion = "Venció el plazo de bloqueo sin completar el pago.";
    }
}

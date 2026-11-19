namespace AutoRenta.Web.Dominio.Tarifas;

/// <summary>Patrón Strategy: cada tarifa es un algoritmo de cálculo intercambiable.</summary>
public interface ICalculoTarifaStrategy
{
    string Clave { get; }
    string Nombre { get; }
    string Descripcion { get; }
    decimal CalcularTotal(decimal tarifaDiaria, int dias);
}

/// <summary>Tarifa diaria por el número de días.</summary>
public class TarifaEstandarStrategy : ICalculoTarifaStrategy
{
    public string Clave => "estandar";
    public string Nombre => "Tarifa estándar";
    public string Descripcion => "Tarifa diaria con el seguro obligatorio incluido.";
    public decimal CalcularTotal(decimal tarifaDiaria, int dias) => tarifaDiaria * dias;
}

/// <summary>Tarifa estándar más un seguro todo riesgo por día.</summary>
public class TarifaConSeguroTodoRiesgoStrategy : ICalculoTarifaStrategy
{
    private readonly decimal valorSeguroDia = 45_000m;
    public string Clave => "todo-riesgo";
    public string Nombre => "Tarifa con seguro todo riesgo";
    public string Descripcion => "Agrega cobertura total por daños y robo ($45.000 por día).";
    public decimal CalcularTotal(decimal tarifaDiaria, int dias) => (tarifaDiaria + valorSeguroDia) * dias;
}

using AutoRenta.Web.Dominio;

namespace AutoRenta.Web.Infraestructura;

/// <summary>Crea la base de datos (si no existe) y carga vehículos de ejemplo.</summary>
public static class DatosIniciales
{
    public static void Inicializar(AutoRentaDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Vehiculos.Any()) return;

        db.Vehiculos.AddRange(
            new Vehiculo { Placa = "JKL123", Marca = "Kia", Modelo = "Picanto", Categoria = "Económico", Transmision = "Manual", Puestos = 5, TarifaDiaria = 120_000 },
            new Vehiculo { Placa = "MNO456", Marca = "Chevrolet", Modelo = "Onix", Categoria = "Económico", Transmision = "Automática", Puestos = 5, TarifaDiaria = 145_000 },
            new Vehiculo { Placa = "PQR789", Marca = "Renault", Modelo = "Logan", Categoria = "Sedán", Transmision = "Manual", Puestos = 5, TarifaDiaria = 150_000 },
            new Vehiculo { Placa = "STU321", Marca = "Mazda", Modelo = "Mazda 3", Categoria = "Sedán", Transmision = "Automática", Puestos = 5, TarifaDiaria = 210_000 },
            new Vehiculo { Placa = "VWX654", Marca = "Renault", Modelo = "Duster", Categoria = "SUV", Transmision = "Manual", Puestos = 5, TarifaDiaria = 230_000 },
            new Vehiculo { Placa = "YZA987", Marca = "Toyota", Modelo = "Corolla Cross", Categoria = "SUV", Transmision = "Automática", Puestos = 5, TarifaDiaria = 290_000 },
            new Vehiculo { Placa = "BCD246", Marca = "Kia", Modelo = "Sportage", Categoria = "SUV", Transmision = "Automática", Puestos = 5, TarifaDiaria = 310_000 },
            new Vehiculo { Placa = "EFG135", Marca = "Toyota", Modelo = "Hilux", Categoria = "Camioneta", Transmision = "Manual", Puestos = 5, TarifaDiaria = 380_000 },
            new Vehiculo { Placa = "HIJ864", Marca = "Chevrolet", Modelo = "Spin", Categoria = "Familiar", Transmision = "Manual", Puestos = 7, TarifaDiaria = 240_000 });
        db.SaveChanges();
    }
}

using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// Proveedores (docs/modelo-datos.md, "Proveedores").
/// Parte de DemoData (partial): solo los usa DevelopmentSeeder.
/// </summary>
public static partial class DemoData
{
    // ---------- Proveedores (10) ----------
    // Repuestos Rápidos Guanacaste está inactivo: con las pastillas de freno demuestra que un
    // proveedor inactivo no entra en la recomendación aunque sea el más barato.
    public static List<Proveedor> Proveedores() =>
    [
        new() { Nombre = "Repuestos Diésel del Valle S.A.", CedulaJuridica = "3-101-456123", PersonaContacto = "Marco Vinicio Solís", Telefono = "2231-4510", Correo = "ventas@dieseldelvalle.test", Direccion = "La Uruca, San José", CondicionesGenerales = "Crédito a 30 días, garantía de 6 meses", Estado = true },
        new() { Nombre = "Frenos y Embragues Centroamericanos S.A.", CedulaJuridica = "3-101-552310", PersonaContacto = "Ana Lucía Brenes", Telefono = "2438-2290", Correo = "pedidos@frenosca.test", Direccion = "El Coyol, Alajuela", CondicionesGenerales = "Crédito a 30 días, garantía de 12 meses", Estado = true },
        new() { Nombre = "Llantas y Servicios del Norte S.A.", CedulaJuridica = "3-101-631874", PersonaContacto = "Diego Alfaro Ramírez", Telefono = "2262-7814", Correo = "ventas@llantasnorte.test", Direccion = "Barva, Heredia", CondicionesGenerales = "Contado, garantía de 12 meses por defecto de fábrica", Estado = true },
        new() { Nombre = "Distribuidora Automotriz Pacífico S.A.", CedulaJuridica = "3-101-702456", PersonaContacto = "Karla Monge Cordero", Telefono = "2663-1145", Correo = "pedidos@dapacifico.test", Direccion = "Barranca, Puntarenas", CondicionesGenerales = "Crédito a 15 días, envío incluido en el GAM", Estado = true },
        new() { Nombre = "Lubricantes y Filtros Central S.R.L.", CedulaJuridica = "3-102-518907", PersonaContacto = "Pablo Coto Aguilar", Telefono = "2551-3360", Correo = "ventas@lubricentral.test", Direccion = "Cartago centro", CondicionesGenerales = "Crédito a 30 días, entrega al día siguiente", Estado = true },
        new() { Nombre = "Electroauto Repuestos Eléctricos S.A.", CedulaJuridica = "3-101-689201", PersonaContacto = "Sofía Rojas Villalobos", Telefono = "2290-6672", Correo = "ventas@electroauto.test", Direccion = "Pavas, San José", CondicionesGenerales = "Contado, garantía de 6 meses", Estado = true },
        new() { Nombre = "Importadora de Partes Pesadas S.A.", CedulaJuridica = "3-101-744318", PersonaContacto = "Gerardo Murillo Sánchez", Telefono = "2444-9031", Correo = "importaciones@partespesadas.test", Direccion = "Grecia, Alajuela", CondicionesGenerales = "Crédito a 45 días, pedidos bajo importación", Estado = true },
        new() { Nombre = "Climatización Vehicular CR S.A.", CedulaJuridica = "3-101-590662", PersonaContacto = "Melissa Quesada Arce", Telefono = "2239-5507", Correo = "ventas@climavehicular.test", Direccion = "Belén, Heredia", CondicionesGenerales = "Crédito a 30 días, garantía de 6 meses", Estado = true },
        new() { Nombre = "Suspensiones Técnicas del Este S.A.", CedulaJuridica = "3-101-615027", PersonaContacto = "Andrés Calderón Vega", Telefono = "2574-8826", Correo = "ventas@suspensioneseste.test", Direccion = "Paraíso, Cartago", CondicionesGenerales = "Crédito a 30 días, garantía de 12 meses", Estado = true },
        new() { Nombre = "Repuestos Rápidos Guanacaste S.R.L.", CedulaJuridica = "3-102-611245", PersonaContacto = "Luis Diego Espinoza", Telefono = "2666-0418", Correo = "ventas@rapidosgte.test", Direccion = "Liberia, Guanacaste", CondicionesGenerales = "Contado", Estado = false }
    ];
}


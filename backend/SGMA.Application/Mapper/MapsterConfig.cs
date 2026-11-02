using Mapster;
using SGMA.Domain.DTO;
using SGMA.Domain.Entities;

namespace SGMA.Application.Mapper;

public static class MapsterConfig
{
    // Entidad → DTO de salida: los mismos nombres y el aplanado (CategoriaNombre ← Categoria.Nombre)
    // Mapster los resuelve por convención, así que no necesitan configuración.
    // DTO de entrada → entidad: se ignora explícitamente lo que el cliente no controla (como en el material).
    public static void RegisterMaps()
    {
        // Activo
        TypeAdapterConfig<ActivoCreateDto, Activo>
            .NewConfig()
            .Ignore(dest => dest.IdActivo,          // lo asigna la base (IDENTITY)
                dest => dest.Codigo,                // lo genera el servicio con el prefijo de la categoría
                dest => dest.IdEstadoActivo,        // todo vehículo nace Operativo (AplicarEstado)
                dest => dest.Imagen,                // se sube aparte (PUT api/Activo/{id}/imagen)
                dest => dest.FechaRegistro,         // la pone la base
                dest => dest.Categoria,             // basta con IdCategoria
                dest => dest.EstadoActivo,
                dest => dest.HistorialEstados);     // lo escribe AplicarEstado

        TypeAdapterConfig<ActivoUpdateDto, Activo>
            .NewConfig()
            .Ignore(dest => dest.IdActivo,          // viene de la ruta, no del body
                dest => dest.Codigo,                // inmutable
                dest => dest.IdEstadoActivo,        // tiene sus propias acciones
                dest => dest.Imagen,
                dest => dest.FechaRegistro,
                dest => dest.Categoria,
                dest => dest.EstadoActivo,
                dest => dest.HistorialEstados);

        // CategoriaActivo
        TypeAdapterConfig<CategoriaActivoCreateDto, CategoriaActivo>
            .NewConfig()
            .Ignore(dest => dest.IdCategoria,       // lo asigna la base
                dest => dest.Activos);

        TypeAdapterConfig<CategoriaActivoUpdateDto, CategoriaActivo>
            .NewConfig()
            .Ignore(dest => dest.IdCategoria,       // viene de la ruta, no del body
                dest => dest.Activos);

        // Especialidad
        TypeAdapterConfig<EspecialidadCreateDto, Especialidad>
            .NewConfig()
            .Ignore(dest => dest.IdEspecialidad,    // lo asigna la base
                dest => dest.TecnicoEspecialidades);

        TypeAdapterConfig<EspecialidadUpdateDto, Especialidad>
            .NewConfig()
            .Ignore(dest => dest.IdEspecialidad,    // viene de la ruta, no del body
                dest => dest.TecnicoEspecialidades);


        // Tecnico
        TypeAdapterConfig<TecnicoCreateDto, Tecnico>
            .NewConfig()
            .Ignore(dest => dest.IdTecnico,         // lo asigna la base
                dest => dest.Estado,                // todo técnico nuevo nace activo (lo pone el servicio)
                dest => dest.TecnicoEspecialidades); // se asocian aparte (POST api/Tecnico/{id}/especialidades)

        // El PUT copia el DTO sobre la entidad rastreada: lo ignorado conserva su valor actual
        TypeAdapterConfig<TecnicoUpdateDto, Tecnico>
            .NewConfig()
            .Ignore(dest => dest.IdTecnico,
                dest => dest.Estado,                // se cambia con DELETE (inactivar)
                dest => dest.TecnicoEspecialidades);

        // Proveedor
        TypeAdapterConfig<ProveedorCreateDto, Proveedor>
            .NewConfig()
            .Ignore(dest => dest.IdProveedor,       // lo asigna la base
                dest => dest.Estado,                // todo proveedor nuevo nace activo (lo pone el servicio)
                dest => dest.RepuestoProveedores);  // se asocian desde el repuesto

        // El PUT copia el DTO sobre la entidad rastreada: lo ignorado conserva su valor actual
        TypeAdapterConfig<ProveedorUpdateDto, Proveedor>
            .NewConfig()
            .Ignore(dest => dest.IdProveedor,
                dest => dest.Estado,                // se cambia con DELETE (inactivar)
                dest => dest.RepuestoProveedores);

        // Repuesto
        TypeAdapterConfig<RepuestoCreateDto, Repuesto>
            .NewConfig()
            .Ignore(dest => dest.IdRepuesto,        // lo asigna la base
                dest => dest.Estado,                // todo repuesto nuevo nace activo (lo pone el servicio)
                dest => dest.RepuestoProveedores);  // se asocian aparte (POST api/Repuesto/{id}/proveedores)

        // El PUT copia el DTO sobre la entidad rastreada: lo ignorado conserva su valor actual.
        // StockActual no está en RepuestoUpdateDto: solo cambia con entradas y consumo en órdenes.
        TypeAdapterConfig<RepuestoUpdateDto, Repuesto>
            .NewConfig()
            .Ignore(dest => dest.IdRepuesto,
                dest => dest.StockActual,
                dest => dest.Estado,                // se cambia con DELETE (inactivar)
                dest => dest.RepuestoProveedores);

        // OrdenMantenimiento: del DTO solo salen IdActivo, IdTipoMantenimiento y Motivo
        TypeAdapterConfig<OrdenMantenimientoCreateDto, OrdenMantenimiento>
            .NewConfig()
            .Ignore(dest => dest.IdOrden,           // lo asigna la base
                dest => dest.Codigo,                // lo genera el servicio (OM-AAAA-00001)
                dest => dest.IdEstadoOrden,         // toda orden nace Solicitada
                dest => dest.IdUsuarioSolicitante,  // es el usuario responsable
                dest => dest.Activo,
                dest => dest.TipoMantenimiento,
                dest => dest.Prioridad,
                dest => dest.EstadoOrden,
                dest => dest.UsuarioSolicitante,
                dest => dest.Asignaciones,          // llegan con el ciclo de vida (Avance 2)
                dest => dest.Detalles,
                dest => dest.RepuestosPendientes,
                dest => dest.HistorialEstados);     // el registro inicial lo agrega el servicio

        // Usuario (Usuario → UsuarioDto no necesita configuración: RolNombre ← Rol.Nombre)
        TypeAdapterConfig<UsuarioCreateDto, Usuario>
            .NewConfig()
            .Ignore(dest => dest.IdUsuario,         // lo asigna la base
                dest => dest.ClaveHash,             // el servicio guarda el hash de Clave, nunca el texto
                dest => dest.Estado,                // todo usuario nuevo nace activo (lo pone el servicio)
                dest => dest.FechaCreacion,         // la pone la base (sysdatetime())
                dest => dest.UltimoAcceso,          // lo actualiza el login
                dest => dest.Rol);                  // basta con IdRol

        // El PUT copia el DTO sobre la entidad rastreada: lo ignorado conserva su valor actual
        TypeAdapterConfig<UsuarioUpdateDto, Usuario>
            .NewConfig()
            .Ignore(dest => dest.IdUsuario,
                dest => dest.ClaveHash,             // PUT api/Usuario/{id}/clave
                dest => dest.IdRol,                 // PUT api/Usuario/{id}/rol
                dest => dest.Estado,                // DELETE (desactivar) y POST .../reactivar
                dest => dest.FechaCreacion,
                dest => dest.UltimoAcceso,
                dest => dest.Rol);
    }
}
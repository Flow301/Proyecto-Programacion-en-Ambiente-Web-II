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
        // =====================================================================
        // Activo
        // =====================================================================
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

        // =====================================================================
        // CategoriaActivo
        // =====================================================================
        TypeAdapterConfig<CategoriaActivoCreateDto, CategoriaActivo>
            .NewConfig()
            .Ignore(dest => dest.IdCategoria,       // lo asigna la base
                dest => dest.Activos);

        TypeAdapterConfig<CategoriaActivoUpdateDto, CategoriaActivo>
            .NewConfig()
            .Ignore(dest => dest.IdCategoria,       // viene de la ruta, no del body
                dest => dest.Activos);

        // =====================================================================
        // Especialidad
        // =====================================================================
        TypeAdapterConfig<EspecialidadCreateDto, Especialidad>
            .NewConfig()
            .Ignore(dest => dest.IdEspecialidad,    // lo asigna la base
                dest => dest.TecnicoEspecialidades);

        TypeAdapterConfig<EspecialidadUpdateDto, Especialidad>
            .NewConfig()
            .Ignore(dest => dest.IdEspecialidad,    // viene de la ruta, no del body
                dest => dest.TecnicoEspecialidades);

        // =====================================================================
        // Tecnico
        // =====================================================================
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
    }
}

using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class ActivoService : IActivoService
{
    // Cuatro dígitos por prefijo: PKP-0001 a PKP-9999
    private const int MaxConsecutivo = 9999;

    private readonly IActivoRepository _repository;
    private readonly ICategoriaActivoRepository _categoriaRepository;
    private readonly IMapper _mapper;

    public ActivoService(IActivoRepository repository, ICategoriaActivoRepository categoriaRepository, IMapper mapper)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ActivoListDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize)
            return Result<IEnumerable<ActivoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var activos = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<ActivoListDto>>(activos);

        return Result<IEnumerable<ActivoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<ActivoDetailDto>> GetByIdAsync(int id)
    {
        var activo = await _repository.GetByIdAsync(id);

        if (activo is null)
            return NoExiste(id);

        var data = _mapper.Map<ActivoDetailDto>(activo);

        return Result<ActivoDetailDto>.Success(data);
    }

    public async Task<Result<ActivoDetailDto>> AddAsync(ActivoCreateDto activo)
    {
        var entity = _mapper.Map<Activo>(activo);

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        var categoria = (await _categoriaRepository.GetByIdAsync(entity.IdCategoria))!;

        entity.IdEstadoActivo = SeedIds.EstadoActivoOperativo;   // todo vehículo nace Operativo

        // Si otro registro simultáneo toma el mismo código, el índice UQ_Activo_Codigo lo rechaza
        // y se reintenta una vez con el siguiente número (docs/decisiones-tecnicas.md, sección 3)
        for (var intento = 1; ; intento++)
        {
            var consecutivo = await _repository.GetUltimoConsecutivoAsync(categoria.Prefijo) + 1;

            if (consecutivo > MaxConsecutivo)
                return Result<ActivoDetailDto>.Failure(409, "Conflicto",
                    $"La categoría '{categoria.Nombre}' alcanzó el máximo de {MaxConsecutivo} vehículos.");

            entity.Codigo = $"{categoria.Prefijo}-{consecutivo:D4}";

            try
            {
                await _repository.AddAsync(entity);
                break;
            }
            catch (DbUpdateException) when (intento == 1)
            {
            }
        }

        var creado = await _repository.GetByIdAsync(entity.IdActivo);
        var data = _mapper.Map<ActivoDetailDto>(creado!);

        return Result<ActivoDetailDto>.Success(data, 201, "Creado", $"Vehículo {entity.Codigo} registrado.");
    }

    public async Task<Result<ActivoDetailDto>> UpdateAsync(int id, ActivoUpdateDto activo)
    {
        var actual = await _repository.GetByIdAsync(id);

        if (actual is null)
            return NoExiste(id);

        if (activo.KilometrajeActual < actual.KilometrajeActual)
            return Result<ActivoDetailDto>.Failure(409, "Conflicto",
                $"El kilometraje no puede disminuir: el actual es {actual.KilometrajeActual} km.");

        var entity = _mapper.Map<Activo>(activo);
        entity.IdActivo = id;                      // el id viene de la ruta, nunca del body
        // El PUT no toca el código (inmutable), el estado (tiene sus acciones), la imagen ni la fecha de registro
        entity.Codigo = actual.Codigo;
        entity.IdEstadoActivo = actual.IdEstadoActivo;
        entity.Imagen = actual.Imagen;
        entity.FechaRegistro = actual.FechaRegistro;

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        await _repository.UpdateAsync(entity);

        var actualizado = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<ActivoDetailDto>(actualizado!);

        return Result<ActivoDetailDto>.Success(data, 200, "OK", "Vehículo actualizado.");
    }

    // Normaliza los textos y aplica las reglas comunes del POST y el PUT
    private async Task<Result<ActivoDetailDto>?> ValidarAsync(Activo entity)
    {
        entity.Nombre = entity.Nombre.Trim();
        entity.Descripcion = entity.Descripcion?.Trim();
        entity.Placa = entity.Placa.Trim().ToUpperInvariant();
        entity.Marca = entity.Marca.Trim();
        entity.Modelo = entity.Modelo.Trim();

        if (entity.KilometrajeUltimoPreventivo > entity.KilometrajeActual)
            return Result<ActivoDetailDto>.Failure(400, "Solicitud inválida",
                "El kilometraje del último preventivo no puede ser mayor que el kilometraje actual.");

        if (entity.FechaAdquisicion.Date > DateTime.Today)
            return Result<ActivoDetailDto>.Failure(400, "Solicitud inválida",
                "La fecha de adquisición no puede ser futura.");

        if (!await _categoriaRepository.ExistsAsync(entity.IdCategoria))
            return Result<ActivoDetailDto>.Failure(404, "Recurso no encontrado",
                $"La categoría {entity.IdCategoria} no existe.");

        int? excludeId = entity.IdActivo == 0 ? null : entity.IdActivo;

        if (await _repository.ExistsByPlacaAsync(entity.Placa, excludeId))
            return Result<ActivoDetailDto>.Failure(409, "Conflicto",
                $"Ya existe un vehículo con la placa '{entity.Placa}'.");

        return null;
    }

    private static Result<ActivoDetailDto> NoExiste(int id) =>
        Result<ActivoDetailDto>.Failure(404, "Recurso no encontrado", $"El vehículo {id} no existe.");
}

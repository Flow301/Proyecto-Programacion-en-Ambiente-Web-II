using MapsterMapper;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;
using SGMA.Shared.Exceptions;

namespace SGMA.Application.Implementation;

public class OrdenMantenimientoService : IOrdenMantenimientoService
{
    // Cinco dígitos por año: OM-2026-00001 a OM-2026-99999
    private const int MaxConsecutivo = 99999;

    private readonly IOrdenMantenimientoRepository _repository;
    private readonly IActivoRepository _activoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OrdenMantenimientoService> _logger;

    public OrdenMantenimientoService(IOrdenMantenimientoRepository repository, IActivoRepository activoRepository,
        IUsuarioRepository usuarioRepository, IMapper mapper, ILogger<OrdenMantenimientoService> logger)
    {
        _repository = repository;
        _activoRepository = activoRepository;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // Administrador y Coordinador/Técnico ven todas; el Solicitante/Consulta, solo las que él creó
    public async Task<Result<IEnumerable<OrdenMantenimientoListDto>>> GetPaginationAsync(int pageNumber, int pageSize,
        int idUsuario, bool esSolicitante)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<OrdenMantenimientoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        int? idSolicitante = esSolicitante ? idUsuario : null;

        var ordenes = await _repository.GetPaginationAsync(pageNumber, pageSize, idSolicitante);
        var totalRecords = await _repository.CountAsync(idSolicitante);

        var data = _mapper.Map<IEnumerable<OrdenMantenimientoListDto>>(ordenes);

        return Result<IEnumerable<OrdenMantenimientoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    // Una orden ajena responde 404 con el mismo mensaje que una inexistente:
    // así el Solicitante no puede averiguar qué ids existen
    public async Task<Result<OrdenMantenimientoDetailDto>> GetByIdAsync(int id, int idUsuario, bool esSolicitante)
    {
        var orden = await _repository.GetByIdAsync(id);

        if (orden is null || (esSolicitante && orden.IdUsuarioSolicitante != idUsuario))
            return Result<OrdenMantenimientoDetailDto>.Failure(404, "Recurso no encontrado", $"La orden {id} no existe.");

        return Result<OrdenMantenimientoDetailDto>.Success(MapearDetalle(orden, ocultarCostos: esSolicitante));
    }

    // Todos los roles lo consultan; el Solicitante/Consulta solo el de sus órdenes, con el mismo 404 que el detalle
    public async Task<Result<IEnumerable<HistorialEstadoOrdenDto>>> GetHistorialAsync(int id, int idUsuario, bool esSolicitante)
    {
        var idSolicitante = await _repository.GetIdSolicitanteAsync(id);

        if (idSolicitante is null || (esSolicitante && idSolicitante != idUsuario))
            return Result<IEnumerable<HistorialEstadoOrdenDto>>.Failure(404, "Recurso no encontrado", $"La orden {id} no existe.");

        var historial = await _repository.GetHistorialAsync(id);

        var data = _mapper.Map<IEnumerable<HistorialEstadoOrdenDto>>(historial);

        return Result<IEnumerable<HistorialEstadoOrdenDto>>.Success(data);
    }

    // Creación → Solicitada (docs/matriz-estados.md): 400 si el cuerpo trae un id que no existe, 409 si falla una condición
    public async Task<Result<OrdenMantenimientoDetailDto>> AddAsync(OrdenMantenimientoCreateDto orden, int idUsuario)
    {
        var activo = await _activoRepository.GetByIdAsync(orden.IdActivo);
        if (activo is null)
            return CuerpoInvalido($"El vehículo {orden.IdActivo} no existe.");

        var tipo = await _repository.GetTipoMantenimientoAsync(orden.IdTipoMantenimiento);
        if (tipo is null)
            return CuerpoInvalido($"El tipo de mantenimiento {orden.IdTipoMantenimiento} no existe.");

        var solicitante = await ValidarSolicitanteAsync(idUsuario);

        if (activo.IdEstadoActivo == SeedIds.EstadoActivoDadoDeBaja)
        {
            _logger.LogWarning("Intento de crear una orden para el vehículo dado de baja {Codigo}", activo.Codigo);
            throw new ReglaNegocioException($"El vehículo {activo.Codigo} está dado de baja y no admite órdenes.");
        }

        // Solo se valida al crear: las órdenes que ya estaban abiertas siguen su curso
        if (activo.IdEstadoActivo == SeedIds.EstadoActivoEnMantenimiento && tipo.IdNaturaleza != SeedIds.NaturalezaEmergencia)
        {
            _logger.LogWarning("Intento de crear una orden {Naturaleza} para el vehículo {Codigo}, que está En Mantenimiento",
                tipo.Naturaleza.Nombre, activo.Codigo);
            throw new ReglaNegocioException(
                $"El vehículo {activo.Codigo} está En Mantenimiento: solo admite órdenes de naturaleza Emergencia.");
        }

        var entity = _mapper.Map<OrdenMantenimiento>(orden);
        entity.Motivo = entity.Motivo.Trim();
        entity.IdUsuarioSolicitante = idUsuario;
        entity.IdEstadoOrden = SeedIds.EstadoOrdenSolicitada;
        entity.FechaSolicitud = DateTime.Now;      // el año del código sale de esta fecha

        // Registro inicial del historial, con estado anterior vacío y la misma fecha de la solicitud
        entity.HistorialEstados.Add(new HistorialEstadoOrden
        {
            IdEstadoNuevo = SeedIds.EstadoOrdenSolicitada,
            FechaCambio = entity.FechaSolicitud,
            IdUsuario = idUsuario
        });

        // Si otro registro simultáneo toma el mismo código, el índice UQ_OrdenMantenimiento_Codigo lo rechaza
        // y se reintenta una vez con el siguiente número (docs/decisiones-tecnicas.md, sección 3)
        var anio = entity.FechaSolicitud.Year;
        for (var intento = 1; ; intento++)
        {
            var consecutivo = await _repository.GetUltimoConsecutivoAsync(anio) + 1;

            if (consecutivo > MaxConsecutivo)
            {
                _logger.LogWarning("El año {Anio} alcanzó el máximo de {MaxConsecutivo} órdenes", anio, MaxConsecutivo);
                return Result<OrdenMantenimientoDetailDto>.Failure(409, "Conflicto",
                    $"El año {anio} alcanzó el máximo de {MaxConsecutivo} órdenes.");
            }

            entity.Codigo = $"OM-{anio}-{consecutivo:D5}";

            if (await _repository.AddAsync(entity))
                break;

            if (intento == 2)
            {
                _logger.LogWarning("Registro de la orden para el vehículo {CodigoActivo} rechazado dos veces por un conflicto con otro registro",
                    activo.Codigo);
                return Result<OrdenMantenimientoDetailDto>.Failure(409, "Conflicto",
                    "Otro registro simultáneo entró en conflicto con esta orden. No se guardó nada; intente de nuevo.");
            }

            _logger.LogWarning("El código {Codigo} ya fue tomado por otro registro; se reintenta con el siguiente", entity.Codigo);
        }

        _logger.LogInformation("Orden {IdOrden} creada: {Codigo} para el vehículo {CodigoActivo}. Usuario: {IdUsuario}",
            entity.IdOrden, entity.Codigo, activo.Codigo, idUsuario);

        var creada = await _repository.GetByIdAsync(entity.IdOrden);

        return Result<OrdenMantenimientoDetailDto>.Success(
            MapearDetalle(creada!, ocultarCostos: solicitante.IdRol == SeedIds.RolSolicitanteConsulta), 201, "Creado",
            $"Orden {entity.Codigo} registrada.");
    }

    // Editar la solicitud (docs/matriz-estados.md, "Acciones que no cambian el estado"): solo el Administrador y solo
    // en Solicitada. Cambia el motivo y el tipo; el vehículo no se cambia. No es una transición: no usa la matriz
    // ni escribe historial.
    public async Task<Result<OrdenMantenimientoDetailDto>> UpdateAsync(int id, OrdenMantenimientoUpdateDto orden, int idUsuario)
    {
        var entity = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        // [Authorize] revisa el rol del token; esto cubre al usuario desactivado o cambiado de rol con un token aún vigente
        var usuario = await UsuarioResponsableAsync(idUsuario);
        if (usuario.Rol.Nombre != Roles.Administrador)
            throw new ReglaNegocioException("Solo el Administrador puede editar solicitudes de mantenimiento.");

        if (entity.IdEstadoOrden != SeedIds.EstadoOrdenSolicitada)
        {
            _logger.LogWarning("Intento de editar la orden {Codigo}, que está {Estado}",
                entity.Codigo, MatrizTransicionesOrden.Nombre(entity.IdEstadoOrden));
            throw new ReglaNegocioException(
                $"La orden {entity.Codigo} está {MatrizTransicionesOrden.Nombre(entity.IdEstadoOrden)}: solo se puede editar mientras está Solicitada.");
        }

        var tipo = await _repository.GetTipoMantenimientoAsync(orden.IdTipoMantenimiento);
        if (tipo is null)
            return CuerpoInvalido($"El tipo de mantenimiento {orden.IdTipoMantenimiento} no existe.");

        // El bloqueo por En Mantenimiento se vuelve a validar solo si cambia el tipo (docs/requerimientos.md, 7.5):
        // así una orden creada como emergencia no se convierte en correctiva para saltarse la regla
        if (tipo.IdTipoMantenimiento != entity.IdTipoMantenimiento
            && entity.Activo.IdEstadoActivo == SeedIds.EstadoActivoEnMantenimiento
            && tipo.IdNaturaleza != SeedIds.NaturalezaEmergencia)
        {
            _logger.LogWarning("Edición de la orden {Codigo} rechazada: tipo {Naturaleza} con el vehículo {CodigoActivo} En Mantenimiento",
                entity.Codigo, tipo.Naturaleza.Nombre, entity.Activo.Codigo);
            throw new ReglaNegocioException(
                $"El vehículo {entity.Activo.Codigo} está En Mantenimiento: el tipo solo puede cambiar a uno de naturaleza Emergencia.");
        }

        _mapper.Map(orden, entity);
        entity.Motivo = entity.Motivo.Trim();

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} editada. Usuario: {IdUsuario}", entity.Codigo, idUsuario);

        return await DetalleAsync(id, $"Orden {entity.Codigo} actualizada.");
    }

    // Solicitada → Diagnosticada (docs/matriz-estados.md). Orden de las validaciones, igual en todas las transiciones:
    // 1) la orden existe (404); 2) la matriz permite la transición para el rol (409); 3) las condiciones propias
    // (400 si el cuerpo trae un id que no existe, 409 si falla una regla); 4) los efectos y el historial en un solo
    // SaveChangesAsync, que falla con 409 si otra petición cambió el estado mientras tanto.
    public async Task<Result<OrdenMantenimientoDetailDto>> DiagnosticarAsync(int id, OrdenMantenimientoDiagnosticoDto diagnostico, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenDiagnosticada, usuario.Rol.Nombre);

        if (!await _repository.PrioridadExistsAsync(diagnostico.IdPrioridad))
            return CuerpoInvalido($"La prioridad {diagnostico.IdPrioridad} no existe.");

        var activo = orden.Activo;
        var kilometraje = diagnostico.KilometrajeDiagnostico!.Value;

        if (kilometraje < activo.KilometrajeActual)
        {
            _logger.LogWarning("Diagnóstico de la orden {Codigo} rechazado: {Kilometraje} km es menor que los {Actual} km del vehículo {CodigoActivo}",
                orden.Codigo, kilometraje, activo.KilometrajeActual, activo.Codigo);
            throw new ReglaNegocioException(
                $"El kilometraje no puede ser menor que el actual del vehículo {activo.Codigo}: {activo.KilometrajeActual} km.");
        }

        // Corrección del tipo (decisión del equipo): si cambia, se vuelve a validar el bloqueo por En Mantenimiento
        var idTipoNuevo = diagnostico.IdTipoMantenimiento ?? orden.IdTipoMantenimiento;
        if (idTipoNuevo != orden.IdTipoMantenimiento)
        {
            var tipo = await _repository.GetTipoMantenimientoAsync(idTipoNuevo);
            if (tipo is null)
                return CuerpoInvalido($"El tipo de mantenimiento {idTipoNuevo} no existe.");

            if (activo.IdEstadoActivo == SeedIds.EstadoActivoEnMantenimiento && tipo.IdNaturaleza != SeedIds.NaturalezaEmergencia)
            {
                _logger.LogWarning("Diagnóstico de la orden {Codigo} rechazado: tipo {Naturaleza} con el vehículo {CodigoActivo} En Mantenimiento",
                    orden.Codigo, tipo.Naturaleza.Nombre, activo.Codigo);
                throw new ReglaNegocioException(
                    $"El vehículo {activo.Codigo} está En Mantenimiento: el tipo solo puede corregirse a uno de naturaleza Emergencia.");
            }

            orden.IdTipoMantenimiento = idTipoNuevo;
        }

        orden.Diagnostico = diagnostico.Diagnostico!.Trim();
        orden.IdPrioridad = diagnostico.IdPrioridad;
        orden.KilometrajeDiagnostico = kilometraje;
        activo.KilometrajeActual = kilometraje;    // el diagnóstico actualiza el kilometraje del vehículo

        AplicarTransicion(orden, SeedIds.EstadoOrdenDiagnosticada, idUsuario, diagnostico.Observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} diagnosticada con prioridad {IdPrioridad} y {Kilometraje} km. Usuario: {IdUsuario}",
            orden.Codigo, orden.IdPrioridad, kilometraje, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} diagnosticada.");
    }

    // Diagnosticada → Aprobada (docs/matriz-estados.md): el Administrador fija la fecha programada
    public async Task<Result<OrdenMantenimientoDetailDto>> AprobarAsync(int id, OrdenMantenimientoAprobacionDto aprobacion, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenAprobada, usuario.Rol.Nombre);

        // Solo cuenta el día: hoy es válido aunque la hora ya haya pasado
        var fechaProgramada = aprobacion.FechaProgramada!.Value.Date;
        if (fechaProgramada < DateTime.Today)
        {
            _logger.LogWarning("Aprobación de la orden {Codigo} rechazada: la fecha {Fecha:yyyy-MM-dd} ya pasó", orden.Codigo, fechaProgramada);
            throw new ReglaNegocioException($"La fecha programada no puede ser anterior a hoy ({DateTime.Today:yyyy-MM-dd}).");
        }

        orden.FechaProgramada = fechaProgramada;

        AplicarTransicion(orden, SeedIds.EstadoOrdenAprobada, idUsuario, aprobacion.Observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} aprobada para el {Fecha:yyyy-MM-dd}. Usuario: {IdUsuario}",
            orden.Codigo, fechaProgramada, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} aprobada.");
    }

    // Diagnosticada → Rechazada (docs/matriz-estados.md): estado final; AplicarTransicion registra la fecha de cierre.
    // El vehículo no cambia, porque la orden nunca lo puso En Mantenimiento.
    public async Task<Result<OrdenMantenimientoDetailDto>> RechazarAsync(int id, string? observacion, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenRechazada, usuario.Rol.Nombre);

        AplicarTransicion(orden, SeedIds.EstadoOrdenRechazada, idUsuario, observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} rechazada. Usuario: {IdUsuario}", orden.Codigo, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} rechazada.");
    }

    // Aprobada - En Ejecución (docs/matriz-estados.md): asigna los técnicos y lleva el vehículo a En Mantenimiento.
    // Decisión del equipo (docs/requerimientos.md, 7.7): iniciar recibe al menos un técnico y asigna e inicia en una sola operación.
    public async Task<Result<OrdenMantenimientoDetailDto>> IniciarAsync(int id, OrdenMantenimientoInicioDto inicio, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        // A En Ejecución se llega desde Aprobada (iniciar) o desde En Espera de Repuesto (reanudar): iniciar exige la primera
        MatrizTransicionesOrden.ValidarOrigen(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenAprobada,
            SeedIds.EstadoOrdenEnEjecucion, "iniciar");
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenEnEjecucion, usuario.Rol.Nombre);

        var idTecnicos = inicio.IdTecnicos!;
        var tecnicos = (await _repository.GetTecnicosAsync(idTecnicos)).ToList();

        // Un id del cuerpo que no existe responde 400
        var inexistentes = idTecnicos.Except(tecnicos.Select(t => t.IdTecnico)).ToList();
        if (inexistentes.Count > 0)
            return CuerpoInvalido($"No existe el técnico {string.Join(", ", inexistentes)}.");

        var tipo = (await _repository.GetTipoMantenimientoAsync(orden.IdTipoMantenimiento))!;
        ValidarTecnicosAsignables(orden.Codigo, tecnicos, tipo);

        var activo = orden.Activo;
        if (activo.IdEstadoActivo == SeedIds.EstadoActivoDadoDeBaja)
            throw new ReglaNegocioException($"El vehículo {activo.Codigo} está Dado de Baja: la orden no se puede ejecutar.");

        var ahora = DateTime.Now;
        foreach (var tecnico in tecnicos)
        {
            orden.Asignaciones.Add(new AsignacionTecnico
            {
                IdTecnico = tecnico.IdTecnico,
                FechaAsignacion = ahora,
                HorasTrabajadas = 0      // las horas se registran durante la ejecución
            });
        }

        // Operativo o Fuera de Servicio - En Mantenimiento, con la orden que lo provocó en el historial del vehículo.
        // Si ya estaba En Mantenimiento por otra orden (una emergencia, por ejemplo), no cambia ni se registra nada.
        if (activo.IdEstadoActivo != SeedIds.EstadoActivoEnMantenimiento)
        {
            activo.HistorialEstados.Add(new HistorialEstadoActivo
            {
                IdEstadoAnterior = activo.IdEstadoActivo,
                IdEstadoNuevo = SeedIds.EstadoActivoEnMantenimiento,
                FechaCambio = ahora,
                IdUsuario = idUsuario,
                IdOrden = orden.IdOrden,
                Observacion = $"Inicio de la ejecución de la orden {orden.Codigo}."
            });
            activo.IdEstadoActivo = SeedIds.EstadoActivoEnMantenimiento;
        }

        AplicarTransicion(orden, SeedIds.EstadoOrdenEnEjecucion, idUsuario, inicio.Observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} en ejecución con los técnicos {IdTecnicos}; vehículo {CodigoActivo} En Mantenimiento. Usuario: {IdUsuario}",
            orden.Codigo, idTecnicos, activo.Codigo, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} en ejecución.");
    }

    // Técnicos y horas de la orden (docs/requerimientos.md, 7.7). La tarifa es la congelada si la orden se cerró;
    // si sigue abierta, la vigente del técnico, igual que el costo de mano de obra del detalle.
    public async Task<Result<IEnumerable<AsignacionTecnicoDto>>> GetTecnicosAsync(int id)
    {
        if (await _repository.GetIdSolicitanteAsync(id) is null)
            return Result<IEnumerable<AsignacionTecnicoDto>>.Failure(404, "Recurso no encontrado", $"La orden {id} no existe.");

        return Result<IEnumerable<AsignacionTecnicoDto>>.Success(await AsignacionesAsync(id));
    }

    // Agregar técnicos mientras la orden sigue En Ejecución (docs/matriz-estados.md, "Acciones que no cambian el estado").
    // Mismas reglas que al iniciar, más una: el técnico no puede estar ya asignado a la orden.
    public async Task<Result<IEnumerable<AsignacionTecnicoDto>>> AgregarTecnicosAsync(int id, OrdenMantenimientoTecnicosDto tecnicos, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        await ValidarCoordinadorEnEjecucionAsync(orden, idUsuario, "agregar técnicos");

        var idTecnicos = tecnicos.IdTecnicos!;
        var encontrados = (await _repository.GetTecnicosAsync(idTecnicos)).ToList();

        var inexistentes = idTecnicos.Except(encontrados.Select(t => t.IdTecnico)).ToList();
        if (inexistentes.Count > 0)
            return Result<IEnumerable<AsignacionTecnicoDto>>.Failure(400, "Solicitud inválida",
                $"No existe el técnico {string.Join(", ", inexistentes)}.");

        var asignados = (await _repository.GetAsignacionesAsync(id)).Select(a => a.IdTecnico).ToHashSet();
        var repetido = encontrados.FirstOrDefault(t => asignados.Contains(t.IdTecnico));
        if (repetido is not null)
            throw new ReglaNegocioException($"El técnico {repetido.NombreCompleto} ya está asignado a la orden {orden.Codigo}.");

        var tipo = (await _repository.GetTipoMantenimientoAsync(orden.IdTipoMantenimiento))!;
        ValidarTecnicosAsignables(orden.Codigo, encontrados, tipo);

        var ahora = DateTime.Now;
        foreach (var tecnico in encontrados)
            orden.Asignaciones.Add(new AsignacionTecnico { IdTecnico = tecnico.IdTecnico, FechaAsignacion = ahora, HorasTrabajadas = 0 });

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Técnicos {IdTecnicos} agregados a la orden {Codigo}. Usuario: {IdUsuario}", idTecnicos, orden.Codigo, idUsuario);

        return Result<IEnumerable<AsignacionTecnicoDto>>.Success(await AsignacionesAsync(id), 200, "OK",
            $"Técnicos agregados a la orden {orden.Codigo}.");
    }

    // Registrar horas (docs/requerimientos.md, 7.7): solo En Ejecución; cada registro se suma a las del técnico
    // en esa orden y va de más de 0 a 24 horas (lo valida el DTO).
    public async Task<Result<IEnumerable<AsignacionTecnicoDto>>> RegistrarHorasAsync(int id, int idTecnico, decimal horas, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        await ValidarCoordinadorEnEjecucionAsync(orden, idUsuario, "registrar horas");

        if ((await _repository.GetAsignacionesAsync(id)).All(a => a.IdTecnico != idTecnico))
            throw new RecursoNoEncontradoException($"El técnico {idTecnico} no está asignado a la orden {orden.Codigo}.");

        // La suma va en un solo UPDATE con la condición "sigue En Ejecución": 0 filas = la orden cambió mientras tanto
        if (await _repository.SumarHorasAsync(id, idTecnico, horas) == 0)
            throw new ReglaNegocioException(
                $"La orden {orden.Codigo} dejó de estar En Ejecución mientras se registraban las horas. No se guardó nada.");

        _logger.LogInformation("{Horas} h registradas al técnico {IdTecnico} en la orden {Codigo}. Usuario: {IdUsuario}",
            horas, idTecnico, orden.Codigo, idUsuario);

        return Result<IEnumerable<AsignacionTecnicoDto>>.Success(await AsignacionesAsync(id), 200, "OK",
            $"Se registraron {horas} h al técnico {idTecnico} en la orden {orden.Codigo}.");
    }

    // Repuestos usados y pendientes de la orden (docs/requerimientos.md, 7.6), con su estado actual
    public async Task<Result<OrdenRepuestosDto>> GetRepuestosAsync(int id)
    {
        var idEstado = await _repository.GetIdEstadoAsync(id);
        if (idEstado is null)
            return Result<OrdenRepuestosDto>.Failure(404, "Recurso no encontrado", $"La orden {id} no existe.");

        return Result<OrdenRepuestosDto>.Success(await RepuestosAsync(id, idEstado.Value));
    }

    // Registrar repuestos (docs/requerimientos.md, 7.6, y docs/matriz-estados.md): solo el Coordinador/Técnico y solo
    // En Ejecución. Cada línea con stock suficiente se descuenta y pasa al detalle con el costo vigente; una línea sin stock
    // completo queda entera como pendiente (sin surtidos parciales) y la orden pasa sola a En Espera de Repuesto.
    // Todo en una transacción: el descuento de stock y el guardado de la orden se confirman juntos o nada.
    public async Task<Result<OrdenRepuestosDto>> RegistrarRepuestosAsync(int id, OrdenMantenimientoRepuestosDto repuestos, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        await ValidarCoordinadorEnEjecucionAsync(orden, idUsuario, "registrar repuestos");

        // Varias líneas del mismo repuesto se suman en una sola, en el orden en que llegaron
        var lineas = repuestos.Repuestos!
            .GroupBy(l => l.IdRepuesto)
            .Select(g => (IdRepuesto: g.Key, Cantidad: g.Sum(l => (long)l.Cantidad)))
            .ToList();

        var excedida = lineas.FirstOrDefault(l => l.Cantidad > int.MaxValue);
        if (excedida != default)
            return Result<OrdenRepuestosDto>.Failure(400, "Solicitud inválida",
                $"La cantidad total del repuesto {excedida.IdRepuesto} supera el máximo permitido.");

        var catalogo = (await _repository.GetRepuestosAsync(lineas.Select(l => l.IdRepuesto))).ToDictionary(r => r.IdRepuesto);

        var inexistentes = lineas.Where(l => !catalogo.ContainsKey(l.IdRepuesto)).Select(l => l.IdRepuesto).ToList();
        if (inexistentes.Count > 0)
            return Result<OrdenRepuestosDto>.Failure(400, "Solicitud inválida", $"No existe el repuesto {string.Join(", ", inexistentes)}.");

        var inactivo = lineas.Select(l => catalogo[l.IdRepuesto]).FirstOrDefault(r => !r.Estado);
        if (inactivo is not null)
            throw new ReglaNegocioException($"El repuesto {inactivo.Codigo} está inactivo.");

        var faltantes = new List<string>();

        await _repository.EjecutarEnTransaccionAsync(async () =>
        {
            var ahora = DateTime.Now;
            var numeroLinea = await _repository.GetUltimaLineaAsync(id);

            foreach (var (idRepuesto, total) in lineas)
            {
                var repuesto = catalogo[idRepuesto];
                var cantidad = (int)total;

                // El UPDATE solo descuenta si alcanza: si no, la línea completa queda pendiente
                if (await _repository.DescontarStockAsync(idRepuesto, cantidad))
                {
                    orden.Detalles.Add(new DetalleRepuesto
                    {
                        NumeroLinea = ++numeroLinea,
                        IdRepuesto = idRepuesto,
                        Cantidad = cantidad,
                        CostoUnitario = repuesto.CostoUnitario,      // costo vigente al momento del uso
                        Subtotal = Math.Round(cantidad * repuesto.CostoUnitario, 2, MidpointRounding.AwayFromZero),
                        FechaRegistro = ahora
                    });
                }
                else
                {
                    orden.RepuestosPendientes.Add(new RepuestoPendiente
                    {
                        IdRepuesto = idRepuesto,
                        Cantidad = cantidad,
                        FechaRegistro = ahora
                    });
                    faltantes.Add($"Stock insuficiente de {repuesto.Codigo}: requeridos {cantidad}, disponibles {repuesto.StockActual}");
                }
            }

            // En Ejecución → En Espera de Repuesto, automática, con la observación de lo que falta.
            // El vehículo sigue En Mantenimiento.
            if (faltantes.Count > 0)
            {
                var observacion = string.Join("; ", faltantes);
                if (observacion.Length > 500)
                    observacion = observacion[..497] + "...";

                MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenEnEsperaDeRepuesto, Roles.CoordinadorTecnico);
                AplicarTransicion(orden, SeedIds.EstadoOrdenEnEsperaDeRepuesto, idUsuario, observacion);
            }

            await _repository.SaveChangesAsync();
        });

        var registrados = lineas.Count - faltantes.Count;

        if (faltantes.Count > 0)
        {
            _logger.LogWarning("Orden {Codigo} en espera de repuesto: {Faltantes}. Usuario: {IdUsuario}",
                orden.Codigo, string.Join("; ", faltantes), idUsuario);
        }

        _logger.LogInformation("{Registrados} repuestos registrados en la orden {Codigo}. Usuario: {IdUsuario}",
            registrados, orden.Codigo, idUsuario);

        var detalle = faltantes.Count == 0
            ? (registrados == 1 ? $"Se registró 1 repuesto en la orden {orden.Codigo}." : $"Se registraron {registrados} repuestos en la orden {orden.Codigo}.")
            : $"La orden {orden.Codigo} pasó a En Espera de Repuesto. {string.Join("; ", faltantes)}.";

        return Result<OrdenRepuestosDto>.Success(await RepuestosAsync(id, orden.IdEstadoOrden), 200, "OK", detalle);
    }

    // En Espera de Repuesto → En Ejecución (docs/matriz-estados.md): todos los pendientes deben tener stock; si falta
    // alguno, se rechaza indicando cuál. Cada pendiente pasa al detalle con el costo vigente, se descuenta su stock y se
    // elimina. Todo en una transacción: si un descuento no alcanza a última hora, no se guarda nada.
    public async Task<Result<OrdenMantenimientoDetailDto>> ReanudarAsync(int id, string? observacion, int idUsuario)
    {
        var orden = await _repository.GetCompletaForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        // A En Ejecución se llega desde Aprobada (iniciar) o desde En Espera de Repuesto (reanudar): reanudar exige la segunda
        MatrizTransicionesOrden.ValidarOrigen(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenEnEsperaDeRepuesto,
            SeedIds.EstadoOrdenEnEjecucion, "reanudar");
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenEnEjecucion, usuario.Rol.Nombre);

        var pendientes = orden.RepuestosPendientes.OrderBy(p => p.FechaRegistro).ThenBy(p => p.IdRepuesto).ToList();

        // Primero se revisa todo, para responder con la lista completa de lo que falta
        var faltantes = pendientes
            .Where(p => !p.Repuesto.Estado || p.Repuesto.StockActual < p.Cantidad)
            .Select(p => p.Repuesto.Estado
                ? $"Stock insuficiente de {p.Repuesto.Codigo}: requeridos {p.Cantidad}, disponibles {p.Repuesto.StockActual}"
                : $"El repuesto {p.Repuesto.Codigo} está inactivo")
            .ToList();

        if (faltantes.Count > 0)
        {
            _logger.LogWarning("Reanudación de la orden {Codigo} rechazada: {Faltantes}", orden.Codigo, string.Join("; ", faltantes));
            throw new ReglaNegocioException($"No se puede reanudar la orden {orden.Codigo}. {string.Join("; ", faltantes)}.");
        }

        await _repository.EjecutarEnTransaccionAsync(async () =>
        {
            var ahora = DateTime.Now;
            var numeroLinea = orden.Detalles.Select(d => d.NumeroLinea).DefaultIfEmpty(0).Max();

            foreach (var pendiente in pendientes)
            {
                var repuesto = pendiente.Repuesto;

                // Otra orden pudo gastar el stock entre la revisión y este UPDATE: se cancela todo
                if (!await _repository.DescontarStockAsync(repuesto.IdRepuesto, pendiente.Cantidad))
                    throw new ReglaNegocioException(
                        $"No se puede reanudar la orden {orden.Codigo}: el stock de {repuesto.Codigo} se agotó mientras se reanudaba. No se guardó nada.");

                orden.Detalles.Add(new DetalleRepuesto
                {
                    NumeroLinea = ++numeroLinea,
                    IdRepuesto = repuesto.IdRepuesto,
                    Cantidad = pendiente.Cantidad,
                    CostoUnitario = repuesto.CostoUnitario,      // costo vigente al reanudar
                    Subtotal = Math.Round(pendiente.Cantidad * repuesto.CostoUnitario, 2, MidpointRounding.AwayFromZero),
                    FechaRegistro = ahora
                });
            }

            _repository.EliminarPendientes(pendientes);
            AplicarTransicion(orden, SeedIds.EstadoOrdenEnEjecucion, idUsuario, observacion);

            await _repository.SaveChangesAsync();
        });

        _logger.LogInformation("Orden {Codigo} reanudada: {Cantidad} pendientes pasaron al detalle. Usuario: {IdUsuario}",
            orden.Codigo, pendientes.Count, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} reanudada: {pendientes.Count} repuestos pendientes pasaron al detalle.");
    }

    // Solicitada, Diagnosticada o Aprobada - Cancelada (docs/matriz-estados.md): estado final con fecha de cierre.
    // El vehículo conserva su estado, porque todavía no había entrado a mantenimiento por esta orden.
    public async Task<Result<OrdenMantenimientoDetailDto>> CancelarAsync(int id, string? observacion, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenCancelada, usuario.Rol.Nombre);

        // La matriz permite cancelar desde la ejecución, pero ese caso tiene efectos propios (congelar costos, eliminar
        // pendientes, devolver el vehículo a Operativo) que llegan con la ejecución en la semana 9. Hasta entonces se rechaza
        // para no dejar la orden cerrada con el vehículo En Mantenimiento.
        if (orden.IdEstadoOrden is SeedIds.EstadoOrdenEnEjecucion or SeedIds.EstadoOrdenEnEsperaDeRepuesto)
        {
            _logger.LogWarning("Intento de cancelar la orden {Codigo} durante la ejecución, todavía no disponible", orden.Codigo);
            throw new ReglaNegocioException(
                $"La orden {orden.Codigo} está {MatrizTransicionesOrden.Nombre(orden.IdEstadoOrden)}: la cancelación durante la ejecución todavía no está disponible.");
        }

        AplicarTransicion(orden, SeedIds.EstadoOrdenCancelada, idUsuario, observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} cancelada. Usuario: {IdUsuario}", orden.Codigo, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} cancelada.");
    }

    // Único punto que cambia el estado de la orden y escribe HistorialEstadoOrden (docs/matriz-estados.md, "Historial").
    // No guarda: quien lo llama hace un solo SaveChangesAsync. Los estados finales registran la fecha de cierre.
    private static void AplicarTransicion(OrdenMantenimiento orden, int idEstadoNuevo, int idUsuario, string? observacion)
    {
        var ahora = DateTime.Now;

        orden.HistorialEstados.Add(new HistorialEstadoOrden
        {
            IdEstadoAnterior = orden.IdEstadoOrden,
            IdEstadoNuevo = idEstadoNuevo,
            FechaCambio = ahora,
            IdUsuario = idUsuario,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
        });

        orden.IdEstadoOrden = idEstadoNuevo;

        if (idEstadoNuevo is SeedIds.EstadoOrdenRechazada or SeedIds.EstadoOrdenCompletada or SeedIds.EstadoOrdenCancelada)
            orden.FechaCierre = ahora;
    }

    // Cada técnico debe estar activo, disponible y poseer la especialidad que requiere el tipo (docs/requerimientos.md, 7.7).
    // El nivel de certificación no bloquea: el enunciado solo exige poseer la especialidad.
    private void ValidarTecnicosAsignables(string codigoOrden, IEnumerable<Tecnico> tecnicos, TipoMantenimiento tipo)
    {
        foreach (var tecnico in tecnicos)
        {
            string? motivo = !tecnico.Estado ? "está inactivo"
                : !tecnico.Disponible ? "no está disponible"
                : tecnico.TecnicoEspecialidades.All(e => e.IdEspecialidad != tipo.IdEspecialidadRequerida)
                    ? $"no posee la especialidad {tipo.EspecialidadRequerida.Nombre}, que requiere el tipo {tipo.Nombre}"
                : null;

            if (motivo is not null)
            {
                _logger.LogWarning("Asignación del técnico {IdTecnico} a la orden {Codigo} rechazada: {Motivo}",
                    tecnico.IdTecnico, codigoOrden, motivo);
                throw new ReglaNegocioException($"El técnico {tecnico.NombreCompleto} {motivo}.");
            }
        }
    }

    // Acciones de la ejecución que no cambian el estado: solo el Coordinador/Técnico (rol de la base, por si el token
    // es viejo) y solo con la orden En Ejecución (docs/matriz-estados.md)
    private async Task ValidarCoordinadorEnEjecucionAsync(OrdenMantenimiento orden, int idUsuario, string accion)
    {
        var usuario = await UsuarioResponsableAsync(idUsuario);
        if (usuario.Rol.Nombre != Roles.CoordinadorTecnico)
            throw new ReglaNegocioException($"Solo el Coordinador/Técnico puede {accion}.");

        if (orden.IdEstadoOrden != SeedIds.EstadoOrdenEnEjecucion)
        {
            _logger.LogWarning("Intento de {Accion} en la orden {Codigo}, que está {Estado}",
                accion, orden.Codigo, MatrizTransicionesOrden.Nombre(orden.IdEstadoOrden));
            throw new ReglaNegocioException(
                $"La orden {orden.Codigo} está {MatrizTransicionesOrden.Nombre(orden.IdEstadoOrden)}: solo se puede {accion} mientras está En Ejecución.");
        }
    }

    // Asignaciones con la tarifa que corresponde y su subtotal, redondeado igual que el costo de mano de obra
    private async Task<IEnumerable<AsignacionTecnicoDto>> AsignacionesAsync(int idOrden)
    {
        var asignaciones = await _repository.GetAsignacionesAsync(idOrden);

        return asignaciones.Select(a =>
        {
            var dto = _mapper.Map<AsignacionTecnicoDto>(a);
            dto.TarifaHora = a.TarifaAplicada ?? a.Tecnico.TarifaHora;
            dto.TarifaCongelada = a.TarifaAplicada.HasValue;
            dto.Subtotal = Math.Round(a.HorasTrabajadas * dto.TarifaHora, 2, MidpointRounding.AwayFromZero);
            return dto;
        }).ToList();
    }

    // Detalle y pendientes de la orden, con el estado que se indica
    private async Task<OrdenRepuestosDto> RepuestosAsync(int idOrden, int idEstadoOrden)
    {
        var detalles = await _repository.GetDetallesAsync(idOrden);
        var pendientes = await _repository.GetPendientesAsync(idOrden);

        return new OrdenRepuestosDto
        {
            IdEstadoOrden = idEstadoOrden,
            EstadoOrdenNombre = MatrizTransicionesOrden.Nombre(idEstadoOrden),
            Detalle = _mapper.Map<List<DetalleRepuestoDto>>(detalles),
            Pendientes = _mapper.Map<List<RepuestoPendienteDto>>(pendientes)
        };
    }

    // Un id del cuerpo que no existe responde 400, como en el material: lo inválido es el cuerpo, no la orden de la ruta
    private static Result<OrdenMantenimientoDetailDto> CuerpoInvalido(string detalle) =>
        Result<OrdenMantenimientoDetailDto>.Failure(400, "Solicitud inválida", detalle);

    // Vuelve a leer la orden con sus relaciones para responder con los nombres y los costos
    private async Task<Result<OrdenMantenimientoDetailDto>> DetalleAsync(int id, string detalle)
    {
        var orden = await _repository.GetByIdAsync(id);
        return Result<OrdenMantenimientoDetailDto>.Success(MapearDetalle(orden!), 200, "OK", detalle);
    }

    // Usuario responsable con su rol de la base: la matriz lo usa para la segunda validación del rol
    private async Task<Usuario> UsuarioResponsableAsync(int idUsuario)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);

        if (usuario is null || !usuario.Estado)
            throw new RecursoNoEncontradoException($"El usuario {idUsuario} no existe o está inactivo.");

        return usuario;
    }

    // Con la orden cerrada se usan los costos congelados; abierta, se calculan en vivo.
    // Horas por tarifa se redondea a 2 decimales igual que SQL Server al guardarlo en decimal(18,2) al congelarlo.
    // El Solicitante/Consulta ve el estado y el historial, pero no los costos (docs/requerimientos.md, 7.5): quedan en null.
    private OrdenMantenimientoDetailDto MapearDetalle(OrdenMantenimiento orden, bool ocultarCostos = false)
    {
        var data = _mapper.Map<OrdenMantenimientoDetailDto>(orden);

        // Mapster ya copió los costos congelados de la entidad (mismo nombre): se borran explícitamente
        if (ocultarCostos)
        {
            data.CostoRepuestos = data.CostoManoObra = data.CostoTotal = null;
            return data;
        }

        data.CostoRepuestos = orden.CostoRepuestos ?? orden.Detalles.Sum(d => d.Subtotal);
        data.CostoManoObra = orden.CostoManoObra
            ?? Math.Round(orden.Asignaciones.Sum(a => a.HorasTrabajadas * (a.TarifaAplicada ?? a.Tecnico.TarifaHora)), 2, MidpointRounding.AwayFromZero);
        data.CostoTotal = orden.CostoTotal ?? data.CostoRepuestos.Value + data.CostoManoObra.Value;

        return data;
    }

    // [Authorize] ya valida el rol del token; esto cubre al usuario desactivado o cambiado de rol con un token aún vigente.
    // El Coordinador/Técnico no crea órdenes (docs/requerimientos.md, 7.5).
    private async Task<Usuario> ValidarSolicitanteAsync(int idUsuario)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);

        if (usuario is null || !usuario.Estado)
            throw new RecursoNoEncontradoException($"El usuario {idUsuario} no existe o está inactivo.");

        if (usuario.IdRol != SeedIds.RolAdministrador && usuario.IdRol != SeedIds.RolSolicitanteConsulta)
            throw new ReglaNegocioException("Solo el Administrador o el Solicitante/Consulta pueden crear órdenes.");

        return usuario;
    }
}

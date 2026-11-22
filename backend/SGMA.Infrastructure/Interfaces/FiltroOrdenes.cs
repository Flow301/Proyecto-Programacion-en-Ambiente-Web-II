namespace SGMA.Infrastructure.Interfaces;

// Filtros del listado de órdenes (enunciado Avance 2, 5.3): por estado (uno o varios) y por vehículo.
// IdSolicitante no viene del cliente: lo pone el servicio cuando el usuario es Solicitante/Consulta.
public record FiltroOrdenes(int? IdSolicitante = null, IReadOnlyCollection<int>? IdsEstado = null, int? IdActivo = null);

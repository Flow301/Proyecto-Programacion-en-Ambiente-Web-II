import { api } from '@/api/api'

// Reporte de costos por orden (reporte B1): Administrador y Coordinador/Técnico.
// Todos los montos, porcentajes y la comparación los calcula la API; el frontend solo los muestra.
export const obtenerCostosOrden = (idOrden) => api(`Reporte/costos-orden/${idOrden}`)
import { api } from '@/api/api'
import { conFiltros } from './query'

// Vehículos (activos). Aquí solo lo que necesitan las órdenes: elegir el vehículo de una solicitud
// y conocer su kilometraje al diagnosticar. Las pantallas de vehículos son del plan de la semana 10 (Integrante B).
export const listarActivos = (pagina, tamano, filtros) => api(conFiltros(`Activo/${pagina}/${tamano}`, filtros))
export const obtenerActivo = (id) => api(`Activo/${id}`)

// Todos los vehículos, recorriendo las páginas de la API (máximo 50 por página, convenciones §3)
export async function listarTodosLosActivos() {
  const primera = await listarActivos(1, 50)
  const resto = await Promise.all(
    Array.from({ length: Math.max((primera.totalPages ?? 1) - 1, 0) }, (_, i) => listarActivos(i + 2, 50)),
  )
  return { ...primera, data: [primera, ...resto].flatMap((r) => r.data) }
}

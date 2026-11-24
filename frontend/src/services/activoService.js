import { api } from '@/api/api'
import { conFiltros } from './query'

// Vehículos (activos): consulta para los tres roles; registro, edición, imagen, lecturas y cambios de estado
// solo para el Administrador (la API lo valida en cada endpoint).

// filtros: { idCategoria, idEstadoActivo }
export const listarActivos = (pagina, tamano, filtros) => api(conFiltros(`Activo/${pagina}/${tamano}`, filtros))
export const obtenerActivo = (id) => api(`Activo/${id}`)
export const historialActivo = (id, pagina = 1, tamano = 10) => api(`Activo/${id}/historial/${pagina}/${tamano}`)
export const lecturasActivo = (id, pagina = 1, tamano = 10) => api(`Activo/${id}/lecturas/${pagina}/${tamano}`)

export const crearActivo = (datos) => api('Activo', { method: 'POST', body: datos })
export const actualizarActivo = (id, datos) => api(`Activo/${id}`, { method: 'PUT', body: datos })
export const registrarLectura = (id, datos) => api(`Activo/${id}/lecturas`, { method: 'POST', body: datos })
// Estado destino por id (Operativo o Fuera de Servicio); la API decide si la transición es válida
export const cambiarEstadoActivo = (id, idEstadoActivo, observacion) =>
  api(`Activo/${id}/estado`, { method: 'POST', body: { idEstadoActivo, observacion } })
// Baja lógica: el vehículo pasa a Dado de Baja y nunca se borra
export const darDeBajaActivo = (id, observacion) => api(`Activo/${id}`, { method: 'DELETE', body: { observacion } })

// La imagen va aparte, como multipart/form-data (JPG, PNG o WebP, máximo 2 MB)
export function subirImagenActivo(id, archivo) {
  const datos = new FormData()
  datos.append('imagen', archivo)
  return api(`Activo/${id}/imagen`, { method: 'PUT', body: datos })
}

// Todos los vehículos, recorriendo las páginas de la API (máximo 50 por página, convenciones §3)
export async function listarTodosLosActivos() {
  const primera = await listarActivos(1, 50)
  const resto = await Promise.all(
    Array.from({ length: Math.max((primera.totalPages ?? 1) - 1, 0) }, (_, i) => listarActivos(i + 2, 50)),
  )
  return { ...primera, data: [primera, ...resto].flatMap((r) => r.data) }
}


import { api } from '@/api/api'

// Rutas de la API de órdenes de mantenimiento. Los componentes solo llaman estas funciones.

// filtros: { idEstadoOrden: [1, 3], idActivo } → ?idEstadoOrden=1&idEstadoOrden=3&idActivo=…
export function listarOrdenes(pagina, tamano, filtros = {}) {
  const query = new URLSearchParams()
  for (const [clave, valor] of Object.entries(filtros)) {
    for (const v of [].concat(valor ?? [])) if (v !== '' && v != null) query.append(clave, v)
  }
  const texto = query.toString()
  return api(`OrdenMantenimiento/${pagina}/${tamano}${texto ? `?${texto}` : ''}`)
}

export const obtenerOrden = (id) => api(`OrdenMantenimiento/${id}`)

export const historialOrden = (id, pagina = 1, tamano = 20) => api(`OrdenMantenimiento/${id}/historial/${pagina}/${tamano}`)

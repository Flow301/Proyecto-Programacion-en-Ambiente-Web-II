import { api } from '@/api/api'
import { conFiltros } from './query'

// Órdenes de mantenimiento: consulta, solicitud y cada acción del proceso. Cada acción tiene su propio endpoint
// y la API decide si es válida (matriz de transiciones); el frontend solo muestra las de accionesDisponibles.

// filtros: { idEstadoOrden: [1, 3], idActivo }
export const listarOrdenes = (pagina, tamano, filtros) => api(conFiltros(`OrdenMantenimiento/${pagina}/${tamano}`, filtros))
export const obtenerOrden = (id) => api(`OrdenMantenimiento/${id}`)
export const historialOrden = (id, pagina = 1, tamano = 20) => api(`OrdenMantenimiento/${id}/historial/${pagina}/${tamano}`)

export const crearOrden = (datos) => api('OrdenMantenimiento', { method: 'POST', body: datos })
export const editarOrden = (id, datos) => api(`OrdenMantenimiento/${id}`, { method: 'PUT', body: datos })

const accion = (id, ruta, body = {}) => api(`OrdenMantenimiento/${id}/${ruta}`, { method: 'POST', body })
export const diagnosticarOrden = (id, datos) => accion(id, 'diagnosticar', datos)
export const aprobarOrden = (id, datos) => accion(id, 'aprobar', datos)
export const rechazarOrden = (id, observacion) => accion(id, 'rechazar', { observacion })
export const iniciarOrden = (id, idTecnicos, observacion) => accion(id, 'iniciar', { idTecnicos, observacion })
export const asignarTecnicos = (id, idTecnicos) => accion(id, 'tecnicos', { idTecnicos })
export const registrarHoras = (id, idTecnico, horas) => accion(id, `tecnicos/${idTecnico}/horas`, { horas })
export const registrarRepuestos = (id, repuestos) => accion(id, 'repuestos', { repuestos })
export const reanudarOrden = (id, observacion) => accion(id, 'reanudar', { observacion })
export const completarOrden = (id, observacion) => accion(id, 'completar', { observacion })
export const cancelarOrden = (id, observacion) => accion(id, 'cancelar', { observacion })

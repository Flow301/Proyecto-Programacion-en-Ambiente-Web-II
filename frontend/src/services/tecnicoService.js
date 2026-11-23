import { api } from '@/api/api'
import { conFiltros } from './query'

// Técnicos del taller y sus especialidades
export const listarTecnicosPagina = (pagina, tamano, filtros) => api(conFiltros(`Tecnico/${pagina}/${tamano}`, filtros))
export const obtenerTecnico = (id) => api(`Tecnico/${id}`)
// Activos y disponibles que tienen la especialidad: los elegibles para una orden
export const tecnicosDisponibles = (idEspecialidad) => api(`Tecnico/disponibles/${idEspecialidad}`)
export const crearTecnico = (datos) => api('Tecnico', { method: 'POST', body: datos })
export const actualizarTecnico = (id, datos) => api(`Tecnico/${id}`, { method: 'PUT', body: datos })
export const inactivarTecnico = (id) => api(`Tecnico/${id}`, { method: 'DELETE' })
export const asociarEspecialidad = (id, datos) => api(`Tecnico/${id}/especialidades`, { method: 'POST', body: datos })
export const quitarEspecialidad = (id, idEspecialidad) => api(`Tecnico/${id}/especialidades/${idEspecialidad}`, { method: 'DELETE' })

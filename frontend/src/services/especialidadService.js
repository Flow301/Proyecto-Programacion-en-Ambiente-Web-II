import { api } from '@/api/api'

// Especialidades del taller. Se gestionan con listado y ventana, sin pantalla de detalle (enunciado 2.2)
export const listarEspecialidades = () => api('Especialidad')
export const listarEspecialidadesPagina = (pagina, tamano) => api(`Especialidad/${pagina}/${tamano}`)
export const crearEspecialidad = (datos) => api('Especialidad', { method: 'POST', body: datos })
export const actualizarEspecialidad = (id, datos) => api(`Especialidad/${id}`, { method: 'PUT', body: datos })
export const eliminarEspecialidad = (id) => api(`Especialidad/${id}`, { method: 'DELETE' })

import { api } from '@/api/api'
import { conFiltros } from './query'

// Usuarios y roles (requerimientos 7.10). Todo es solo del Administrador.
// filtros: { idRol, estado: 'true' | 'false' }
export const listarUsuariosPagina = (pagina, tamano, filtros) => api(conFiltros(`Usuario/${pagina}/${tamano}`, filtros))
export const listarRoles = () => api('Usuario/roles')
export const crearUsuario = (datos) => api('Usuario', { method: 'POST', body: datos })
export const actualizarUsuario = (id, datos) => api(`Usuario/${id}`, { method: 'PUT', body: datos })
export const cambiarRol = (id, idRol) => api(`Usuario/${id}/rol`, { method: 'PUT', body: { idRol } })
export const resetearClave = (id, clave) => api(`Usuario/${id}/clave`, { method: 'PUT', body: { clave } })
export const desactivarUsuario = (id) => api(`Usuario/${id}`, { method: 'DELETE' })
export const reactivarUsuario = (id) => api(`Usuario/${id}/reactivar`, { method: 'POST' })

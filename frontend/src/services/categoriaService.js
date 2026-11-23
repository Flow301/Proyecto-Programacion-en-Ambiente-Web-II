import { api } from '@/api/api'

// Categorías de vehículo con su regla de mantenimiento preventivo
export const listarCategorias = () => api('CategoriaActivo')
export const listarCategoriasPagina = (pagina, tamano) => api(`CategoriaActivo/${pagina}/${tamano}`)
export const obtenerCategoria = (id) => api(`CategoriaActivo/${id}`)
export const crearCategoria = (datos) => api('CategoriaActivo', { method: 'POST', body: datos })
export const actualizarCategoria = (id, datos) => api(`CategoriaActivo/${id}`, { method: 'PUT', body: datos })
export const eliminarCategoria = (id) => api(`CategoriaActivo/${id}`, { method: 'DELETE' })

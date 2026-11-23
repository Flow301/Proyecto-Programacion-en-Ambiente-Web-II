import { api } from '@/api/api'

// Catálogos de consulta (requerimientos, "Catálogos de consulta"): llenan filtros, listas y etiquetas.
export const estadosOrden = () => api('Catalogo/estados-orden')
export const estadosActivo = () => api('Catalogo/estados-activo')

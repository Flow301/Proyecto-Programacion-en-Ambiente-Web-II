import { api } from '@/api/api'

// Catálogos de consulta (requerimientos, "Catálogos de consulta"): llenan filtros, listas y etiquetas.
export const estadosOrden = () => api('Catalogo/estados-orden')
export const estadosActivo = () => api('Catalogo/estados-activo')
export const variablesControl = () => api('Catalogo/variables-control')
export const nivelesCertificacion = () => api('Catalogo/niveles-certificacion')
export const tiposMantenimiento = () => api('Catalogo/tipos-mantenimiento')
export const prioridades = () => api('Catalogo/prioridades')

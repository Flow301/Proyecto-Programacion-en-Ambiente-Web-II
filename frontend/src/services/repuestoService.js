import { api } from '@/api/api'
import { conFiltros } from './query'

// Repuestos, su stock y sus proveedores
export const listarRepuestos = () => api('Repuesto')
export const listarRepuestosPagina = (pagina, tamano, filtros) => api(conFiltros(`Repuesto/${pagina}/${tamano}`, filtros))
export const obtenerRepuesto = (id) => api(`Repuesto/${id}`)
export const crearRepuesto = (datos) => api('Repuesto', { method: 'POST', body: datos })
export const actualizarRepuesto = (id, datos) => api(`Repuesto/${id}`, { method: 'PUT', body: datos })
export const inactivarRepuesto = (id) => api(`Repuesto/${id}`, { method: 'DELETE' })
export const registrarEntrada = (id, cantidad) => api(`Repuesto/${id}/entrada`, { method: 'POST', body: { cantidad } })
export const proveedorRecomendado = (id) => api(`Repuesto/${id}/proveedor-recomendado`)
export const proveedoresDeRepuesto = (id) => api(`Repuesto/${id}/proveedores`)
export const asociarProveedor = (id, datos) => api(`Repuesto/${id}/proveedores`, { method: 'POST', body: datos })
export const actualizarCondiciones = (id, idProveedor, datos) => api(`Repuesto/${id}/proveedores/${idProveedor}`, { method: 'PUT', body: datos })
export const quitarProveedor = (id, idProveedor) => api(`Repuesto/${id}/proveedores/${idProveedor}`, { method: 'DELETE' })

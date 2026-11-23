import { api } from '@/api/api'

// Proveedores. Se gestionan con listado y ventana, sin pantalla de detalle (enunciado 2.2)
export const listarProveedores = () => api('Proveedor')
export const listarProveedoresPagina = (pagina, tamano) => api(`Proveedor/${pagina}/${tamano}`)
export const crearProveedor = (datos) => api('Proveedor', { method: 'POST', body: datos })
export const actualizarProveedor = (id, datos) => api(`Proveedor/${id}`, { method: 'PUT', body: datos })
export const inactivarProveedor = (id) => api(`Proveedor/${id}`, { method: 'DELETE' })

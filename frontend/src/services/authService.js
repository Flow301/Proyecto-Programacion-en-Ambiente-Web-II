import { api } from '@/api/api'

// Cambio de la contraseña propia: cualquier rol, siempre sobre el usuario del token
export const cambiarClave = (claveActual, claveNueva) =>
  api('Auth/clave', { method: 'PUT', body: { claveActual, claveNueva } })

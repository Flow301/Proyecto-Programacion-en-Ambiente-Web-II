import { z } from 'zod'

// Mismos límites que LoginDto en la API: se valida antes de enviar
// Mismos límites que CambioClaveDto: la nueva entre 8 y 100 caracteres y distinta de la actual
export const cambioClaveSchema = z.object({
  claveActual: z.string().min(1, 'Ingrese su contraseña actual.').max(100, 'Máximo 100 caracteres.'),
  claveNueva: z.string().min(8, 'La nueva contraseña debe tener al menos 8 caracteres.').max(100, 'Máximo 100 caracteres.'),
  confirmacion: z.string().min(1, 'Repita la nueva contraseña.'),
})
  .refine((d) => d.claveNueva === d.confirmacion, { path: ['confirmacion'], message: 'No coincide con la nueva contraseña.' })
  .refine((d) => d.claveNueva !== d.claveActual, { path: ['claveNueva'], message: 'La nueva contraseña debe ser distinta de la actual.' })

export const loginSchema = z.object({
  usuario: z.string().trim().min(1, 'Ingrese su usuario.').max(50, 'Máximo 50 caracteres.'),
  clave: z.string().min(1, 'Ingrese su contraseña.').max(100, 'Máximo 100 caracteres.'),
})

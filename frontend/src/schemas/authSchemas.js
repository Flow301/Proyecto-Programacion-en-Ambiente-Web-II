import { z } from 'zod'

// Mismos límites que LoginDto en la API: se valida antes de enviar
export const loginSchema = z.object({
  usuario: z.string().trim().min(1, 'Ingrese su usuario.').max(50, 'Máximo 50 caracteres.'),
  clave: z.string().min(1, 'Ingrese su contraseña.').max(100, 'Máximo 100 caracteres.'),
})

import { toast } from 'sonner'

// Avisos con las reglas del encargo: el éxito es breve y se cierra solo (~5 s, se pausa con el mouse);
// el error no se cierra solo y muestra el mensaje de negocio de la API, nunca un código HTTP.
// Los avisos nunca son el único lugar de la información: la pantalla también cambia.
export function avisarExito(mensaje) {
  toast.success(mensaje, { duration: 5000 })
}

export function avisarError(error) {
  const mensaje = !error?.status
    ? 'No fue posible conectar con el servidor. Revise su conexión e intente de nuevo.'
    : error.status >= 500
      ? 'Ocurrió un error inesperado. Intente de nuevo; si sigue, avise al Administrador.'
      : error.message
  toast.error(mensaje, { duration: Infinity })
}

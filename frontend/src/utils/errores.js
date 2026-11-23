import { ErrorApi } from '@/api/api'

// Copia los errores por campo de un 400 de la API ({ "Placa": ["..."] }) al formulario de React Hook Form,
// junto a cada campo (enunciado Avance 2, 4.4). La API los nombra en PascalCase; el formulario, en camelCase.
// Pone el foco en el primero. Devuelve true si asignó alguno; si no, el error va en un aviso general.
export function aplicarErroresApi(error, setError, campos) {
  if (!(error instanceof ErrorApi) || error.status !== 400 || !error.data) return false
  let primero = true
  for (const [clave, mensajes] of Object.entries(error.data)) {
    const campo = campos.find((c) => c.toLowerCase() === clave.toLowerCase())
    if (!campo || !mensajes?.length) continue
    setError(campo, { type: 'server', message: mensajes[0] }, { shouldFocus: primero })
    primero = false
  }
  return !primero
}

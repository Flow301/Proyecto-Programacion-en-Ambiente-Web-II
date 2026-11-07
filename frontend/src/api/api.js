// Único módulo que llama a la API. Toda respuesta llega como Result<T>: isSuccess, status, title, detail y data.
const API_URL = import.meta.env.VITE_API_URL
const CLAVE_SESION = 'sgma.sesion'

export class ErrorApi extends Error {
  constructor(status, title, detail, data) {
    super(detail || title)
    this.status = status
    this.title = title
    this.data = data // errores por campo cuando es un 400 de validación
  }
}

// La sesión es la data del login: { token, expira, usuario }. Vencida cuenta como inexistente.
export function leerSesion() {
  try {
    const sesion = JSON.parse(localStorage.getItem(CLAVE_SESION))
    return sesion && new Date(sesion.expira) > new Date() ? sesion : null
  } catch {
    return null
  }
}

export function guardarSesion(sesion) {
  localStorage.setItem(CLAVE_SESION, JSON.stringify(sesion))
}

export function borrarSesion() {
  localStorage.removeItem(CLAVE_SESION)
}

// AuthProvider registra aquí qué hacer cuando la API rechaza el token (401)
let alExpirarSesion = () => {}
export function onSesionExpirada(funcion) {
  alExpirarSesion = funcion
}

export async function api(ruta, { method = 'GET', body } = {}) {
  const sesion = leerSesion()
  const headers = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  if (sesion) headers.Authorization = `Bearer ${sesion.token}`

  let respuesta
  try {
    respuesta = await fetch(`${API_URL}/api/${ruta}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ErrorApi(0, 'Sin conexión', 'No se pudo conectar con la API. Intente de nuevo.')
  }

  const result = await respuesta.json().catch(() => null)

  // Con sesión, un 401 significa token vencido o inválido: se cierra la sesión y se vuelve al login
  if (respuesta.status === 401 && sesion) {
    borrarSesion()
    alExpirarSesion()
  }

  if (!respuesta.ok) {
    throw new ErrorApi(
      respuesta.status,
      result?.title ?? 'Error',
      result?.detail ?? 'La API respondió con un error inesperado.',
      result?.data,
    )
  }
  return result
}

export const login = (usuario, clave) =>
  api('Auth/login', { method: 'POST', body: { login: usuario, clave } })

export const getPerfil = () => api('Auth/perfil')

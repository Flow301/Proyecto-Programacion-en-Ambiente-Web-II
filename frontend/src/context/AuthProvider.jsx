import { useCallback, useEffect, useState } from 'react'
import { borrarSesion, guardarSesion, leerSesion, login, onSesionExpirada } from '../api/api'
import { AuthContext } from './AuthContext'

export default function AuthProvider({ children }) {
  const [sesion, setSesion] = useState(leerSesion)
  const [expirada, setExpirada] = useState(false)

  // Sin sesión, RutaProtegida manda al login; el aviso de "sesión expirada" viaja en el contexto
  const cerrarSesion = useCallback((porExpiracion = false) => {
    borrarSesion()
    setExpirada(porExpiracion)
    setSesion(null)
  }, [])

  // La API rechazó el token (401)
  useEffect(() => onSesionExpirada(() => cerrarSesion(true)), [cerrarSesion])

  // El token vence aunque no se haga ninguna petición
  useEffect(() => {
    if (!sesion) return
    const temporizador = setTimeout(() => cerrarSesion(true), new Date(sesion.expira) - Date.now())
    return () => clearTimeout(temporizador)
  }, [sesion, cerrarSesion])

  const iniciarSesion = async (usuario, clave) => {
    const { data } = await login(usuario, clave)
    guardarSesion(data)
    setExpirada(false)
    setSesion(data)
  }

  return (
    <AuthContext.Provider value={{ sesion, usuario: sesion?.usuario, expirada, iniciarSesion, cerrarSesion }}>
      {children}
    </AuthContext.Provider>
  )
}

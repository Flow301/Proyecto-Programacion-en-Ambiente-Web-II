import { useCallback, useEffect, useState } from 'react'
import { toast } from 'sonner'
import { borrarSesion, guardarSesion, leerSesion, login, onSesionExpirada, sesionVencida } from '../api/api'
import { AuthContext } from './AuthContext'

export default function AuthProvider({ children }) {
  const [sesion, setSesion] = useState(leerSesion)
  const [expirada, setExpirada] = useState(sesionVencida)
  // Tras "Cerrar sesión" no se recuerda la pantalla: quien ingrese después puede ser otra persona, con otro rol
  const [salidaManual, setSalidaManual] = useState(false)

  // Sin sesión, RutaProtegida manda al login; el aviso de "sesión expirada" viaja en el contexto
  const cerrarSesion = useCallback((porExpiracion = false) => {
    borrarSesion()
    toast.dismiss() // los avisos de error no se cierran solos: no deben quedar a la vista del siguiente usuario
    setExpirada(porExpiracion)
    setSalidaManual(!porExpiracion)
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
    setSalidaManual(false)
    setSesion(data)
  }

  return (
    <AuthContext.Provider value={{ sesion, usuario: sesion?.usuario, expirada, salidaManual, iniciarSesion, cerrarSesion }}>
      {children}
    </AuthContext.Provider>
  )
}

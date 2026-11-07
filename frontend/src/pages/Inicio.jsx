import { useEffect, useState } from 'react'
import { getPerfil } from '../api/api'
import { useAuth } from '../context/AuthContext'

// shortcut: pantalla mínima para probar el token; la plantilla con el menú por rol es de la semana 9
export default function Inicio() {
  const { cerrarSesion } = useAuth()
  const [perfil, setPerfil] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    getPerfil()
      .then((result) => setPerfil(result.data))
      .catch((err) => setError(err.message))
  }, [])

  return (
    <main className="inicio">
      <header>
        <strong>SGMA</strong>
        <button onClick={() => cerrarSesion()}>Cerrar sesión</button>
      </header>
      {error && <p className="error" role="alert">{error}</p>}
      {!perfil && !error && <p>Cargando…</p>}
      {perfil && (
        <p>
          Bienvenido, {perfil.nombreCompleto} ({perfil.rolNombre}).
        </p>
      )}
    </main>
  )
}

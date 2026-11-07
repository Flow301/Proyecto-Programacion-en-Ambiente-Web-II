import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { useAuth } from '../context/AuthContext'

export default function Login() {
  const { sesion, expirada, iniciarSesion } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [form, setForm] = useState({ usuario: '', clave: '' })
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  if (sesion) return <Navigate to="/" replace />

  const cambiar = (e) => setForm({ ...form, [e.target.name]: e.target.value })

  const enviar = async (e) => {
    e.preventDefault()
    setError('')
    setEnviando(true)
    try {
      await iniciarSesion(form.usuario.trim(), form.clave)
      navigate(location.state?.desde ?? '/', { replace: true })
    } catch (err) {
      setError(err.message)
      setEnviando(false)
    }
  }

  return (
    <main className="login">
      <form className="tarjeta" onSubmit={enviar}>
        <h1>SGMA</h1>
        <p className="subtitulo">Sistema de Gestión de Mantenimiento de Activos</p>

        {expirada && !error && (
          <p className="aviso" role="status">Su sesión expiró. Inicie sesión de nuevo.</p>
        )}
        {error && <p className="error" role="alert">{error}</p>}

        <label htmlFor="usuario">Usuario</label>
        <input id="usuario" name="usuario" value={form.usuario} onChange={cambiar}
          required maxLength={50} autoComplete="username" autoFocus />

        <label htmlFor="clave">Contraseña</label>
        <input id="clave" name="clave" type="password" value={form.clave} onChange={cambiar}
          required maxLength={100} autoComplete="current-password" />

        <button type="submit" disabled={enviando}>
          {enviando ? 'Ingresando…' : 'Iniciar sesión'}
        </button>
      </form>
    </main>
  )
}

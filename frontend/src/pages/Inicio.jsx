import { useEffect, useState } from 'react'
import { LogOut } from 'lucide-react'
import { getPerfil } from '@/api/api'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/context/AuthContext'

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
    <div className="min-h-svh">
      <header className="flex items-center justify-between bg-primary px-6 py-3 text-primary-foreground">
        <strong>SGMA</strong>
        <Button variant="secondary" onClick={() => cerrarSesion()}>
          <LogOut /> Cerrar sesión
        </Button>
      </header>
      <main className="p-6">
        {error && <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>}
        {!perfil && !error && <p>Cargando…</p>}
        {perfil && <p>Bienvenido, {perfil.nombreCompleto} ({perfil.rolNombre}).</p>}
      </main>
    </div>
  )
}

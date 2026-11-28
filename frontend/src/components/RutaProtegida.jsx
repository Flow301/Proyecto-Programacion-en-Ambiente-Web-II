import { Navigate, Outlet, useLocation } from 'react-router'
import { useAuth } from '../context/AuthContext'
import PaginaError from './shared/PaginaError'

// Sin sesión, manda al login y recuerda a dónde quería ir (salvo después de "Cerrar sesión").
// Con roles, un rol que no está en la lista ve "Acceso no autorizado", también si escribió la ruta a mano.
export default function RutaProtegida({ roles }) {
  const { sesion, usuario, salidaManual } = useAuth()
  const location = useLocation()
  if (!sesion) return <Navigate to="/login" replace state={salidaManual ? undefined : { desde: location.pathname + location.search }} />
  if (roles && !roles.includes(usuario?.rolNombre)) return <PaginaError codigo={403} />
  return <Outlet />
}

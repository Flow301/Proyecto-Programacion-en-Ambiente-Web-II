import { Navigate, Outlet, useLocation } from 'react-router'
import { useAuth } from '../context/AuthContext'

// Sin sesión, manda al login y recuerda a dónde quería ir
export default function RutaProtegida() {
  const { sesion } = useAuth()
  const location = useLocation()
  return sesion ? <Outlet /> : <Navigate to="/login" replace state={{ desde: location.pathname }} />
}

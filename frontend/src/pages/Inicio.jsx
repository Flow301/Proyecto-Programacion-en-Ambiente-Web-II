import { Navigate } from 'react-router'
import { ROLES, inicioDelRol } from '@/components/layout/menu'
import { useAuth } from '@/context/AuthContext'
import Dashboard from './Dashboard'

// Pantalla de inicio según el rol: el Administrador entra al Dashboard (reporte A1);
// el Coordinador/Técnico y el Solicitante/Consulta, a sus órdenes.
export default function Inicio() {
  const { usuario } = useAuth()
  if (usuario?.rolNombre !== ROLES.administrador) return <Navigate to={inicioDelRol(usuario?.rolNombre)} replace />
  return <Dashboard />
}

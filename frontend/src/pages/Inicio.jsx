import { Navigate } from 'react-router'
import { ROLES, inicioDelRol } from '@/components/layout/menu'
import { useAuth } from '@/context/AuthContext'
import PaginaPendiente from './PaginaPendiente'

// Pantalla de inicio según el rol (enunciado Avance 2, 4.2): el Administrador entra al Dashboard;
// el Coordinador/Técnico y el Solicitante/Consulta, a sus órdenes.
export default function Inicio() {
  const { usuario } = useAuth()
  if (usuario?.rolNombre !== ROLES.administrador) return <Navigate to={inicioDelRol(usuario?.rolNombre)} replace />
  // shortcut: el Dashboard se construye en la semana 11 (plan de trabajo)
  return <PaginaPendiente titulo="Inicio" />
}

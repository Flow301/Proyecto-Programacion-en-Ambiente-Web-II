import { ROLES } from '@/components/layout/menu'
import { useAuth } from '@/context/AuthContext'
import PaginaPendiente from './PaginaPendiente'

// Listado de órdenes. El Solicitante/Consulta lo ve como "Mis solicitudes" (enunciado Avance 2, 5.3),
// el mismo nombre que tiene en su menú.
export default function Ordenes() {
  const { usuario } = useAuth()
  const titulo = usuario?.rolNombre === ROLES.solicitante ? 'Mis solicitudes' : 'Órdenes'
  // shortcut: el listado se construye en la semana 11 (plan de trabajo)
  return <PaginaPendiente titulo={titulo} />
}

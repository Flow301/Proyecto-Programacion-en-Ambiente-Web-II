import { ROLES } from '@/components/layout/menu'
import { useAuth } from '@/context/AuthContext'

// El rol del usuario de la sesión, para mostrar u ocultar acciones (enunciado Avance 2, 4.1: sin botones
// que el rol no puede usar). La API valida el rol de todos modos; esto solo evita mostrar lo que no aplica.
export default function useRol() {
  const { usuario } = useAuth()
  const rol = usuario?.rolNombre
  return {
    rol,
    esAdmin: rol === ROLES.administrador,
    esCoordinador: rol === ROLES.coordinador,
    esSolicitante: rol === ROLES.solicitante,
  }
}

import { Link } from 'react-router'
import { MapPinOff, ShieldX, TriangleAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'
import EncabezadoPagina from './EncabezadoPagina'

// Página de error dentro de la estructura base (enunciado Avance 2, 4.5):
// 403 cuando el rol no tiene acceso (también si la ruta se escribe a mano) y 404 cuando no existe.
const tipos = {
  403: {
    Icono: ShieldX,
    titulo: 'Acceso no autorizado',
    mensaje: 'Su rol no tiene acceso a esta pantalla. Si necesita usarla, solicítelo al Administrador.',
  },
  404: {
    Icono: MapPinOff,
    titulo: 'No encontramos esta página',
    mensaje: 'La dirección no existe o el registro ya no está disponible. Revise el enlace o vuelva al inicio.',
  },
  error: {
    Icono: TriangleAlert,
    titulo: 'Algo salió mal',
    mensaje: 'No se pudo mostrar esta pantalla. Intente de nuevo; si el problema sigue, avise al Administrador.',
  },
}

export default function PaginaError({ codigo = 404, mensaje }) {
  const tipo = tipos[codigo] ?? tipos.error
  return (
    <div className="flex max-w-2xl flex-col gap-5 py-6">
      <tipo.Icono className="size-10 text-muted-foreground" aria-hidden="true" />
      <EncabezadoPagina titulo={tipo.titulo} detalle={mensaje ?? tipo.mensaje} />
      <Button asChild className="w-fit">
        <Link to="/">Volver al inicio</Link>
      </Button>
    </div>
  )
}

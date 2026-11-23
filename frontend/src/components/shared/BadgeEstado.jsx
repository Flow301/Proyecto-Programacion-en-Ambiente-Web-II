import {
  ArrowDown, ArrowUp, Award, BadgeCheck, Ban, Check, CircleAlert, CircleCheckBig, CircleDashed, CircleX, Clock,
  FileText, Flag, Minus, OctagonAlert, OctagonX, Search, TriangleAlert, Wrench,
} from 'lucide-react'
import { cn } from '@/lib/utils'

// Un solo badge para todo estado del sistema (enunciado Avance 2, 4.3): color + ícono + texto, nunca solo color.
// El estado llega como lo nombra la API; aquí solo se decide cómo se ve. El mismo significado, el mismo color.
const familias = {
  pendiente: 'bg-pendiente-tinte text-pendiente-texto',
  revision: 'bg-revision-tinte text-revision-texto',
  autorizado: 'bg-autorizado-tinte text-autorizado-texto',
  proceso: 'bg-proceso-tinte text-proceso-texto',
  espera: 'bg-espera-tinte text-espera-texto',
  exito: 'bg-exito-tinte text-exito-texto',
  alto: 'bg-alto-tinte text-alto-texto',
  cerrado: 'bg-cerrado-tinte text-cerrado-texto',
  aviso: 'bg-aviso-tinte text-aviso-texto',
  info: 'bg-accent text-accent-foreground',
}

const estados = {
  // Orden
  'Solicitada': ['pendiente', FileText],
  'Diagnosticada': ['revision', Search],
  'Aprobada': ['autorizado', BadgeCheck],
  'En Ejecución': ['proceso', Wrench],
  'En Espera de Repuesto': ['espera', Clock],
  'Completada': ['exito', Flag],
  'Rechazada': ['alto', CircleX],
  'Cancelada': ['cerrado', Ban],
  // Vehículo
  'Operativo': ['exito', CircleCheckBig],
  'En Mantenimiento': ['proceso', Wrench],
  'Fuera de Servicio': ['alto', OctagonX],
  'Dado de Baja': ['cerrado', Ban],
  // Mantenimiento preventivo
  'Vencido': ['alto', OctagonAlert],
  'Próximo': ['aviso', TriangleAlert],
  'Al día': ['exito', CircleCheckBig],
  'Sin lecturas': ['cerrado', CircleDashed],
  'Orden en curso': ['revision', Wrench],
  'En taller': ['revision', Wrench],
  // Otros
  'Activo': ['exito', CircleCheckBig],
  'Inactivo': ['cerrado', Ban],
  'Disponible': ['exito', CircleCheckBig],
  'No disponible': ['cerrado', Minus],
  'Consumido': ['cerrado', Check],
  'Pendiente': ['espera', Clock],
  'Stock bajo': ['alto', CircleAlert],
  'Recomendado': ['info', Award],
  'Por debajo': ['exito', ArrowDown],
  'Dentro': ['cerrado', Minus],
  'Por encima': ['alto', ArrowUp],
}

export default function BadgeEstado({ estado, children, className }) {
  const [familia, Icono] = estados[estado] ?? ['cerrado', CircleDashed]
  return (
    <span
      className={cn(
        'inline-flex h-[26px] w-fit shrink-0 items-center gap-1.5 rounded-sm px-2 text-sm font-semibold whitespace-nowrap',
        familias[familia],
        className,
      )}
    >
      <Icono className="size-4 shrink-0" aria-hidden="true" />
      {children ?? estado}
    </span>
  )
}

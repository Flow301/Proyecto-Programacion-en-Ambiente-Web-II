import { Link } from 'react-router'
import {
  BadgeCheck, Ban, CircleX, Clock, FileText, Flag, OctagonAlert, Search, TriangleAlert, Wrench,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { formatNumero } from '@/utils/format'

// Celda de indicador del dashboard: un conteo por estado que lleva al listado filtrado.
// No es una tarjeta con sombra: van juntas en una franja (FranjaIndicadores) en el orden de la ruta.
const apariencia = {
  'Solicitada': ['text-pendiente-texto', FileText],
  'Diagnosticada': ['text-revision-texto', Search],
  'Aprobada': ['text-autorizado-texto', BadgeCheck],
  'En Ejecución': ['text-proceso-texto', Wrench],
  'En Espera de Repuesto': ['text-espera-texto', Clock],
  'Completada': ['text-exito-texto', Flag],
  'Rechazada': ['text-alto-texto', CircleX],
  'Cancelada': ['text-cerrado-texto', Ban],
  'Vencido': ['text-alto-texto', OctagonAlert],
  'Próximo': ['text-aviso-texto', TriangleAlert],
}

/**
 * - estado: como lo nombra la API; define el ícono y el color.
 * - cantidad: el conteo calculado por la API.
 * - detalle: una línea de contexto ("desde 19/11/2026").
 * - to: el listado filtrado por ese estado.
 * - cerrado: los estados finales se ven más quietos.
 */
export default function TarjetaIndicador({ estado, cantidad, detalle, to, cerrado = false }) {
  const [color, Icono] = apariencia[estado] ?? ['text-foreground', FileText]
  // La cifra va primero: todas quedan en la misma línea aunque el nombre del estado ocupe dos
  return (
    <Link
      to={to}
      className={cn(
        'flex min-w-0 flex-col gap-1 px-4 text-foreground tabular-nums transition-colors hover:bg-muted/40',
        cerrado ? 'flex-row items-baseline gap-x-2 rounded-sm px-2 py-1.5' : '-mr-px -mb-px border-r border-b border-border-suave pt-3.5 pb-4',
      )}
    >
      <span className={cn('font-bold leading-tight', cerrado ? 'text-xl text-muted-foreground' : 'text-3xl')}>{formatNumero(cantidad)}</span>
      <span className={cn('flex items-start gap-1.5 text-sm font-semibold hyphens-auto', color)}>
        <Icono className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
        <span className="min-w-0">{estado}</span>
      </span>
      {detalle && <span className="text-sm text-muted-foreground">{detalle}</span>}
    </Link>
  )
}

// Franja que agrupa las celdas: arriba las abiertas, que requieren atención; debajo, en una fila baja
// rotulada una sola vez, las cerradas. Si no caben, las celdas pasan a otra línea.
export function FranjaIndicadores({ abiertas, cerradas, className }) {
  return (
    <div className={cn('overflow-hidden border-t border-border-suave', className)}>
      <div className="grid grid-cols-[repeat(auto-fit,minmax(9rem,1fr))]">{abiertas}</div>
      <div className="flex flex-wrap items-center gap-x-4 gap-y-1 border-t-2 border-recorrido px-4 py-1.5">
        <span className="text-sm font-semibold text-muted-foreground">Cerradas</span>
        {cerradas}
      </div>
    </div>
  )
}

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
  return (
    <Link
      to={to}
      className={cn('flex flex-col gap-1 border-r border-border-suave px-4 pt-3.5 pb-4 text-foreground tabular-nums transition-colors last:border-r-0 hover:bg-muted/40', cerrado && 'bg-muted/25')}
    >
      <span className={cn('flex items-center gap-1.5 text-sm font-semibold', color)}>
        <Icono className="size-4 shrink-0" aria-hidden="true" />
        {estado}
      </span>
      <span className={cn('font-bold leading-tight', cerrado ? 'text-2xl text-muted-foreground' : 'text-3xl')}>{formatNumero(cantidad)}</span>
      {detalle && <span className="text-sm text-muted-foreground">{detalle}</span>}
    </Link>
  )
}

// Franja que agrupa las celdas; la línea oscura separa las abiertas de las cerradas.
export function FranjaIndicadores({ abiertas, cerradas, className }) {
  return (
    <div className={cn('grid border-t border-border-suave', className)} style={{ gridTemplateColumns: `repeat(${abiertas.length}, minmax(0, 1.15fr)) 2px repeat(${cerradas.length}, minmax(0, 1fr))` }}>
      {abiertas}
      <span aria-hidden="true" className="bg-recorrido" />
      {cerradas}
    </div>
  )
}

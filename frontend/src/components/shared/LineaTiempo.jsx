import { Link } from 'react-router'
import { cn } from '@/lib/utils'
import { formatFechaHora } from '@/utils/format'
import BadgeEstado from './BadgeEstado'

/**
 * Línea de tiempo del historial de estados (enunciado Avance 2, 5.7), del más reciente al más antiguo,
 * tal como lo devuelve la API. Solo lectura: no hay botones de agregar, editar ni eliminar.
 * eventos: [{ id, fecha, usuario, anterior, nuevo, observacion?, orden?: { codigo, to } }]
 * En el historial del vehículo, un cambio automático muestra la orden que lo provocó.
 */
export default function LineaTiempo({ eventos, className }) {
  return (
    <ol className={cn('flex flex-col', className)}>
      {eventos.map((e, i) => {
        const ultimo = i === eventos.length - 1
        return (
          <li key={e.id} className={cn('relative border-l-2 pl-5', ultimo ? 'border-transparent' : 'border-border pb-4')}>
            <span aria-hidden="true" className={cn('absolute top-1.5 -left-[6px] size-2.5 rounded-full', i === 0 ? 'bg-primary' : 'bg-input')} />
            <p className="text-sm text-muted-foreground tabular-nums">
              <time dateTime={e.fecha}>{formatFechaHora(e.fecha)}</time> · {e.usuario}
            </p>
            <p className="mt-1.5 flex flex-wrap items-center gap-1.5">
              {e.anterior ? <BadgeEstado estado={e.anterior} /> : <span className="text-sm text-muted-foreground">Registro inicial</span>}
              <span aria-hidden="true">→</span>
              <span className="sr-only">pasó a</span>
              <BadgeEstado estado={e.nuevo} />
            </p>
            {e.orden && (
              <p className="mt-1 text-sm">
                Por la orden <Link to={e.orden.to} className="font-semibold text-primary underline underline-offset-4 hover:no-underline">{e.orden.codigo}</Link>
              </p>
            )}
            {e.observacion && <p className="mt-1 max-w-prose text-sm">{e.observacion}</p>}
          </li>
        )
      })}
    </ol>
  )
}

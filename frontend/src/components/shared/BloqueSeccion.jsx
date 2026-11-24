import { useId } from 'react'
import { cn } from '@/lib/utils'

/**
 * Bloque de sección del detalle (Diagnóstico, Técnicos y horas, Repuestos, Costos, Historial...).
 * Superficie blanca con borde de 1 px, sin sombra; el título es un h2 y las acciones van a su derecha.
 * Con sinRelleno, el contenido (una tabla) llega a los bordes.
 */
export default function BloqueSeccion({ titulo, detalle, acciones, sinRelleno = false, children, className }) {
  const id = useId()
  return (
    <section aria-labelledby={id} className={cn('rounded-sm border bg-card', className)}>
      <div className={cn('flex flex-wrap items-center justify-between gap-x-4 gap-y-2 px-6 pt-4', sinRelleno ? 'pb-2' : 'pb-3')}>
        <div className="flex flex-col gap-0.5">
          <h2 id={id} className="text-lg font-semibold">{titulo}</h2>
          {detalle && <div className="text-sm text-muted-foreground">{detalle}</div>}
        </div>
        {acciones && <div className="flex flex-wrap items-center gap-2">{acciones}</div>}
      </div>
      <div className={sinRelleno ? '' : 'px-6 pb-5'}>{children}</div>
    </section>
  )
}

import { cn } from '@/lib/utils'
import { formatKm, formatPorcentaje } from '@/utils/format'

// Barra del mantenimiento preventivo: un tramo de kilómetros con dos mojones, el aviso y el umbral (100 %).
// La escala va de 0 a 150 % para que un vehículo vencido se vea pasado del umbral.
// El porcentaje, el aviso y el estado los calcula la API; aquí solo se dibujan.
const ESCALA = 150

const relleno = {
  'Vencido': 'bg-alto',
  'Próximo': 'bg-aviso ring-1 ring-inset ring-aviso-borde',
  'Al día': 'bg-exito',
}

const posicion = (porcentaje) => `${(Math.min(Math.max(porcentaje, 0), ESCALA) / ESCALA) * 100}%`

/**
 * @param porcentaje      porcentaje del umbral consumido (API)
 * @param porcentajeAviso porcentaje desde el que el vehículo queda Próximo (API, de la categoría)
 * @param estado          Vencido, Próximo, Al día o Sin lecturas (API)
 * @param recorrido       uso acumulado desde el último preventivo (API)
 * @param umbral          intervalo del preventivo (API)
 * @param unidad          unidad de la variable de control (API: "km")
 * @param compacta        sin las cifras, para tablas angostas
 */
export default function BarraPreventivo({ porcentaje, porcentajeAviso, estado, recorrido, umbral, unidad = 'km', compacta = false, className }) {
  const sinLecturas = estado === 'Sin lecturas' || porcentaje == null

  return (
    <div className={cn('flex items-center gap-3 tabular-nums', className)}>
      <div aria-hidden="true" className={cn('relative h-3 flex-1 rounded-[2px]', sinLecturas ? 'border border-dashed border-input bg-card' : 'bg-muted', compacta ? 'min-w-28' : 'min-w-40')}>
        {!sinLecturas && (
          <>
            <span className={cn('absolute inset-y-0 left-0 rounded-[2px]', relleno[estado] ?? 'bg-cerrado')} style={{ width: posicion(porcentaje) }} />
            {porcentajeAviso != null && (
              <span className="absolute -inset-y-1.5 w-0.5 bg-aviso-borde" style={{ left: posicion(porcentajeAviso) }} />
            )}
            <span className="absolute -inset-y-1.5 w-0.5 bg-recorrido" style={{ left: posicion(100) }} />
          </>
        )}
      </div>
      {sinLecturas ? (
        <span className="text-sm text-muted-foreground">Sin lecturas registradas</span>
      ) : compacta ? (
        <strong className="w-16 text-right text-[0.9375rem]">{formatPorcentaje(porcentaje)}</strong>
      ) : (
        <span className="text-[0.9375rem] whitespace-nowrap">
          {formatKm(recorrido, unidad)} de {formatKm(umbral, unidad)} · <strong>{formatPorcentaje(porcentaje)}</strong>
        </span>
      )}
    </div>
  )
}

import { Ban, BadgeCheck, Check, CircleX, Clock, FileText, Flag, Search, Wrench } from 'lucide-react'
import { cn } from '@/lib/utils'
import { formatFecha } from '@/utils/format'

// Ruta de la orden: los pasos del enunciado (4.3) como mojones sobre una carretera.
// - Postes recorridos en gris asfalto; el actual, más grande y en el color de su estado.
// - En Espera de Repuesto cuelga como un desvío del poste En Ejecución.
// - Cancelada y Rechazada cortan la ruta donde se detuvo; lo que no ocurrió queda visible y apagado.
// - Al completarse, toda la ruta pasa a verde.
// No decide nada: dibuja el estado y las fechas que manda la API (historial de la orden).
const PASOS = ['Solicitada', 'Diagnosticada', 'Aprobada', 'En Ejecución', 'Completada']

const actual = {
  'Solicitada': { Icono: FileText, poste: 'bg-pendiente ring-pendiente-tinte', texto: 'text-pendiente-texto' },
  'Diagnosticada': { Icono: Search, poste: 'bg-revision ring-revision-tinte', texto: 'text-revision-texto' },
  'Aprobada': { Icono: BadgeCheck, poste: 'bg-autorizado ring-autorizado-tinte', texto: 'text-autorizado-texto' },
  'En Ejecución': { Icono: Wrench, poste: 'bg-proceso ring-proceso-tinte', texto: 'text-proceso-texto' },
  'Completada': { Icono: Flag, poste: 'bg-exito ring-exito-tinte', texto: 'text-exito-texto' },
}

const corte = {
  'Cancelada': { Icono: Ban, fondo: 'bg-cerrado', texto: 'text-cerrado-texto' },
  'Rechazada': { Icono: CircleX, fondo: 'bg-alto', texto: 'text-alto-texto' },
}

const POSTE = 'relative z-[1] grid place-items-center rounded-t-full rounded-b-[3px]'

/**
 * @param estado       estado actual de la orden, como lo nombra la API
 * @param fechas       { [estado]: fecha } con la fecha en que la orden llegó a cada estado (del historial)
 * @param diasEnEstado días en el estado actual (campo calculado de la API)
 * @param detenidaEn   último paso alcanzado antes de Cancelada o Rechazada (estado anterior en el historial)
 * @param detalleEspera qué falta mientras está En Espera de Repuesto (texto o nodo)
 */
export default function ProgresoOrden({ estado, fechas = {}, diasEnEstado, detenidaEn, detalleEspera }) {
  const enEspera = estado === 'En Espera de Repuesto'
  const cortada = corte[estado]
  const completada = estado === 'Completada'
  const indiceCorte = cortada ? Math.max(PASOS.indexOf(detenidaEn), 0) : -1
  const indiceActual = enEspera ? PASOS.indexOf('En Ejecución') : cortada ? indiceCorte : PASOS.indexOf(estado)
  const dias = diasEnEstado == null ? '' : ` · ${diasEnEstado} ${diasEnEstado === 1 ? 'día' : 'días'}`

  return (
    <ol aria-label="Progreso de la orden" className="grid grid-cols-5 tabular-nums">
      {PASOS.map((paso, i) => {
        const esActual = i === indiceActual && !enEspera && !cortada
        const recorrido = i < indiceActual || (i === indiceActual && (enEspera || cortada)) || completada
        const noOcurrio = cortada && i > indiceCorte
        const ultimo = i === PASOS.length - 1
        const fecha = fechas[paso]
        const { Icono, poste, texto } = actual[paso]

        // Tramo hacia el siguiente poste
        let tramo = null
        if (!ultimo && !(cortada && i > indiceCorte)) {
          if (cortada && i === indiceCorte) tramo = <span aria-hidden="true" className="absolute top-[15px] left-1/2 h-1 w-1/2 bg-recorrido" />
          else if (i < indiceActual || completada) tramo = <span aria-hidden="true" className={cn('absolute top-[15px] left-1/2 h-1 w-full', completada ? 'bg-exito' : 'bg-recorrido')} />
          else tramo = <span aria-hidden="true" className="absolute top-4 left-1/2 w-full border-t-[3px] border-dashed border-input/50" />
        }

        return (
          <li
            key={paso}
            aria-current={esActual ? 'step' : undefined}
            className={cn('relative flex flex-col items-center gap-2 text-center', noOcurrio && 'opacity-50')}
          >
            {tramo}
            {esActual ? (
              <span className={cn(POSTE, '-mt-1.5 h-11 w-8 text-white ring-4', poste)} aria-hidden="true">
                <Icono className="size-4" />
              </span>
            ) : recorrido ? (
              <span className={cn(POSTE, 'h-[34px] w-6 text-white', completada ? 'bg-exito' : 'bg-recorrido')} aria-hidden="true">
                {completada && ultimo ? <Flag className="size-3.5" /> : <Check className="size-3.5" />}
              </span>
            ) : (
              <span className={cn(POSTE, 'h-[34px] w-6 border-2 border-input bg-card', noOcurrio && 'border-dotted')} aria-hidden="true" />
            )}

            <span className={cn('text-[0.9375rem] font-semibold', esActual && texto, !esActual && !recorrido && 'text-muted-foreground')}>
              {paso}
              <span className="sr-only">
                {esActual ? ', estado actual' : noOcurrio ? ', no ocurrió' : recorrido ? ', completado' : ', pendiente'}
              </span>
            </span>
            <span className="-mt-1.5 text-sm text-muted-foreground">
              {esActual ? `desde ${formatFecha(fecha)}${dias}` : noOcurrio ? 'no ocurrió' : fecha ? formatFecha(fecha) : recorrido ? '' : 'pendiente'}
            </span>

            {enEspera && i === indiceActual && (
              <div aria-current="step" className="flex flex-col items-center">
                <span aria-hidden="true" className="h-4 border-l-[3px] border-espera" />
                <div className="flex items-center gap-2.5 rounded-sm bg-espera-tinte px-3 py-2 text-left text-espera-texto">
                  <span className={cn(POSTE, 'h-[38px] w-7 shrink-0 bg-espera text-white')} aria-hidden="true">
                    <Clock className="size-4" />
                  </span>
                  <span>
                    <strong className="block text-[0.9375rem]">En Espera de Repuesto{dias}</strong>
                    {detalleEspera && <span className="block text-sm">{detalleEspera}</span>}
                  </span>
                </div>
              </div>
            )}

            {cortada && i === indiceCorte && (
              <span aria-current="step" className="absolute -top-1 left-full z-[2] flex -translate-x-1/2 flex-col items-center gap-1.5">
                <span className={cn('grid size-10 place-items-center rounded-full text-white ring-4 ring-card', cortada.fondo)} aria-hidden="true">
                  <cortada.Icono className="size-5" />
                </span>
                <strong className={cn('bg-card px-1.5 text-[0.9375rem]', cortada.texto)}>{estado}</strong>
                <span className="bg-card px-1.5 text-sm text-muted-foreground">{formatFecha(fechas[estado])}</span>
              </span>
            )}
          </li>
        )
      })}
    </ol>
  )
}

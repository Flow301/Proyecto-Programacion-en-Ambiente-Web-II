import { Ban, BadgeCheck, Check, CircleX, Clock, FileText, Flag, Search, Wrench } from 'lucide-react'
import { cn } from '@/lib/utils'
import { formatFecha, formatFechaCorta } from '@/utils/format'
import Separador from './Separador'

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

// Fecha completa cuando la ruta tiene espacio; día y mes cuando es angosta
function Fecha({ valor }) {
  return (
    <>
      <span className="@[40rem]:hidden">{formatFechaCorta(valor)}</span>
      <span className="hidden @[40rem]:inline">{formatFecha(valor)}</span>
    </>
  )
}

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
  const dias = diasEnEstado == null ? null : <> <Separador /> <span className="whitespace-nowrap">{diasEnEstado} {diasEnEstado === 1 ? 'día' : 'días'}</span></>

  return (
    // La ruta mide su propio ancho (container query): en un bloque angosto las fechas pierden el año
    <div className="@container">
    <ol aria-label="Progreso de la orden" className={cn('grid grid-cols-5 gap-x-3 tabular-nums', cortada && 'pt-9')}>
      {PASOS.map((paso, i) => {
        const esActual = i === indiceActual && !enEspera && !cortada
        const recorrido = i < indiceActual || (i === indiceActual && (enEspera || cortada)) || completada
        const noOcurrio = cortada && i > indiceCorte
        const ultimo = i === PASOS.length - 1
        const fecha = fechas[paso]
        const { Icono, poste, texto } = actual[paso]

        // Tramo hacia el siguiente poste
        let tramo = null
        if (noOcurrio && !ultimo) {
          tramo = <span aria-hidden="true" className="absolute top-4 left-1/2 w-full border-t-[3px] border-dotted border-input/35" />
        } else if (!ultimo) {
          if (cortada && i === indiceCorte) tramo = <span aria-hidden="true" className="absolute top-[15px] left-1/2 h-1 w-1/2 bg-recorrido" />
          else if (i < indiceActual || completada) tramo = <span aria-hidden="true" className={cn('absolute top-[15px] left-1/2 h-1 w-full', completada ? 'bg-exito' : 'bg-recorrido')} />
          else tramo = <span aria-hidden="true" className="absolute top-4 left-1/2 w-full border-t-[3px] border-dashed border-input/50" />
        }

        return (
          <li
            key={paso}
            aria-current={esActual ? 'step' : undefined}
            className="relative flex flex-col items-center gap-2 text-center"
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
              // Lo que no ocurrió se apaga en el poste; el texto conserva su contraste
              <span className={cn(POSTE, 'h-[34px] w-6 border-2 border-input bg-card', noOcurrio && 'border-dotted opacity-50')} aria-hidden="true" />
            )}

            <span className={cn('text-sm font-semibold text-balance @[40rem]:text-[0.9375rem]', esActual && texto, !esActual && !recorrido && 'text-muted-foreground')}>
              {paso}
              <span className="sr-only">
                {esActual ? ', estado actual' : noOcurrio ? ', no ocurrió' : recorrido ? ', completado' : ', pendiente'}
              </span>
            </span>
            <span className="-mt-1.5 text-sm text-muted-foreground">
              {esActual && !completada ? <>desde <Fecha valor={fecha} />{dias}</> : esActual ? <Fecha valor={fecha} /> : noOcurrio ? 'no ocurrió' : fecha ? <Fecha valor={fecha} /> : recorrido ? '' : 'pendiente'}
            </span>

            {enEspera && i === indiceActual && (
              <div aria-current="step" className="flex flex-col items-center">
                <span aria-hidden="true" className="h-4 border-l-[3px] border-espera" />
                <div className="flex w-max max-w-64 items-center gap-2.5 rounded-sm bg-espera-tinte px-3 py-2 text-left text-espera-texto">
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
              // La señal de corte va sobre la línea; su texto, encima, para no chocar con los nombres de los pasos
              <span aria-current="step" className="absolute -top-[3px] left-full z-[2] -translate-x-1/2">
                <span className="absolute bottom-full left-1/2 mb-1.5 -translate-x-1/2 text-center whitespace-nowrap">
                  <strong className={cn('text-[0.9375rem]', cortada.texto)}>{estado}</strong>
                  <span className="text-sm text-muted-foreground"> <Separador /> {formatFecha(fechas[estado])}</span>
                </span>
                <span className={cn('grid size-10 place-items-center rounded-full text-white ring-4 ring-card', cortada.fondo)} aria-hidden="true">
                  <cortada.Icono className="size-5" />
                </span>
              </span>
            )}
          </li>
        )
      })}
    </ol>
    </div>
  )
}

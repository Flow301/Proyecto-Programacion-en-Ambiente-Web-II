import { useCallback } from 'react'
import { useParams } from 'react-router'
import { RotateCcw } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import EstadoVacio from '@/components/shared/EstadoVacio'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import PaginaError from '@/components/shared/PaginaError'
import Separador from '@/components/shared/Separador'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import { cn } from '@/lib/utils'
import { obtenerCostosOrden } from '@/services/reporteService'
import { formatColones, formatFechaHora, formatHoras, formatNumero, formatPorcentaje } from '@/utils/format'

// Reporte de costos por orden (reporte B1). Se abre con el botón "Reporte de costos" del detalle de la orden.
// Una sola petición (GET api/Reporte/costos-orden/{id}): subtotales, porcentajes, promedio e indicador los calcula
// la API con consultas agregadas; aquí solo se muestran. Resumen arriba (totales y comparación), detalle abajo (tablas).
export default function ReporteCostos() {
  const { id } = useParams()
  const cargar = useCallback(() => obtenerCostosOrden(id), [id])
  const carga = useCarga(cargar)
  const volver = { to: `/ordenes/${id}`, texto: 'Volver a la orden' }

  if (carga.error?.status === 404 || carga.error?.status === 403) return <PaginaError codigo={carga.error.status} />
  if (carga.cargando && !carga.datos) return <><EncabezadoPagina titulo="Reporte de costos" volver={volver} /><IndicadorCarga forma="detalle" texto="Cargando el reporte de costos…" /></>
  if (carga.error) return <><EncabezadoPagina titulo="Reporte de costos" volver={volver} /><EstadoError error={carga.error} onReintentar={carga.recargar} /></>

  const r = carga.datos.data

  return (
    <>
      <EncabezadoPagina
        titulo="Reporte de costos"
        pestana={`Costos ${r.codigo}`}
        volver={volver}
        estado={<BadgeEstado estado={r.estadoOrdenNombre} />}
        detalle={<><strong>{r.codigo}</strong> <Separador /> {r.activoCodigo} {r.activoNombre} <Separador /> {r.tipoMantenimientoNombre}</>}
        acciones={(
          <Button variant="outline" onClick={carga.recargar} disabled={carga.cargando}>
            <RotateCcw aria-hidden="true" />
            Actualizar
          </Button>
        )}
      />

      <div className="grid items-start gap-5 xl:grid-cols-[3fr_2fr]">
        <BloqueSeccion titulo="Orden">
          <Datos filas={[
            ['Código', <strong key="codigo">{r.codigo}</strong>],
            ['Vehículo', <><strong>{r.activoCodigo}</strong> <Separador /> {r.activoNombre}</>],
            ['Tipo', r.tipoMantenimientoNombre],
            ['Prioridad', r.prioridadNombre ?? 'Sin asignar'],
            ['Estado', <BadgeEstado key="estado" estado={r.estadoOrdenNombre} />],
            ['Fecha de solicitud', formatFechaHora(r.fechaSolicitud)],
            ['Fecha de cierre', r.fechaCierre ? formatFechaHora(r.fechaCierre) : 'Sigue abierta'],
          ]} />
        </BloqueSeccion>

        <ResumenCostos r={r} />
      </div>

      <Comparacion r={r} />

    </>
  )
}

// Recuadro de totales destacado: el costo total con su etiqueta ("Costo final" o "Costo acumulado a la fecha")
// y la proporción de repuestos y de mano de obra. Los porcentajes vienen de la API y suman 100.
function ResumenCostos({ r }) {
  const partes = [
    { nombre: 'Repuestos', monto: r.subtotalRepuestos, porcentaje: r.porcentajeRepuestos, color: 'bg-primary' },
    { nombre: 'Mano de obra', monto: r.costoManoObra, porcentaje: r.porcentajeManoObra, color: 'bg-autorizado', extra: formatHoras(r.totalHoras) },
  ]
  return (
    <section aria-labelledby="titulo-total" className="flex flex-col gap-4 rounded-sm border-2 border-primary bg-card px-6 py-5 tabular-nums">
      <div className="flex flex-col gap-1">
        <h2 id="titulo-total" className="text-sm font-semibold text-muted-foreground">{r.etiquetaCosto}</h2>
        <p className="text-4xl font-bold">{formatColones(r.costoTotal)}</p>
        <p className="text-sm text-muted-foreground">Subtotal de repuestos + costo de mano de obra.</p>
      </div>

      {r.costoTotal > 0 ? (
        <div className="flex h-3 overflow-hidden rounded-full bg-muted" aria-hidden="true">
          {partes.map((p) => <div key={p.nombre} className={p.color} style={{ width: `${p.porcentaje}%` }} />)}
        </div>
      ) : (
        <p className="text-sm text-muted-foreground">La orden todavía no tiene repuestos consumidos ni horas registradas.</p>
      )}

      <dl className="flex flex-col gap-2.5 text-[0.9375rem]">
        {partes.map((p) => (
          <div key={p.nombre} className="flex items-baseline justify-between gap-4">
            <dt className="flex items-center gap-2">
              <span className={cn('size-3 shrink-0 rounded-[3px]', p.color)} aria-hidden="true" />
              {p.nombre}
              {p.extra && <span className="text-sm text-muted-foreground">({p.extra})</span>}
            </dt>
            <dd className="flex items-baseline gap-3">
              <span className="text-sm text-muted-foreground">{formatPorcentaje(p.porcentaje)}</span>
              <strong>{formatColones(p.monto)}</strong>
            </dd>
          </div>
        ))}
      </dl>
    </section>
  )
}

// Bloque de nivelación: costo de esta orden contra el promedio de las Completadas del mismo tipo y prioridad.
// El indicador (Por debajo / Dentro / Por encima) lo decide la API con la tolerancia del equipo; aquí lleva texto,
// ícono y color (BadgeEstado). Sin comparables, se dice así en lugar de mostrar ceros.
const TEXTO_INDICADOR = {
  'Por debajo': 'Por debajo de lo esperado',
  'Dentro': 'Dentro de lo esperado',
  'Por encima': 'Por encima de lo esperado',
}

function Comparacion({ r }) {
  const c = r.comparacion
  const grupo = `${r.tipoMantenimientoNombre} con prioridad ${r.prioridadNombre ?? 'sin asignar'}`
  const sinComparables = !TEXTO_INDICADOR[c.indicador]

  return (
    <BloqueSeccion
      titulo="Comparación con órdenes similares"
      detalle={`Órdenes Completadas de ${grupo}, sin contar esta.`}
      acciones={!sinComparables && <BadgeEstado estado={c.indicador} className="h-8 px-3 text-[0.9375rem]">{TEXTO_INDICADOR[c.indicador]}</BadgeEstado>}
    >
      {sinComparables ? (
        <EstadoVacio
          titulo="Sin órdenes comparables"
          mensaje={`No hay otras órdenes Completadas de ${grupo} con las cuales comparar el costo.`}
        />
      ) : (
        <div className="flex flex-col gap-4 tabular-nums">
          <dl className="grid grid-cols-2 overflow-hidden rounded-sm border md:grid-cols-4">
            <Cifra etiqueta="Costo promedio" valor={formatColones(c.costoPromedio)} />
            <Cifra etiqueta="Órdenes comparadas" valor={formatNumero(c.cantidadComparadas)} />
            <Cifra etiqueta="Diferencia" valor={conSigno(c.diferencia, formatColones)} />
            <Cifra etiqueta="Diferencia (%)" valor={conSigno(c.diferenciaPorcentaje, formatPorcentaje)} />
          </dl>
          <p className="max-w-prose text-[0.9375rem] text-muted-foreground">
            {r.etiquetaCosto === 'Costo final' ? 'Esta orden costó ' : 'Esta orden lleva acumulado '}
            <strong className="text-foreground">{formatColones(r.costoTotal)}</strong>
            {c.diferencia === 0
              ? ', igual que el promedio. '
              : <>, {formatColones(Math.abs(c.diferencia))} {c.diferencia > 0 ? 'más' : 'menos'} que el promedio. </>}
            Se considera dentro de lo esperado una diferencia de hasta ±{formatNumero(c.toleranciaPorcentaje)} %.
          </p>
        </div>
      )}
    </BloqueSeccion>
  )
}

function Cifra({ etiqueta, valor }) {
  return (
    <div className="-mr-px -mb-px flex flex-col gap-1 border-r border-b px-4 py-3">
      <dt className="text-sm text-muted-foreground">{etiqueta}</dt>
      <dd className="text-xl font-bold">{valor}</dd>
    </div>
  )
}

// +₡20 200 / −₡4 500: el signo hace evidente si está por encima o por debajo del promedio
function conSigno(valor, formato) {
  if (valor > 0) return `+${formato(valor)}`
  if (valor < 0) return `−${formato(Math.abs(valor))}`
  return formato(valor)
}

function Datos({ filas }) {
  return (
    <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2.5 text-[0.9375rem] tabular-nums">
      {filas.map(([etiqueta, valor]) => (
        <div key={etiqueta} className="contents">
          <dt className="text-muted-foreground">{etiqueta}</dt>
          <dd className="min-w-0">{valor}</dd>
        </div>
      ))}
    </dl>
  )
}

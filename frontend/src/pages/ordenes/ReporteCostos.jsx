import { useCallback } from 'react'
import { useParams } from 'react-router'
import { RotateCcw } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import PaginaError from '@/components/shared/PaginaError'
import Separador from '@/components/shared/Separador'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import { obtenerCostosOrden } from '@/services/reporteService'
import { formatFechaHora } from '@/utils/format'

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
      </div>

    </>
  )
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

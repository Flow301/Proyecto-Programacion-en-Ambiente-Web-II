import { Link } from 'react-router'
import { ArrowRight, RotateCcw } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraPreventivo, { LeyendaPreventivo } from '@/components/shared/BarraPreventivo'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import EstadoVacio from '@/components/shared/EstadoVacio'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import Separador from '@/components/shared/Separador'
import TarjetaIndicador, { FranjaIndicadores } from '@/components/shared/TarjetaIndicador'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import { obtenerDashboard } from '@/services/dashboardService'
import { formatFecha, formatNumero } from '@/utils/format'

// Dashboard general (enunciado Avance 2, 5.9, reporte A1): la pantalla de inicio del Administrador. Muestra de un
// vistazo lo que requiere atención: órdenes por estado, alertas de preventivo y repuestos bajo el mínimo.
// Una sola petición: todos los conteos y las listas los calcula la API (GET api/Dashboard); aquí solo se muestran.
export default function Dashboard() {
  const dashboard = useCarga(obtenerDashboard)
  const datos = dashboard.datos?.data

  return (
    <>
      <EncabezadoPagina
        titulo="Dashboard"
        detalle="Lo que requiere atención hoy. Se calcula en cada consulta con los datos actuales."
        acciones={(
          <Button variant="outline" onClick={dashboard.recargar} disabled={dashboard.cargando}>
            <RotateCcw aria-hidden="true" />
            Actualizar
          </Button>
        )}
      />

      {dashboard.error && <EstadoError error={dashboard.error} onReintentar={dashboard.recargar} />}

      {!dashboard.error && (
        <BloqueSeccion
          titulo="Órdenes por estado"
          detalle={datos && `${formatNumero(datos.totalOrdenes)} órdenes en total, ${formatNumero(datos.ordenesAbiertas)} abiertas. Cada estado abre el listado filtrado.`}
          sinRelleno
        >
          {dashboard.cargando || !datos
            ? <IndicadorCarga forma="bloque" filas={3} texto="Cargando el dashboard…" className="px-6 pb-5" />
            : <OrdenesPorEstado estados={datos.ordenesPorEstado} />}
        </BloqueSeccion>
      )}

      {/* Las dos listas de alertas, una al lado de la otra en escritorio para verlas sin desplazarse */}
      {!dashboard.error && (
        <div className="grid gap-5 xl:grid-cols-2">
          <BloqueSeccion
            titulo="Mantenimiento preventivo"
            detalle={datos
              ? `${formatNumero(datos.preventivosVencidos)} vencidos y ${formatNumero(datos.preventivosProximos)} próximos. Se muestran los 10 más urgentes.`
              : <LeyendaPreventivo />}
            acciones={(
              <Link to="/preventivo" className="inline-flex items-center gap-1.5 text-[0.9375rem] font-semibold text-primary underline-offset-4 hover:underline">
                Ver todos
                <ArrowRight className="size-4" aria-hidden="true" />
              </Link>
            )}
          >
            {dashboard.cargando || !datos
              ? <IndicadorCarga forma="bloque" texto="Cargando las alertas de preventivo…" />
              : <AlertasPreventivo alertas={datos.alertasPreventivo} />}
          </BloqueSeccion>
        </div>
      )}
    </>
  )
}

// Una celda por estado, también los que tienen 0 órdenes (los trae la API). Arriba los abiertos, que requieren
// atención; abajo los cerrados. Cada celda lleva al listado de órdenes filtrado por ese estado.
function OrdenesPorEstado({ estados }) {
  const enlace = (e) => `/ordenes?idEstadoOrden=${e.idEstadoOrden}`
  return (
    <FranjaIndicadores
      abiertas={estados.filter((e) => !e.esFinal).map((e) => (
        <TarjetaIndicador
          key={e.idEstadoOrden}
          estado={e.nombre}
          cantidad={e.cantidad}
          to={enlace(e)}
          detalle={e.solicitudMasAntigua ? `desde ${formatFecha(e.solicitudMasAntigua)}` : 'sin órdenes'}
        />
      ))}
      cerradas={estados.filter((e) => e.esFinal).map((e) => (
        <TarjetaIndicador key={e.idEstadoOrden} estado={e.nombre} cantidad={e.cantidad} to={enlace(e)} cerrado />
      ))}
    />
  )
}

// Vehículos Vencidos y Próximos, del mayor al menor porcentaje consumido (máximo 10, los recorta la API).
// El código lleva al detalle del vehículo; la barra y el badge muestran el porcentaje y el estado que calculó la API.
function AlertasPreventivo({ alertas }) {
  if (alertas.length === 0) {
    return <EstadoVacio tipo="positivo" titulo="No hay vehículos vencidos ni próximos a su mantenimiento" mensaje="Toda la flota está al día según su kilometraje registrado." />
  }
  return (
    <>
      <LeyendaPreventivo className="mb-4" />
      <ul className="flex flex-col divide-y divide-border-suave">
        {alertas.map((a) => (
          <li key={a.idActivo} className="grid items-center gap-x-4 gap-y-1.5 py-3 first:pt-0 last:pb-0 md:grid-cols-[minmax(0,13rem)_minmax(0,1fr)_auto]">
            <span className="flex min-w-0 flex-col tabular-nums">
              <span className="truncate">
                <Link to={`/vehiculos/${a.idActivo}`} className="font-bold text-primary underline-offset-4 hover:underline">{a.codigo}</Link>
                {' '}<Separador /> {a.nombre}
              </span>
              <span className="text-sm text-muted-foreground">{a.categoriaNombre}</span>
            </span>
            <BarraPreventivo porcentaje={a.porcentajeConsumido} porcentajeAviso={a.porcentajeAviso} estado={a.estado}
              recorrido={a.kmRecorridos} umbral={a.intervaloKm} unidad={a.variableControlUnidad} compacta />
            <BadgeEstado estado={a.estado} />
          </li>
        ))}
      </ul>
    </>
  )
}


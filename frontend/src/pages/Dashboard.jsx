import { RotateCcw } from 'lucide-react'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import TarjetaIndicador, { FranjaIndicadores } from '@/components/shared/TarjetaIndicador'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import { obtenerDashboard } from '@/services/dashboardService'
import { formatFecha, formatNumero } from '@/utils/format'

// Dashboard general: la pantalla de inicio del Administrador. Muestra de un
// lo que requiere atención: órdenes por estado, alertas de preventivo y repuestos bajo el mínimo.
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

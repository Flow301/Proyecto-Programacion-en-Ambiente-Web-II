import { useCallback, useState } from 'react'
import { Link } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Ban, Loader2 } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import BarraPreventivo, { LeyendaPreventivo } from '@/components/shared/BarraPreventivo'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import EstadoVacio from '@/components/shared/EstadoVacio'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import LineaTiempo from '@/components/shared/LineaTiempo'
import ProgresoOrden from '@/components/shared/ProgresoOrden'
import Separador from '@/components/shared/Separador'
import TablaPaginada from '@/components/shared/TablaPaginada'
import TarjetaIndicador, { FranjaIndicadores } from '@/components/shared/TarjetaIndicador'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import { estadosActivo, estadosOrden } from '@/services/catalogoService'
import { obtenerDashboard } from '@/services/dashboardService'
import { historialOrden, listarOrdenes, obtenerOrden } from '@/services/ordenService'
import { alertasPreventivo } from '@/services/preventivoService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { formatFecha } from '@/utils/format'

// Muestrario de los componentes compartidos con datos reales de la API, para revisarlos antes de que
// existan las pantallas de negocio (encargo de estilo, fase 4). Solo existe en desarrollo (App.jsx)
// y no envía ningún cambio a la API: las confirmaciones y el formulario solo se muestran.

// Órdenes sembradas que muestran cada caso de la ruta
const CASOS_RUTA = [6, 10, 11, 5, 8]

async function cargarRutas() {
  return Promise.all(CASOS_RUTA.map(async (id) => {
    const [{ data: orden }, { data: historial }] = await Promise.all([obtenerOrden(id), historialOrden(id)])
    const fechas = {}
    for (const h of [...historial].reverse()) fechas[h.estadoNuevoNombre] = h.fechaCambio
    const pendientes = orden.repuestos?.pendientes ?? []
    return {
      orden,
      historial,
      fechas,
      detenidaEn: historial[0]?.estadoAnteriorNombre,
      detalleEspera: pendientes.map((p) => `Falta ${p.repuestoCodigo} × ${p.cantidad} (stock ${p.repuestoStockActual})`).join(', '),
    }
  }))
}

const cargarEstados = () => Promise.all([estadosOrden(), estadosActivo()])

const esquemaLectura = z.object({
  lectura: z.string().trim().min(1, 'Ingrese la lectura en kilómetros.').regex(/^\d+$/, 'Use solo números enteros, sin puntos ni comas.'),
})

export default function Muestrario() {
  const filtros = useFiltrosUrl()
  const pagina = filtros.pagina
  const cargarOrdenes = useCallback(() => listarOrdenes(pagina, 5), [pagina])
  const ordenes = useCarga(cargarOrdenes)
  const rutas = useCarga(cargarRutas)
  const estados = useCarga(cargarEstados)
  const alertas = useCarga(alertasPreventivo)
  const dashboard = useCarga(obtenerDashboard)
  const [confirmando, setConfirmando] = useState(false)
  const [procesando, setProcesando] = useState(false)

  const { control, handleSubmit } = useForm({ resolver: zodResolver(esquemaLectura), defaultValues: { lectura: '' } })

  const orden6 = rutas.datos?.[0]

  return (
    <>
      <EncabezadoPagina
        titulo="Muestrario de componentes"
        detalle="Solo en desarrollo. Datos reales de la API; nada de esta página envía cambios."
        acciones={<Button variant="outline" onClick={() => avisarExito('Orden OM-2026-00006 aprobada')}>Aviso de éxito</Button>}
      />

      <BloqueSeccion titulo="Ruta de la orden" detalle="ProgresoOrden: en curso, en espera, cancelada, rechazada y completada">
        {rutas.cargando && <IndicadorCarga forma="bloque" />}
        {rutas.error && <EstadoError error={rutas.error} onReintentar={rutas.recargar} />}
        <div className="flex flex-col gap-10 pt-2">
          {rutas.datos?.map(({ orden, fechas, detenidaEn, detalleEspera }) => (
            <div key={orden.idOrden} className="flex flex-col gap-4">
              <p className="flex flex-wrap items-center gap-3 tabular-nums">
                <strong>{orden.codigo}</strong>
                <BadgeEstado estado={orden.estadoOrdenNombre} />
                <span className="text-sm text-muted-foreground">{orden.activoCodigo} <Separador /> {orden.activoNombre}</span>
              </p>
              <ProgresoOrden
                estado={orden.estadoOrdenNombre}
                fechas={fechas}
                diasEnEstado={orden.diasEnEstadoActual}
                detenidaEn={detenidaEn}
                detalleEspera={detalleEspera}
              />
            </div>
          ))}
        </div>
      </BloqueSeccion>

      <BloqueSeccion titulo="Badges de estado" detalle="BadgeEstado con los catálogos de la API">
        {estados.error && <EstadoError error={estados.error} onReintentar={estados.recargar} />}
        {estados.datos && (
          <div className="flex flex-col gap-3">
            <div className="flex flex-wrap gap-2">{estados.datos[0].data.map((e) => <BadgeEstado key={e.idEstadoOrden} estado={e.nombre} />)}</div>
            <div className="flex flex-wrap gap-2">{estados.datos[1].data.map((e) => <BadgeEstado key={e.idEstadoActivo} estado={e.nombre} />)}</div>
            <div className="flex flex-wrap gap-2">
              {['Vencido', 'Próximo', 'Al día', 'Sin lecturas', 'Consumido', 'Pendiente', 'Stock bajo', 'Recomendado', 'Por debajo', 'Dentro', 'Por encima'].map((e) => <BadgeEstado key={e} estado={e} />)}
            </div>
          </div>
        )}
      </BloqueSeccion>

      <BloqueSeccion titulo="Órdenes por estado" detalle="TarjetaIndicador del dashboard (solo Administrador)" sinRelleno>
        {dashboard.cargando && <IndicadorCarga forma="bloque" className="px-6 pb-5" />}
        {dashboard.error && <EstadoError error={dashboard.error} onReintentar={dashboard.recargar} className="mx-6 mb-5" />}
        {dashboard.datos && (
          <FranjaIndicadores
            abiertas={dashboard.datos.data.ordenesPorEstado.filter((e) => !e.esFinal).map((e) => (
              <TarjetaIndicador key={e.idEstadoOrden} estado={e.nombre} cantidad={e.cantidad} to="/ordenes"
                detalle={e.solicitudMasAntigua ? `desde ${formatFecha(e.solicitudMasAntigua)}` : 'sin órdenes'} />
            ))}
            cerradas={dashboard.datos.data.ordenesPorEstado.filter((e) => e.esFinal).map((e) => (
              <TarjetaIndicador key={e.idEstadoOrden} estado={e.nombre} cantidad={e.cantidad} to="/ordenes" cerrado />
            ))}
          />
        )}
      </BloqueSeccion>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,3fr)_minmax(0,2fr)]">
        <BloqueSeccion titulo="Mantenimiento preventivo" detalle={<LeyendaPreventivo />}>
          {alertas.cargando && <IndicadorCarga forma="bloque" />}
          {alertas.error && <EstadoError error={alertas.error} onReintentar={alertas.recargar} />}
          {alertas.datos?.data.length === 0 && <EstadoVacio tipo="positivo" titulo="Todos los activos están al día según su uso registrado" />}
          <ul className="flex flex-col gap-4">
            {alertas.datos?.data.map((a) => (
              <li key={a.idActivo} className="grid items-center gap-x-4 gap-y-1 md:grid-cols-[11rem_minmax(0,1fr)_auto]">
                <span className="tabular-nums"><strong>{a.codigo}</strong> <Separador /> {a.nombre}</span>
                <BarraPreventivo porcentaje={a.porcentajeConsumido} porcentajeAviso={a.porcentajeAviso} estado={a.estado}
                  recorrido={a.kmRecorridos} umbral={a.intervaloKm} unidad={a.variableControlUnidad} />
                <BadgeEstado estado={a.estado} />
              </li>
            ))}
          </ul>
        </BloqueSeccion>

        <BloqueSeccion titulo="Historial" detalle="LineaTiempo de la OM-2026-00006">
          {orden6 && (
            <LineaTiempo eventos={orden6.historial.map((h) => ({
              id: h.idHistorial, fecha: h.fechaCambio, usuario: h.usuarioNombreCompleto,
              anterior: h.estadoAnteriorNombre, nuevo: h.estadoNuevoNombre, observacion: h.observacion,
            }))} />
          )}
        </BloqueSeccion>
      </div>

      <section aria-label="Tabla paginada" className="flex flex-col gap-3">
        <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
          <p className="text-sm text-muted-foreground">BarraFiltros y TablaPaginada: 5 por página; la página queda en la URL.</p>
        </BarraFiltros>
        <TablaPaginada
          titulo="Órdenes de mantenimiento"
          nombre="órdenes"
          columnas={[
            { clave: 'codigo', titulo: 'Código', fija: true, render: (o) => <Link to="/ordenes" className="font-bold text-primary underline-offset-4 hover:underline">{o.codigo}</Link> },
            { clave: 'estado', titulo: 'Estado', render: (o) => <BadgeEstado estado={o.estadoOrdenNombre} /> },
            { clave: 'activo', titulo: 'Vehículo', recortar: true, completo: (o) => `${o.activoCodigo} · ${o.activoNombre}`, render: (o) => <><strong>{o.activoCodigo}</strong> <Separador /> {o.activoNombre}</> },
            { clave: 'tipoMantenimientoNombre', titulo: 'Tipo', recortar: true },
            { clave: 'prioridadNombre', titulo: 'Prioridad' },
            { clave: 'fechaSolicitud', titulo: 'Solicitud', render: (o) => formatFecha(o.fechaSolicitud) },
            { clave: 'diasEnEstadoActual', titulo: 'Días', derecha: true },
          ]}
          filas={ordenes.datos?.data ?? []}
          claveFila={(o) => o.idOrden}
          cargando={ordenes.cargando}
          error={ordenes.error}
          onReintentar={ordenes.recargar}
          pagina={pagina}
          tamano={5}
          totalRegistros={ordenes.datos?.totalRecords}
          totalPaginas={ordenes.datos?.totalPages}
          onCambiarPagina={filtros.irAPagina}
        />
      </section>

      <div className="grid gap-5 lg:grid-cols-[repeat(2,minmax(0,1fr))]">
        <BloqueSeccion titulo="Campo de formulario" detalle="CampoFormulario con validación en el cliente (no se envía)">
          <form noValidate className="flex max-w-sm flex-col gap-4" onSubmit={handleSubmit(() => avisarExito('La lectura es válida (no se envió).'))}>
            <Controller name="lectura" control={control} render={({ field, fieldState }) => (
              <CampoFormulario id="lectura" etiqueta="Nueva lectura" requerido unidad="km" ayuda="Debe ser igual o mayor que la última lectura." error={fieldState.error?.message}>
                {(props) => <Input {...field} {...props} inputMode="numeric" />}
              </CampoFormulario>
            )} />
            <Button type="submit" className="w-fit">Validar</Button>
          </form>
        </BloqueSeccion>

        <BloqueSeccion titulo="Confirmación y avisos" detalle="DialogoConfirmacion y sonner (no envían nada)">
          <div className="flex flex-wrap gap-2">
            <Button variant="destructive" onClick={() => setConfirmando(true)}><Ban aria-hidden="true" />Cancelar orden</Button>
            <Button variant="outline" onClick={() => avisarError({ status: 409, message: 'Stock insuficiente de FIL-010: disponible 2, requerido 5. La orden pasó a En Espera de Repuesto.' })}>Aviso de error</Button>
            <Button disabled><Loader2 className="animate-spin" aria-hidden="true" />Guardando…</Button>
          </div>
        </BloqueSeccion>
      </div>

      <BloqueSeccion titulo="Estados de pantalla" detalle="EstadoVacio, EstadoError e IndicadorCarga">
        <div className="grid gap-4 lg:grid-cols-2 xl:grid-cols-4">
          <EstadoVacio tipo="sinResultados" titulo="No hay órdenes En Espera de Repuesto" mensaje="Cambie el filtro de estado para ver otras órdenes.">
            <Button variant="outline" size="sm">Limpiar filtros</Button>
          </EstadoVacio>
          <EstadoVacio tipo="positivo" titulo="No hay repuestos bajo el mínimo" mensaje="Todo el inventario está sobre su stock mínimo." />
          <EstadoError error={{ status: 0 }} onReintentar={() => {}} />
          <IndicadorCarga forma="tabla" filas={3} columnas={3} className="rounded-sm border" />
        </div>
      </BloqueSeccion>

      <DialogoConfirmacion
        abierto={confirmando}
        titulo={`¿Cancelar la orden ${orden6?.orden.codigo ?? ''} del vehículo ${orden6?.orden.activoCodigo ?? ''}?`}
        mensaje="La orden pasa a Cancelada y no admite más cambios. Muestra: no se envía nada a la API."
        textoConfirmar="Cancelar orden"
        textoCancelar="Volver"
        conObservacion
        peligro
        procesando={procesando}
        onCancelar={() => setConfirmando(false)}
        onConfirmar={() => {
          setProcesando(true)
          setTimeout(() => { setProcesando(false); setConfirmando(false) }, 800)
        }}
      />
    </>
  )
}

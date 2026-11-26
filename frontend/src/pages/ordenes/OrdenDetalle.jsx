import { useCallback, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router'
import { Ban, BadgeCheck, CirclePlay, CircleX, Clock, Flag, Pencil, Plus, ReceiptText, RotateCcw, Search } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import LineaTiempo from '@/components/shared/LineaTiempo'
import PaginaError from '@/components/shared/PaginaError'
import ProgresoOrden from '@/components/shared/ProgresoOrden'
import Separador from '@/components/shared/Separador'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import useRol from '@/hooks/useRol'
import { cancelarOrden, completarOrden, historialOrden, obtenerOrden, reanudarOrden, rechazarOrden } from '@/services/ordenService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { formatColones, formatFecha, formatFechaHora, formatHoras, formatKm, formatNumero } from '@/utils/format'
import { DialogoAprobacion, DialogoDiagnostico, DialogoEdicion, DialogoHoras, DialogoRepuesto, DialogoTecnicos } from './DialogosOrden'

// Acciones de cambio de estado que van en el encabezado, en el orden en que se muestran.
// Las que solo confirman llevan su texto; las demás abren una ventana con datos.
const ACCIONES = {
  editar: { texto: 'Editar', Icono: Pencil, variante: 'outline' },
  cancelar: { texto: 'Cancelar orden', Icono: Ban, variante: 'destructive', confirmar: { fn: cancelarOrden, exito: 'cancelada', peligro: true, pregunta: '¿Cancelar la orden', mensaje: 'La orden se cierra sin completarse y no se puede retomar.' } },
  rechazar: { texto: 'Rechazar', Icono: CircleX, variante: 'destructive', confirmar: { fn: rechazarOrden, exito: 'rechazada', peligro: true, pregunta: '¿Rechazar la orden', mensaje: 'La orden se cierra sin ejecutarse y no se puede retomar.' } },
  diagnosticar: { texto: 'Diagnosticar', Icono: Search },
  aprobar: { texto: 'Aprobar', Icono: BadgeCheck },
  iniciar: { texto: 'Iniciar', Icono: CirclePlay },
  reanudar: { texto: 'Reanudar', Icono: RotateCcw, confirmar: { fn: reanudarOrden, exito: 'reanudada', pregunta: '¿Reanudar la orden', mensaje: 'La orden vuelve a En Ejecución.' } },
  completar: { texto: 'Completar', Icono: Flag, confirmar: { fn: completarOrden, exito: 'completada', pregunta: '¿Completar la orden', mensaje: 'Se cierran los costos con los repuestos y las horas registrados, y ya no se pueden modificar.' } },
}

async function cargarOrden(id) {
  const [{ data: orden }, { data: historial }] = await Promise.all([obtenerOrden(id), historialOrden(id, 1, 50)])
  // La API da el historial del más reciente al más antiguo; la ruta necesita la fecha en que llegó a cada estado
  const fechas = {}
  for (const h of [...historial].reverse()) fechas[h.estadoNuevoNombre] = h.fechaCambio
  return { orden, historial, fechas }
}

// Detalle de la orden (enunciado Avance 2, 5.4): ruta de estados, datos, técnicos y horas, repuestos, costos e historial.
// Las acciones visibles son exactamente las que la API devuelve en accionesDisponibles para el rol y el estado.
export default function OrdenDetalle() {
  const { id } = useParams()
  const { state } = useLocation()
  const { esSolicitante } = useRol()
  // Si se llegó desde el detalle de un vehículo, el enlace de regreso vuelve a él y lo dice
  const textoVolver = state?.desde?.startsWith('/vehiculos/') ? 'Volver al vehículo'
    : esSolicitante ? 'Volver a Mis solicitudes' : 'Volver a Órdenes'
  const cargar = useCallback(() => cargarOrden(id), [id])
  const carga = useCarga(cargar)
  const [dialogo, setDialogo] = useState(null) // { accion, tecnico? }
  const [procesando, setProcesando] = useState(false)

  const volver = { to: state?.desde ?? '/ordenes', texto: textoVolver }
  if (carga.error?.status === 404 || carga.error?.status === 403) return <PaginaError codigo={carga.error.status} />
  if (carga.cargando && !carga.datos) return <><EncabezadoPagina titulo="Orden" volver={volver} /><IndicadorCarga forma="detalle" /></>
  if (carga.error) return <><EncabezadoPagina titulo="Orden" volver={volver} /><EstadoError error={carga.error} onReintentar={carga.recargar} /></>

  const { orden: o, historial, fechas } = carga.datos
  const puede = new Set(o.accionesDisponibles)
  const cerrar = () => setDialogo(null)
  const hecho = () => { setDialogo(null); carga.recargar() }
  const confirmacion = dialogo && ACCIONES[dialogo.accion]?.confirmar
  const pendientes = o.repuestos?.pendientes ?? []
  const detalleRepuestos = o.repuestos?.detalle ?? []

  const confirmar = async (observacion) => {
    setProcesando(true)
    try {
      await confirmacion.fn(o.idOrden, observacion || null)
      avisarExito(`Orden ${o.codigo} ${confirmacion.exito}`)
      hecho()
    } catch (err) {
      avisarError(err)
    } finally {
      setProcesando(false)
    }
  }

  return (
    <>
      <EncabezadoPagina
        codigo={o.codigo}
        pestana={`Orden ${o.codigo}`}
        volver={volver}
        estado={<BadgeEstado estado={o.estadoOrdenNombre} />}
        detalle={<><strong>{o.activoCodigo}</strong> <Separador /> {o.activoNombre} <Separador /> {o.tipoMantenimientoNombre}</>}
        acciones={Object.entries(ACCIONES).filter(([accion]) => puede.has(accion)).map(([accion, { texto, Icono, variante }]) => (
          <Button key={accion} variant={variante} onClick={() => setDialogo({ accion })}><Icono aria-hidden="true" />{texto}</Button>
        ))}
      />

      <BloqueSeccion titulo="Ruta de la orden">
        <ProgresoOrden
          estado={o.estadoOrdenNombre}
          fechas={fechas}
          diasEnEstado={o.diasEnEstadoActual}
          detenidaEn={historial[0]?.estadoAnteriorNombre}
          detalleEspera={pendientes.map((p) => `Falta ${p.repuestoCodigo} × ${p.cantidad} (stock ${p.repuestoStockActual})`).join(', ')}
        />
      </BloqueSeccion>

      <div className="grid items-start gap-5 2xl:grid-cols-2">
        <BloqueSeccion titulo="Solicitud">
          <Datos filas={[
            ['Vehículo', <><strong>{o.activoCodigo}</strong> <Separador /> {o.activoNombre}</>],
            ['Placa', o.activoPlaca],
            ['Categoría', o.activoCategoriaNombre],
            ['Estado del vehículo', <BadgeEstado key="estado" estado={o.activoEstadoActivoNombre} />],
            ['Tipo', `${o.tipoMantenimientoNombre} (${o.tipoMantenimientoNaturalezaNombre})`],
            ['Especialidad requerida', o.tipoMantenimientoEspecialidadRequeridaNombre],
            ['Prioridad', o.prioridadNombre ?? 'Sin asignar'],
            ['Solicitada por', o.usuarioSolicitanteNombreCompleto],
            ['Fecha de solicitud', formatFechaHora(o.fechaSolicitud)],
            ['Fecha programada', o.fechaProgramada ? formatFecha(o.fechaProgramada) : 'Sin programar'],
            ...(o.fechaCierre ? [['Fecha de cierre', formatFechaHora(o.fechaCierre)]] : []),
          ]} />
          <h3 className="mt-5 text-sm font-semibold text-muted-foreground">Motivo</h3>
          <p className="mt-1 whitespace-pre-line">{o.motivo}</p>
        </BloqueSeccion>

        <div className="flex flex-col gap-5">
          <BloqueSeccion titulo="Diagnóstico">
            {o.diagnostico
              ? <><p className="whitespace-pre-line">{o.diagnostico}</p><p className="mt-3 text-sm text-muted-foreground">Kilometraje al diagnosticar: {formatKm(o.kilometrajeDiagnostico)}</p></>
              : <p className="text-muted-foreground">Todavía no se ha diagnosticado.</p>}
          </BloqueSeccion>
          <BloqueSeccion
            titulo="Costos"
            detalle={o.estadoOrdenNombre === 'Completada' ? 'Costos cerrados al completar la orden.' : 'Acumulados hasta hoy; se cierran al completar la orden.'}
            acciones={!esSolicitante && (
              <Button asChild variant="outline" size="sm">
                <Link to={`/ordenes/${o.idOrden}/costos`}><ReceiptText aria-hidden="true" />Reporte de costos</Link>
              </Button>
            )}
          >
            <Datos filas={[
              ['Repuestos', formatColones(o.costoRepuestos ?? detalleRepuestos.reduce((t, d) => t + d.subtotal, 0))],
              ['Mano de obra', o.costoManoObra == null ? `${formatHoras(o.totalHoras)} registradas` : formatColones(o.costoManoObra)],
              ...(o.costoTotal == null ? [] : [['Total', <strong key="total" className="text-lg">{formatColones(o.costoTotal)}</strong>]]),
            ]} />
          </BloqueSeccion>
        </div>
      </div>

      <BloqueSeccion
        titulo="Técnicos y horas"
        detalle={`${o.cantidadTecnicos} ${o.cantidadTecnicos === 1 ? 'técnico' : 'técnicos'} · ${formatHoras(o.totalHoras)} en total`}
        sinRelleno
        acciones={puede.has('asignarTecnicos') && <Button variant="outline" onClick={() => setDialogo({ accion: 'asignarTecnicos' })}><Plus aria-hidden="true" />Asignar técnicos</Button>}
      >
        <TablaPaginada
          titulo={`Técnicos de la orden ${o.codigo}`}
          nombre="técnicos"
          className="rounded-none border-0 border-t"
          filas={o.tecnicos ?? []}
          claveFila={(t) => t.idTecnico}
          vacio={{ titulo: 'Sin técnicos asignados', mensaje: 'Se asignan al iniciar la orden.' }}
          columnas={[
            { clave: 'tecnicoNombreCompleto', titulo: 'Técnico', render: (t) => <strong>{t.tecnicoNombreCompleto}</strong> },
            { clave: 'fechaAsignacion', titulo: 'Asignado', render: (t) => formatFecha(t.fechaAsignacion) },
            { clave: 'tarifaHora', titulo: 'Tarifa por hora', derecha: true, render: (t) => formatColones(t.tarifaHora) },
            { clave: 'horasTrabajadas', titulo: 'Horas', derecha: true, render: (t) => formatHoras(t.horasTrabajadas) },
            ...(puede.has('registrarHoras') ? [{
              clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
              render: (t) => <Button variant="ghost" size="sm" onClick={() => setDialogo({ accion: 'registrarHoras', tecnico: t })}><Clock aria-hidden="true" />Registrar horas</Button>,
            }] : []),
          ]}
        />
      </BloqueSeccion>

      <BloqueSeccion
        titulo="Repuestos"
        sinRelleno
        acciones={puede.has('registrarRepuestos') && <Button variant="outline" onClick={() => setDialogo({ accion: 'registrarRepuestos' })}><Plus aria-hidden="true" />Registrar repuesto</Button>}
      >
        <TablaPaginada
          titulo={`Repuestos consumidos y pendientes de la orden ${o.codigo}`}
          nombre="repuestos"
          className="rounded-none border-0 border-t"
          filas={[...pendientes.map((p) => ({ ...p, pendiente: true })), ...detalleRepuestos]}
          claveFila={(r) => (r.pendiente ? `p-${r.idRepuesto}` : `d-${r.numeroLinea}`)}
          vacio={{ titulo: 'Sin repuestos registrados' }}
          columnas={[
            { clave: 'repuestoCodigo', titulo: 'Código', render: (r) => <strong>{r.repuestoCodigo}</strong> },
            { clave: 'repuestoNombre', titulo: 'Repuesto', recortar: true },
            { clave: 'estado', titulo: 'Estado', render: (r) => <BadgeEstado estado={r.pendiente ? 'Pendiente' : 'Consumido'} /> },
            { clave: 'cantidad', titulo: 'Cantidad', derecha: true, render: (r) => formatNumero(r.cantidad) },
            { clave: 'costoUnitario', titulo: 'Costo unitario', derecha: true, render: (r) => (r.pendiente ? <span className="text-sm text-muted-foreground">Stock {formatNumero(r.repuestoStockActual)}</span> : formatColones(r.costoUnitario)) },
            { clave: 'subtotal', titulo: 'Subtotal', derecha: true, render: (r) => (r.pendiente ? '' : formatColones(r.subtotal)) },
          ]}
        />
      </BloqueSeccion>

      <BloqueSeccion titulo="Historial">
        <LineaTiempo eventos={historial.map((h) => ({
          id: h.idHistorial, fecha: h.fechaCambio, usuario: h.usuarioNombreCompleto,
          anterior: h.estadoAnteriorNombre, nuevo: h.estadoNuevoNombre, observacion: h.observacion,
        }))} />
      </BloqueSeccion>

      {dialogo?.accion === 'diagnosticar' && <DialogoDiagnostico orden={o} onCerrar={cerrar} onHecho={hecho} />}
      {dialogo?.accion === 'aprobar' && <DialogoAprobacion orden={o} onCerrar={cerrar} onHecho={hecho} />}
      {dialogo?.accion === 'iniciar' && <DialogoTecnicos orden={o} iniciar onCerrar={cerrar} onHecho={hecho} />}
      {dialogo?.accion === 'asignarTecnicos' && <DialogoTecnicos orden={o} onCerrar={cerrar} onHecho={hecho} />}
      {dialogo?.accion === 'registrarHoras' && <DialogoHoras orden={o} tecnico={dialogo.tecnico} onCerrar={cerrar} onHecho={hecho} />}
      {dialogo?.accion === 'registrarRepuestos' && <DialogoRepuesto orden={o} onCerrar={cerrar} onHecho={hecho} />}
      {dialogo?.accion === 'editar' && <DialogoEdicion orden={o} onCerrar={cerrar} onHecho={hecho} />}
      <DialogoConfirmacion
        abierto={Boolean(confirmacion)}
        titulo={confirmacion ? `${confirmacion.pregunta} ${o.codigo} del vehículo ${o.activoCodigo}?` : ''}
        mensaje={confirmacion?.mensaje}
        textoConfirmar={confirmacion ? ACCIONES[dialogo.accion].texto : ''}
        textoCancelar="Volver"
        conObservacion
        peligro={confirmacion?.peligro}
        procesando={procesando}
        onCancelar={cerrar}
        onConfirmar={confirmar}
      />
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

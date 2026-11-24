import { useCallback, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router'
import { Ban, ChevronLeft, ChevronRight, CircleCheckBig, OctagonX } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraPreventivo, { LeyendaPreventivo } from '@/components/shared/BarraPreventivo'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import EstadoVacio from '@/components/shared/EstadoVacio'
import ImagenVehiculo from '@/components/shared/ImagenVehiculo'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import LineaTiempo from '@/components/shared/LineaTiempo'
import PaginaError from '@/components/shared/PaginaError'
import Separador from '@/components/shared/Separador'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import useRol from '@/hooks/useRol'
import { cambiarEstadoActivo, darDeBajaActivo, historialActivo, lecturasActivo, obtenerActivo } from '@/services/activoService'
import { estadosActivo } from '@/services/catalogoService'
import { listarOrdenes } from '@/services/ordenService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { formatAntiguedad, formatColones, formatFecha, formatFechaHora, formatKm, formatNumero } from '@/utils/format'

// Detalle del vehículo (enunciado Avance 2, 5.2): encabezado con la foto, datos generales y del contexto,
// el bloque de uso y mantenimiento preventivo (5.8), sus órdenes, su historial de estados (5.7) y sus lecturas de uso.
// La evaluación preventiva la calcula la API.
// El Administrador cambia el estado a mano (Fuera de Servicio, de vuelta a Operativo, Dar de baja), con confirmación y
// observación opcional. Qué cambio se permite lo decide la API (409 con su mensaje); aquí solo se ofrecen los que
// tienen sentido para el estado actual. En Mantenimiento lo manejan las órdenes.
const sinEstados = () => Promise.resolve({ data: [] })

const CAMBIOS = {
  fueraServicio: {
    texto: 'Fuera de Servicio', Icono: OctagonX, variante: 'outline', destino: 'Fuera de Servicio', peligro: true,
    pregunta: (v) => `¿Poner el vehículo ${v.codigo} Fuera de Servicio?`,
    mensaje: 'Deja de evaluarse en el mantenimiento preventivo hasta que vuelva a Operativo. El cambio queda en el historial.',
  },
  operativo: {
    texto: 'Volver a Operativo', Icono: CircleCheckBig, variante: 'outline', destino: 'Operativo',
    pregunta: (v) => `¿Devolver el vehículo ${v.codigo} a Operativo?`,
    mensaje: 'Vuelve a evaluarse en el mantenimiento preventivo. El cambio queda en el historial.',
  },
  baja: {
    texto: 'Dar de baja', Icono: Ban, variante: 'destructive', destino: 'Dado de Baja', peligro: true,
    pregunta: (v) => `¿Dar de baja el vehículo ${v.codigo}?`,
    mensaje: 'Es definitivo: no recibe más órdenes ni lecturas. Sus datos, órdenes e historial se conservan. No se permite si tiene órdenes abiertas.',
  },
}
const CAMBIOS_POR_ESTADO = {
  'Operativo': ['fueraServicio', 'baja'],
  'Fuera de Servicio': ['operativo', 'baja'],
}

export default function VehiculoDetalle() {
  const { id } = useParams()
  const { state } = useLocation()
  const { esAdmin } = useRol()
  const cargar = useCallback(() => obtenerActivo(id), [id])
  const carga = useCarga(cargar)
  const estados = useCarga(esAdmin ? estadosActivo : sinEstados)
  const [cambio, setCambio] = useState(null) // clave de CAMBIOS
  const [procesando, setProcesando] = useState(false)
  // Después de un cambio se vuelven a pedir el detalle y sus secciones (historial, lecturas, órdenes)
  const [version, setVersion] = useState(0)

  const volver = { to: state?.desde ?? '/vehiculos', texto: 'Volver a Vehículos' }
  if (carga.error?.status === 404 || carga.error?.status === 403) return <PaginaError codigo={carga.error.status} />
  if (carga.cargando && !carga.datos) return <><EncabezadoPagina titulo="Vehículo" volver={volver} /><IndicadorCarga forma="detalle" /></>
  if (carga.error) return <><EncabezadoPagina titulo="Vehículo" volver={volver} /><EstadoError error={carga.error} onReintentar={carga.recargar} /></>

  const v = carga.datos.data
  const cambios = esAdmin ? (CAMBIOS_POR_ESTADO[v.estadoActivoNombre] ?? []) : []
  const elegido = cambio && CAMBIOS[cambio]

  const confirmar = async (observacion) => {
    setProcesando(true)
    try {
      if (cambio === 'baja') {
        await darDeBajaActivo(v.idActivo, observacion)
      } else {
        const destino = (estados.datos?.data ?? []).find((e) => e.nombre === elegido.destino)
        await cambiarEstadoActivo(v.idActivo, destino?.idEstadoActivo, observacion)
      }
      avisarExito(`${v.codigo} pasó a ${elegido.destino}`)
      setCambio(null)
      carga.recargar()
      setVersion((n) => n + 1)
    } catch (err) {
      avisarError(err)
    } finally {
      setProcesando(false)
    }
  }

  return (
    <>
      <EncabezadoPagina
        codigo={v.codigo}
        pestana={`Vehículo ${v.codigo}`}
        volver={volver}
        imagen={<ImagenVehiculo ruta={v.imagen} alt={`${v.codigo} · ${v.nombre}`} className="h-28 w-40" />}
        estado={<BadgeEstado estado={v.estadoActivoNombre} />}
        detalle={<>{v.nombre} <Separador /> {v.categoriaNombre}</>}
        acciones={cambios.length > 0 && cambios.map((c) => {
          const { texto, Icono, variante } = CAMBIOS[c]
          return <Button key={c} variant={variante} onClick={() => setCambio(c)}><Icono aria-hidden="true" />{texto}</Button>
        })}
      />

      <div className="grid items-start gap-5 xl:grid-cols-2">
        <BloqueSeccion titulo="Datos generales">
          {/* Los datos propios del contexto (placa, marca, modelo, año y kilometraje) van con los generales */}
          <Datos filas={[
            ['Descripción', v.descripcion || <span key="sin" className="text-muted-foreground">Sin descripción</span>],
            ['Placa', <strong key="placa">{v.placa}</strong>],
            ['Marca y modelo', `${v.marca} ${v.modelo}`],
            ['Año', v.anio],
            ['Kilometraje actual', formatKm(v.kilometrajeActual)],
            ['Fecha de adquisición', formatFecha(v.fechaAdquisicion)],
            ['Antigüedad', formatAntiguedad(v.fechaAdquisicion)],
            ['Valor de referencia', formatColones(v.valorReferencia)],
            ['Registrado en el sistema', formatFechaHora(v.fechaRegistro)],
          ]} />
        </BloqueSeccion>

        <UsoPreventivo v={v} />
      </div>

      <OrdenesVehiculo key={`o${version}`} v={v} />

      <div className="grid items-start gap-5 xl:grid-cols-2">
        <HistorialVehiculo key={`h${version}`} v={v} />
        <LecturasVehiculo key={`l${version}`} v={v} />
      </div>

      <DialogoConfirmacion
        abierto={Boolean(elegido)}
        titulo={elegido ? elegido.pregunta(v) : ''}
        mensaje={elegido?.mensaje}
        textoConfirmar={elegido?.texto}
        textoCancelar="Volver"
        conObservacion
        peligro={elegido?.peligro}
        procesando={procesando}
        onCancelar={() => setCambio(null)}
        onConfirmar={confirmar}
      />
    </>
  )
}

// Bloque "Uso y mantenimiento preventivo": la misma evaluación de la pantalla Preventivo, para este vehículo.
// Un vehículo Fuera de Servicio o Dado de Baja no se evalúa (la API manda preventivo = null).
function UsoPreventivo({ v }) {
  const p = v.preventivo
  if (!p) {
    return (
      <BloqueSeccion titulo="Uso y mantenimiento preventivo">
        <p className="text-muted-foreground">
          No se evalúa mientras el vehículo esté {v.estadoActivoNombre}. Su último kilometraje registrado es {formatKm(v.kilometrajeActual)}.
        </p>
      </BloqueSeccion>
    )
  }
  const unidad = p.variableControlUnidad
  const sinLecturas = p.estado === 'Sin lecturas'
  return (
    <BloqueSeccion
      titulo="Uso y mantenimiento preventivo"
      detalle={`Se evalúa por ${p.variableControlNombre?.toLowerCase() ?? 'kilometraje'} en cada consulta.`}
      acciones={(
        <span className="flex flex-wrap items-center gap-2">
          <BadgeEstado estado={p.estado} />
          {p.situacion === 'Orden en curso' && p.idOrdenEnCurso
            ? <Link to={`/ordenes/${p.idOrdenEnCurso}`} className="text-sm font-bold text-primary underline-offset-4 hover:underline">{p.codigoOrdenEnCurso} en curso</Link>
            : p.situacion && <BadgeEstado estado={p.situacion} />}
        </span>
      )}
    >
      {!sinLecturas && (
        <>
          <BarraPreventivo porcentaje={p.porcentajeConsumido} porcentajeAviso={p.porcentajeAviso} estado={p.estado}
            recorrido={p.kmRecorridos} umbral={p.intervaloKm} unidad={unidad} />
          <LeyendaPreventivo className="mt-2 mb-4" />
        </>
      )}
      <Datos filas={[
        ['Última lectura', sinLecturas
          ? <span key="sin" className="text-muted-foreground">Sin lecturas registradas</span>
          : <>{formatKm(p.kilometrajeActual, unidad)} <Separador /> {formatFecha(p.fechaUltimaLectura)}</>],
        ['Lectura del último preventivo', formatKm(p.kilometrajeUltimoPreventivo, unidad)],
        ['Uso acumulado', formatKm(p.kmRecorridos, unidad)],
        ['Umbral', <>{formatKm(p.intervaloKm, unidad)} <span className="text-sm text-muted-foreground">({p.intervaloPropio ? 'propio del vehículo' : 'de la categoría'})</span></>],
        ['Aviso desde', `${formatNumero(p.porcentajeAviso)} % del umbral`],
        [p.kmRestantes < 0 ? 'Se pasó del umbral por' : 'Faltan para el umbral', formatKm(Math.abs(p.kmRestantes), unidad)],
      ]} />
    </BloqueSeccion>
  )
}

const TAMANO = 5

// Órdenes del vehículo, de la más reciente a la más antigua, con su enlace. Es el listado de órdenes filtrado por
// vehículo: al Solicitante/Consulta la API le devuelve solo las suyas.
function OrdenesVehiculo({ v }) {
  const { pathname } = useLocation()
  const [pagina, setPagina] = useState(1)
  const cargar = useCallback(() => listarOrdenes(pagina, TAMANO, { idActivo: v.idActivo }), [pagina, v.idActivo])
  const lista = useCarga(cargar)
  return (
    <BloqueSeccion titulo="Órdenes" detalle={`Órdenes de mantenimiento de ${v.codigo}.`} sinRelleno>
      <TablaPaginada
        titulo={`Órdenes del vehículo ${v.codigo}`}
        nombre="órdenes"
        className="rounded-none border-0 border-t"
        filas={lista.datos?.data ?? []}
        claveFila={(o) => o.idOrden}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={{ titulo: 'Este vehículo no tiene órdenes', mensaje: 'Las solicitudes de mantenimiento aparecen aquí.' }}
        columnas={[
          { clave: 'codigo', titulo: 'Código', render: (o) => <Link to={`/ordenes/${o.idOrden}`} state={{ desde: pathname }} className="font-bold text-primary underline-offset-4 hover:underline">{o.codigo}</Link> },
          { clave: 'fechaSolicitud', titulo: 'Solicitud', render: (o) => formatFecha(o.fechaSolicitud) },
          { clave: 'tipoMantenimientoNombre', titulo: 'Tipo', recortar: true },
          { clave: 'prioridadNombre', titulo: 'Prioridad', render: (o) => o.prioridadNombre ?? 'Sin asignar' },
          { clave: 'estado', titulo: 'Estado', render: (o) => <BadgeEstado estado={o.estadoOrdenNombre} /> },
        ]}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={setPagina}
      />
    </BloqueSeccion>
  )
}

// Historial de estados (enunciado Avance 2, 5.7): solo lectura, del más reciente al más antiguo. Un cambio automático
// muestra la orden que lo provocó. Al Solicitante/Consulta la API solo le muestra el historial de vehículos con
// órdenes suyas: para los demás responde 404, y aquí se explica en lugar de mostrar un error.
function HistorialVehiculo({ v }) {
  const [pagina, setPagina] = useState(1)
  const cargar = useCallback(() => historialActivo(v.idActivo, pagina, TAMANO), [pagina, v.idActivo])
  const historial = useCarga(cargar)
  const datos = historial.datos

  let contenido
  if (historial.error?.status === 404) {
    contenido = <EstadoVacio titulo="Historial no disponible" mensaje="Solo puede consultar el historial de los vehículos de sus solicitudes." />
  } else if (historial.error) {
    contenido = <EstadoError error={historial.error} onReintentar={historial.recargar} />
  } else if (historial.cargando && !datos) {
    contenido = <IndicadorCarga forma="bloque" texto="Cargando el historial…" />
  } else if (!datos?.data.length) {
    contenido = <EstadoVacio titulo="Sin cambios de estado registrados" />
  } else {
    contenido = (
      <>
        <LineaTiempo eventos={datos.data.map((h) => ({
          id: h.idHistorial, fecha: h.fechaCambio, usuario: h.usuarioNombreCompleto,
          anterior: h.estadoAnteriorNombre, nuevo: h.estadoNuevoNombre, observacion: h.observacion,
          orden: h.idOrden ? { codigo: h.ordenCodigo, to: `/ordenes/${h.idOrden}` } : null,
        }))} />
        <Paginacion pagina={pagina} totalPaginas={datos.totalPages} totalRegistros={datos.totalRecords} nombre="cambios" cargando={historial.cargando} onCambiar={setPagina} />
      </>
    )
  }
  return <BloqueSeccion titulo="Historial de estados" detalle="Cada cambio de estado, quién lo hizo y por qué.">{contenido}</BloqueSeccion>
}

// Lecturas de uso: las registra el Administrador con su acción o las toma el diagnóstico de una orden
function LecturasVehiculo({ v }) {
  const [pagina, setPagina] = useState(1)
  const cargar = useCallback(() => lecturasActivo(v.idActivo, pagina, TAMANO), [pagina, v.idActivo])
  const lista = useCarga(cargar)
  const unidad = v.preventivo?.variableControlUnidad ?? 'km'
  return (
    <BloqueSeccion titulo="Lecturas de uso" detalle="Alimentan el mantenimiento preventivo. Cada lectura es igual o mayor que la anterior." sinRelleno>
      <TablaPaginada
        titulo={`Lecturas de uso del vehículo ${v.codigo}`}
        nombre="lecturas"
        className="rounded-none border-0 border-t"
        filas={lista.datos?.data ?? []}
        claveFila={(l) => l.idLectura}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={{ titulo: 'Sin lecturas registradas', mensaje: 'Mientras no tenga lecturas, el preventivo queda "Sin lecturas".' }}
        columnas={[
          { clave: 'fecha', titulo: 'Fecha', render: (l) => formatFecha(l.fecha) },
          { clave: 'kilometraje', titulo: 'Lectura', derecha: true, render: (l) => <strong>{formatKm(l.kilometraje, unidad)}</strong> },
          { clave: 'usuarioNombreCompleto', titulo: 'Registró', render: (l) => <span className="whitespace-normal">{l.usuarioNombreCompleto}</span> },
          {
            clave: 'origen', titulo: 'Origen',
            render: (l) => (l.idOrden
              ? <Link to={`/ordenes/${l.idOrden}`} className="font-semibold text-primary underline-offset-4 hover:underline">{l.ordenCodigo}</Link>
              : <span className="text-sm text-muted-foreground">Lectura registrada</span>),
          },
        ]}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={setPagina}
      />
    </BloqueSeccion>
  )
}

// La misma paginación de TablaPaginada, para el historial (que es una línea de tiempo y no una tabla)
function Paginacion({ pagina, totalPaginas, totalRegistros, nombre, cargando, onCambiar }) {
  if (totalPaginas <= 1) return null
  return (
    <nav aria-label={`Paginación de ${nombre}`} className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-border-suave pt-3 text-sm tabular-nums">
      <span className="text-muted-foreground">{formatNumero(totalRegistros)} {nombre}</span>
      <div className="flex items-center gap-2">
        <Button variant="outline" disabled={cargando || pagina <= 1} onClick={() => onCambiar(pagina - 1)}>
          <ChevronLeft aria-hidden="true" />
          Anterior
        </Button>
        <span aria-live="polite">Página {pagina} de {totalPaginas}</span>
        <Button variant="outline" disabled={cargando || pagina >= totalPaginas} onClick={() => onCambiar(pagina + 1)}>
          Siguiente
          <ChevronRight aria-hidden="true" />
        </Button>
      </div>
    </nav>
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

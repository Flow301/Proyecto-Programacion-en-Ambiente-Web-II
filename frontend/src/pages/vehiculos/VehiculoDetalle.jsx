import { useCallback } from 'react'
import { Link, useLocation, useParams } from 'react-router'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraPreventivo, { LeyendaPreventivo } from '@/components/shared/BarraPreventivo'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import ImagenVehiculo from '@/components/shared/ImagenVehiculo'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import PaginaError from '@/components/shared/PaginaError'
import Separador from '@/components/shared/Separador'
import useCarga from '@/hooks/useCarga'
import { obtenerActivo } from '@/services/activoService'
import { formatAntiguedad, formatColones, formatFecha, formatFechaHora, formatKm, formatNumero } from '@/utils/format'

// Detalle del vehículo (enunciado Avance 2, 5.2): encabezado con la foto, datos generales y del contexto,
// y el bloque de uso y mantenimiento preventivo (5.8). La evaluación preventiva la calcula la API.
export default function VehiculoDetalle() {
  const { id } = useParams()
  const { state } = useLocation()
  const cargar = useCallback(() => obtenerActivo(id), [id])
  const carga = useCarga(cargar)

  const volver = { to: state?.desde ?? '/vehiculos', texto: 'Volver a Vehículos' }
  if (carga.error?.status === 404 || carga.error?.status === 403) return <PaginaError codigo={carga.error.status} />
  if (carga.cargando && !carga.datos) return <><EncabezadoPagina titulo="Vehículo" volver={volver} /><IndicadorCarga forma="detalle" /></>
  if (carga.error) return <><EncabezadoPagina titulo="Vehículo" volver={volver} /><EstadoError error={carga.error} onReintentar={carga.recargar} /></>

  const v = carga.datos.data

  return (
    <>
      <EncabezadoPagina
        codigo={v.codigo}
        pestana={`Vehículo ${v.codigo}`}
        volver={volver}
        imagen={<ImagenVehiculo ruta={v.imagen} alt={`${v.codigo} · ${v.nombre}`} className="h-28 w-40" />}
        estado={<BadgeEstado estado={v.estadoActivoNombre} />}
        detalle={<>{v.nombre} <Separador /> {v.categoriaNombre}</>}
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

import { useCallback, useEffect } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router'
import { Plus } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import ListaOpciones from '@/components/shared/ListaOpciones'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { listarTodosLosActivos } from '@/services/activoService'
import { estadosOrden } from '@/services/catalogoService'
import { listarOrdenes } from '@/services/ordenService'
import { cn } from '@/lib/utils'
import { formatFecha } from '@/utils/format'

const TAMANO = 10
// Bandeja del taller (enunciado Avance 2, 5.3): los estados que requieren gestión del Coordinador/Técnico
const BANDEJA_TALLER = ['Solicitada', 'Aprobada', 'En Ejecución', 'En Espera de Repuesto']

// Listado de órdenes (enunciado Avance 2, 5.3). El Solicitante/Consulta lo ve como "Mis solicitudes" y la API
// le devuelve solo las suyas. Filtros por estado (uno o varios) y por vehículo, guardados en la URL.
export default function Ordenes() {
  const { esAdmin, esSolicitante } = useRol()
  const { search } = useLocation()
  const [, setParams] = useSearchParams()
  const filtros = useFiltrosUrl()
  const estadosElegidos = filtros.valores('idEstadoOrden')
  const idActivo = filtros.valor('idActivo')
  const bandeja = filtros.valor('bandeja')
  const pagina = filtros.pagina

  const estados = useCarga(estadosOrden)
  const vehiculos = useCarga(listarTodosLosActivos)

  // ?bandeja=taller se traduce a los ids de esos estados según el catálogo de la API (no hay ids fijos aquí)
  useEffect(() => {
    if (bandeja !== 'taller' || !estados.datos) return
    const ids = estados.datos.data.filter((e) => BANDEJA_TALLER.includes(e.nombre)).map((e) => e.idEstadoOrden)
    setParams(() => new URLSearchParams(ids.map((id) => ['idEstadoOrden', String(id)])), { replace: true })
  }, [bandeja, estados.datos, setParams])

  const claveEstados = filtros.valores('idEstadoOrden').join(',')
  const cargar = useCallback(
    () => listarOrdenes(pagina, TAMANO, { idEstadoOrden: claveEstados ? claveEstados.split(',') : [], idActivo }),
    [pagina, claveEstados, idActivo],
  )
  const lista = useCarga(cargar)

  const alternarEstado = (id) => {
    const valor = String(id)
    filtros.cambiar('idEstadoOrden', estadosElegidos.includes(valor) ? estadosElegidos.filter((e) => e !== valor) : [...estadosElegidos, valor])
  }

  const desde = { desde: `/ordenes${search}` }
  const titulo = esSolicitante ? 'Mis solicitudes' : 'Órdenes'
  const nombresElegidos = (estados.datos?.data ?? []).filter((e) => estadosElegidos.includes(String(e.idEstadoOrden))).map((e) => e.nombre)

  return (
    <>
      <EncabezadoPagina
        titulo={titulo}
        detalle={esSolicitante ? 'Las solicitudes de mantenimiento que usted registró y en qué van.' : 'Las órdenes de mantenimiento de la flota.'}
        acciones={(esAdmin || esSolicitante) && <Button asChild><Link to="/ordenes/nueva"><Plus aria-hidden="true" />Nueva solicitud</Link></Button>}
      />
      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
        <fieldset className="flex min-w-0 flex-col gap-1.5">
          <legend className="mb-1.5 text-sm font-semibold">Estado</legend>
          <div className="flex flex-wrap gap-1.5">
            {(estados.datos?.data ?? []).map((e) => {
              const activo = estadosElegidos.includes(String(e.idEstadoOrden))
              return (
                <button
                  key={e.idEstadoOrden}
                  type="button"
                  aria-pressed={activo}
                  onClick={() => alternarEstado(e.idEstadoOrden)}
                  className={cn(
                    'h-9 rounded-sm border px-3 text-sm font-semibold transition-colors',
                    activo ? 'border-primary bg-accent text-accent-foreground' : 'border-border bg-card text-muted-foreground hover:border-input hover:text-foreground',
                  )}
                >
                  {e.nombre}
                </button>
              )
            })}
          </div>
        </fieldset>
        <div className="flex w-72 flex-col gap-1.5">
          <label htmlFor="filtro-vehiculo" className="text-sm font-semibold">Vehículo</label>
          <ListaOpciones id="filtro-vehiculo" value={idActivo} onChange={(v) => filtros.cambiar('idActivo', v)} todas="Todos los vehículos"
            opciones={(vehiculos.datos?.data ?? []).map((v) => ({ valor: v.idActivo, texto: `${v.codigo} · ${v.nombre}` }))} />
        </div>
      </BarraFiltros>
      <TablaPaginada
        titulo={titulo}
        nombre="órdenes"
        filas={lista.datos?.data ?? []}
        claveFila={(o) => o.idOrden}
        cargando={lista.cargando || bandeja === 'taller'}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={filtros.hayFiltros
          ? {
            titulo: nombresElegidos.length ? `No hay órdenes ${nombresElegidos.length === 1 ? 'en estado' : 'en los estados'} ${nombresElegidos.join(', ')}` : 'Ninguna orden coincide con el filtro',
            tipo: 'sinResultados',
            accion: <Button variant="outline" onClick={filtros.limpiar}>Limpiar filtros</Button>,
          }
          : esSolicitante
            ? { titulo: 'Todavía no ha registrado solicitudes', mensaje: 'Cuando un vehículo necesite mantenimiento, regístrelo con "Nueva solicitud".', accion: <Button asChild><Link to="/ordenes/nueva">Nueva solicitud</Link></Button> }
            : { titulo: 'No hay órdenes registradas' }}
        columnas={[
          { clave: 'codigo', titulo: 'Código', fija: true, render: (o) => <Link to={`/ordenes/${o.idOrden}`} state={desde} className="font-bold text-primary underline-offset-4 hover:underline">{o.codigo}</Link> },
          { clave: 'estado', titulo: 'Estado', render: (o) => <BadgeEstado estado={o.estadoOrdenNombre} /> },
          // Código arriba y nombre abajo: en tablet el nombre se lee completo en vez de quedar recortado a nada
          { clave: 'activo', titulo: 'Vehículo', render: (o) => <span className="flex min-w-40 flex-col"><strong>{o.activoCodigo}</strong><span className="text-sm text-muted-foreground">{o.activoNombre}</span></span> },
          { clave: 'tipoMantenimientoNombre', titulo: 'Tipo', recortar: true },
          { clave: 'prioridadNombre', titulo: 'Prioridad' },
          { clave: 'fechaSolicitud', titulo: 'Solicitud', render: (o) => formatFecha(o.fechaSolicitud) },
          { clave: 'diasEnEstadoActual', titulo: 'Días', derecha: true },
        ]}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={filtros.irAPagina}
      />
    </>
  )
}

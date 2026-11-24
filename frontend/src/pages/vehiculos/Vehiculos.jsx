import { useCallback } from 'react'
import { Link, useLocation } from 'react-router'
import { Eye } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import ImagenVehiculo from '@/components/shared/ImagenVehiculo'
import ListaOpciones from '@/components/shared/ListaOpciones'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { listarActivos } from '@/services/activoService'
import { estadosActivo } from '@/services/catalogoService'
import { listarCategorias } from '@/services/categoriaService'

const TAMANO = 10
// El Solicitante/Consulta no consulta categorías (la API le responde 403): no se le pide la lista
const sinCategorias = () => Promise.resolve({ data: [] })

// Vehículos (enunciado Avance 2, 5.2): los tres roles consultan.
// Filtros por categoría y estado guardados en la URL. El estado preventivo lo calcula la API con la misma
// consulta de la pantalla Preventivo; los vehículos Fuera de Servicio o Dados de Baja no se evalúan.
export default function Vehiculos() {
  const { esSolicitante } = useRol()
  const { search } = useLocation()
  const filtros = useFiltrosUrl()
  const idCategoria = filtros.valor('idCategoria')
  const idEstadoActivo = filtros.valor('idEstadoActivo')
  const pagina = filtros.pagina

  const categorias = useCarga(esSolicitante ? sinCategorias : listarCategorias)
  const estados = useCarga(estadosActivo)
  const cargar = useCallback(() => listarActivos(pagina, TAMANO, { idCategoria, idEstadoActivo }), [pagina, idCategoria, idEstadoActivo])
  const lista = useCarga(cargar)

  const desde = { desde: `/vehiculos${search}` }
  const enlace = (v) => `/vehiculos/${v.idActivo}`

  return (
    <>
      <EncabezadoPagina
        titulo="Vehículos"
        detalle="La flota de la empresa, con su estado y cómo va su mantenimiento preventivo."
      />
      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
        {!esSolicitante && (
          <div className="flex w-64 flex-col gap-1.5">
            <label htmlFor="filtro-categoria" className="text-sm font-semibold">Categoría</label>
            <ListaOpciones id="filtro-categoria" value={idCategoria} onChange={(v) => filtros.cambiar('idCategoria', v)} todas="Todas las categorías"
              opciones={(categorias.datos?.data ?? []).map((c) => ({ valor: c.idCategoria, texto: c.nombre }))} />
          </div>
        )}
        <div className="flex w-64 flex-col gap-1.5">
          <label htmlFor="filtro-estado" className="text-sm font-semibold">Estado</label>
          <ListaOpciones id="filtro-estado" value={idEstadoActivo} onChange={(v) => filtros.cambiar('idEstadoActivo', v)} todas="Todos los estados"
            opciones={(estados.datos?.data ?? []).map((e) => ({ valor: e.idEstadoActivo, texto: e.nombre }))} />
        </div>
      </BarraFiltros>
      <TablaPaginada
        titulo="Vehículos de la flota"
        nombre="vehículos"
        filas={lista.datos?.data ?? []}
        claveFila={(v) => v.idActivo}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={filtros.hayFiltros
          ? { titulo: 'Ningún vehículo coincide con el filtro', tipo: 'sinResultados', accion: <Button variant="outline" onClick={filtros.limpiar}>Limpiar filtros</Button> }
          : { titulo: 'No hay vehículos registrados' }}
        columnas={[
          { clave: 'imagen', titulo: <span className="sr-only">Foto</span>, render: (v) => <ImagenVehiculo ruta={v.imagen} alt="" className="h-9 w-12" /> },
          { clave: 'codigo', titulo: 'Código', fija: true, render: (v) => <Link to={enlace(v)} state={desde} className="font-bold text-primary underline-offset-4 hover:underline">{v.codigo}</Link> },
          { clave: 'nombre', titulo: 'Nombre', recortar: true },
          { clave: 'categoriaNombre', titulo: 'Categoría' },
          { clave: 'estado', titulo: 'Estado', render: (v) => <BadgeEstado estado={v.estadoActivoNombre} /> },
          {
            clave: 'preventivo', titulo: 'Preventivo',
            render: (v) => (v.estadoPreventivo ? <BadgeEstado estado={v.estadoPreventivo} /> : <span className="text-sm text-muted-foreground">No se evalúa</span>),
          },
          {
            clave: 'ver', titulo: <span className="sr-only">Acciones</span>, derecha: true,
            render: (v) => <Button asChild variant="ghost" size="sm"><Link to={enlace(v)} state={desde} aria-label={`Ver ${v.codigo}`}><Eye aria-hidden="true" />Ver</Link></Button>,
          },
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

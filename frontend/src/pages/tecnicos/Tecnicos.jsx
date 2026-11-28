import { useCallback, useState } from 'react'
import { Link, useLocation } from 'react-router'
import { Pencil, Plus, RotateCcw, UserX } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import CampoBusqueda from '@/components/shared/CampoBusqueda'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import EstadoVacio from '@/components/shared/EstadoVacio'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import ListaOpciones from '@/components/shared/ListaOpciones'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { listarEspecialidades } from '@/services/especialidadService'
import { listarTecnicosInactivos, listarTecnicosPagina, reactivarTecnico } from '@/services/tecnicoService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { formatColones } from '@/utils/format'

const TAMANO = 10

// Técnicos del taller (enunciado Avance 2, 5.2): filtros por especialidad y disponibilidad, que quedan en la URL.
export default function Tecnicos() {
  const { esAdmin } = useRol()
  const { search } = useLocation()
  const filtros = useFiltrosUrl()
  const idEspecialidad = filtros.valor('idEspecialidad')
  const disponible = filtros.valor('disponible')
  const buscar = filtros.valor('buscar')
  const pagina = filtros.pagina

  const cargar = useCallback(
    () => listarTecnicosPagina(pagina, TAMANO, { idEspecialidad, disponible, buscar }),
    [pagina, idEspecialidad, disponible, buscar],
  )
  const lista = useCarga(cargar)
  const especialidades = useCarga(listarEspecialidades)

  // El detalle recibe el listado con su filtro para el "Volver"
  const desde = { desde: `/tecnicos${search}` }

  const columnas = [
    {
      clave: 'nombre', titulo: 'Técnico', recortar: true, completo: (t) => t.nombreCompleto,
      render: (t) => <Link to={`/tecnicos/${t.idTecnico}`} state={desde} className="font-bold text-primary underline-offset-4 hover:underline">{t.nombreCompleto}</Link>,
    },
    { clave: 'identificacion', titulo: 'Identificación' },
    { clave: 'disponible', titulo: 'Disponibilidad', render: (t) => <BadgeEstado estado={t.disponible ? 'Disponible' : 'No disponible'} /> },
    { clave: 'tarifaHora', titulo: 'Tarifa por hora', derecha: true, render: (t) => formatColones(t.tarifaHora) },
    { clave: 'cantidadEspecialidades', titulo: 'Especialidades', derecha: true },
  ]
  if (esAdmin) {
    columnas.push({
      clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
      render: (t) => <Button asChild variant="ghost" size="sm"><Link to={`/tecnicos/${t.idTecnico}/editar`} state={desde}><Pencil aria-hidden="true" />Editar</Link></Button>,
    })
  }

  return (
    <>
      <EncabezadoPagina
        titulo="Técnicos"
        detalle="El personal del taller, su disponibilidad y su tarifa."
        acciones={esAdmin && (
          <>
            <TecnicosInactivos onReactivado={lista.recargar} />
            <Button asChild><Link to="/tecnicos/nuevo"><Plus aria-hidden="true" />Nuevo técnico</Link></Button>
          </>
        )}
      />
      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
        <CampoBusqueda valor={buscar} onCambiar={(texto) => filtros.cambiar('buscar', texto)} ayuda="Nombre o identificación." />
        <div className="flex w-64 flex-col gap-1.5">
          <label htmlFor="filtro-especialidad" className="text-sm font-semibold">Especialidad</label>
          <ListaOpciones id="filtro-especialidad" value={idEspecialidad} onChange={(v) => filtros.cambiar('idEspecialidad', v)} todas="Todas las especialidades"
            opciones={(especialidades.datos?.data ?? []).map((e) => ({ valor: e.idEspecialidad, texto: e.nombre }))} />
        </div>
        <div className="flex w-52 flex-col gap-1.5">
          <label htmlFor="filtro-disponible" className="text-sm font-semibold">Disponibilidad</label>
          <ListaOpciones id="filtro-disponible" value={disponible} onChange={(v) => filtros.cambiar('disponible', v)} todas="Todos"
            opciones={[{ valor: 'true', texto: 'Disponibles' }, { valor: 'false', texto: 'No disponibles' }]} />
        </div>
      </BarraFiltros>
      <TablaPaginada
        titulo="Técnicos del taller"
        nombre="técnicos"
        columnas={columnas}
        filas={lista.datos?.data ?? []}
        claveFila={(t) => t.idTecnico}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={filtros.hayFiltros
          ? { titulo: buscar ? `Ningún técnico coincide con «${buscar}»` : 'Ningún técnico coincide con el filtro', tipo: 'sinResultados', mensaje: buscar ? 'Revise lo escrito o cambie los filtros.' : 'Cambie la especialidad o la disponibilidad.', accion: <Button variant="outline" onClick={filtros.limpiar}>Limpiar filtros</Button> }
          : { titulo: 'No hay técnicos registrados' }}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={filtros.irAPagina}
      />
    </>
  )
}

// Los técnicos inactivos no aparecen en el taller; aquí el Administrador los ve y los devuelve con sus datos y especialidades
function TecnicosInactivos({ onReactivado }) {
  const [abierto, setAbierto] = useState(false)
  return (
    <Dialog open={abierto} onOpenChange={setAbierto}>
      <DialogTrigger asChild>
        <Button variant="outline"><UserX aria-hidden="true" />Técnicos inactivos</Button>
      </DialogTrigger>
      <DialogContent className="max-h-[90svh] gap-5 overflow-y-auto p-6 shadow-lg sm:max-w-lg" showCloseButton={false}>
        <DialogHeader className="gap-2 text-left">
          <DialogTitle className="text-lg font-semibold">Técnicos inactivos</DialogTitle>
          <DialogDescription className="text-base text-muted-foreground">Al reactivarlo, vuelve al taller con sus datos y especialidades.</DialogDescription>
        </DialogHeader>
        {abierto && <ListaInactivos onReactivado={onReactivado} />}
        <DialogFooter className="-mx-6 -mb-6 rounded-b-xl bg-muted/40 px-6 py-4">
          <Button variant="outline" onClick={() => setAbierto(false)}>Cerrar</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function ListaInactivos({ onReactivado }) {
  const inactivos = useCarga(listarTecnicosInactivos)
  const [procesando, setProcesando] = useState(null)

  const reactivar = async (t) => {
    setProcesando(t.idTecnico)
    try {
      await reactivarTecnico(t.idTecnico)
      avisarExito(`${t.nombreCompleto} volvió al taller`)
      inactivos.recargar()
      onReactivado()
    } catch (err) {
      avisarError(err)
    } finally {
      setProcesando(null)
    }
  }

  if (inactivos.cargando && !inactivos.datos) return <IndicadorCarga forma="bloque" texto="Cargando técnicos inactivos…" />
  if (inactivos.error) return <EstadoError error={inactivos.error} onReintentar={inactivos.recargar} />
  const lista = inactivos.datos?.data ?? []
  if (lista.length === 0) return <EstadoVacio tipo="positivo" titulo="No hay técnicos inactivos" />
  return (
    <ul className="flex flex-col divide-y divide-border-suave rounded-sm border">
      {lista.map((t) => (
        <li key={t.idTecnico} className="flex items-center gap-3 px-4 py-3">
          <span className="flex min-w-0 flex-1 flex-col">
            <strong className="truncate">{t.nombreCompleto}</strong>
            <span className="text-sm text-muted-foreground tabular-nums">{t.identificacion}</span>
          </span>
          <Button variant="outline" size="sm" disabled={procesando !== null} onClick={() => reactivar(t)}>
            <RotateCcw aria-hidden="true" />{procesando === t.idTecnico ? 'Reactivando…' : 'Reactivar'}
          </Button>
        </li>
      ))}
    </ul>
  )
}

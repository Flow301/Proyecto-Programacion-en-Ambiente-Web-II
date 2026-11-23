import { useCallback } from 'react'
import { Link, useLocation } from 'react-router'
import { Pencil, Plus } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import ListaOpciones from '@/components/shared/ListaOpciones'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { listarEspecialidades } from '@/services/especialidadService'
import { listarTecnicosPagina } from '@/services/tecnicoService'
import { formatColones } from '@/utils/format'

const TAMANO = 10

// Técnicos del taller (enunciado Avance 2, 5.2): filtros por especialidad y disponibilidad, que quedan en la URL.
export default function Tecnicos() {
  const { esAdmin } = useRol()
  const { search } = useLocation()
  const filtros = useFiltrosUrl()
  const idEspecialidad = filtros.valor('idEspecialidad')
  const disponible = filtros.valor('disponible')
  const pagina = filtros.pagina

  const cargar = useCallback(
    () => listarTecnicosPagina(pagina, TAMANO, { idEspecialidad, disponible }),
    [pagina, idEspecialidad, disponible],
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
        acciones={esAdmin && <Button asChild><Link to="/tecnicos/nuevo"><Plus aria-hidden="true" />Nuevo técnico</Link></Button>}
      />
      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
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
          ? { titulo: 'Ningún técnico coincide con el filtro', tipo: 'sinResultados', mensaje: 'Cambie la especialidad o la disponibilidad.', accion: <Button variant="outline" onClick={filtros.limpiar}>Limpiar filtros</Button> }
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

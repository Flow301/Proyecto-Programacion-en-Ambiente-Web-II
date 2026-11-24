import { useCallback, useState } from 'react'
import { Link } from 'react-router'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { eliminarCategoria, listarCategoriasPagina } from '@/services/categoriaService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { formatNumero } from '@/utils/format'

const TAMANO = 10

// Categorías de vehículo con su regla de mantenimiento preventivo (enunciado Avance 2, 5.2).
// Administrador: CRUD; Coordinador/Técnico: consulta.
export default function Categorias() {
  const { esAdmin } = useRol()
  const { pagina, irAPagina } = useFiltrosUrl()
  const cargar = useCallback(() => listarCategoriasPagina(pagina, TAMANO), [pagina])
  const lista = useCarga(cargar)
  const [eliminando, setEliminando] = useState(null)
  const [procesando, setProcesando] = useState(false)

  const confirmarEliminar = async () => {
    setProcesando(true)
    try {
      await eliminarCategoria(eliminando.idCategoria)
      avisarExito(`Categoría ${eliminando.nombre} eliminada`)
      setEliminando(null)
      lista.recargar()
    } catch (err) {
      // 409: tiene vehículos asociados; el mensaje de la API lo explica
      avisarError(err)
    } finally {
      setProcesando(false)
    }
  }

  const columnas = [
    { clave: 'nombre', titulo: 'Categoría', render: (c) => <strong>{c.nombre}</strong> },
    { clave: 'prefijo', titulo: 'Prefijo' },
    { clave: 'variable', titulo: 'Variable de control', render: (c) => `${c.variableControlNombre} (${c.variableControlUnidad})` },
    { clave: 'intervaloKm', titulo: 'Umbral', derecha: true, render: (c) => `${formatNumero(c.intervaloKm)} ${c.variableControlUnidad}` },
    { clave: 'porcentajeAviso', titulo: 'Aviso', derecha: true, render: (c) => `${c.porcentajeAviso} %` },
    { clave: 'cantidadActivos', titulo: 'Vehículos', derecha: true },
  ]
  if (esAdmin) {
    columnas.push({
      clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
      render: (c) => (
        <span className="inline-flex gap-1">
          <Button asChild variant="ghost" size="sm"><Link to={`/categorias/${c.idCategoria}/editar`}><Pencil aria-hidden="true" />Editar</Link></Button>
          <Button variant="ghost" size="sm" className="text-alto-texto hover:bg-alto-tinte" onClick={() => setEliminando(c)}><Trash2 aria-hidden="true" />Eliminar</Button>
        </span>
      ),
    })
  }

  return (
    <>
      <EncabezadoPagina
        titulo="Categorías"
        detalle="Clasifican los vehículos y definen cada cuánto toca su mantenimiento preventivo."
        acciones={esAdmin && <Button asChild><Link to="/categorias/nueva"><Plus aria-hidden="true" />Nueva categoría</Link></Button>}
      />
      <TablaPaginada
        titulo="Categorías de vehículo"
        nombre="categorías"
        columnas={columnas}
        filas={lista.datos?.data ?? []}
        claveFila={(c) => c.idCategoria}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={{ titulo: 'No hay categorías registradas' }}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={irAPagina}
      />
      <DialogoConfirmacion
        abierto={Boolean(eliminando)}
        titulo={`¿Eliminar la categoría ${eliminando?.nombre ?? ''}?`}
        mensaje="Se elimina de forma definitiva. Una categoría con vehículos asociados no se puede eliminar."
        textoConfirmar="Eliminar"
        peligro
        procesando={procesando}
        onCancelar={() => setEliminando(null)}
        onConfirmar={confirmarEliminar}
      />
    </>
  )
}

import { useCallback } from 'react'
import { Link, useLocation } from 'react-router'
import { Pencil, Plus } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { listarRepuestosPagina } from '@/services/repuestoService'
import { formatNumero } from '@/utils/format'

const TAMANO = 10

// Repuestos (enunciado Avance 2, 5.2): stock, mínimo, faltante e indicador de stock bajo, con el filtro
// "Stock bajo el mínimo". El faltante y el indicador los calcula la API.
export default function Repuestos() {
  const { esAdmin } = useRol()
  const { search } = useLocation()
  const filtros = useFiltrosUrl()
  const bajoMinimo = filtros.valor('bajoMinimo') === 'true'
  const pagina = filtros.pagina

  const cargar = useCallback(() => listarRepuestosPagina(pagina, TAMANO, { bajoMinimo: bajoMinimo || '' }), [pagina, bajoMinimo])
  const lista = useCarga(cargar)
  const desde = { desde: `/repuestos${search}` }

  const columnas = [
    { clave: 'codigo', titulo: 'Código', fija: true, render: (r) => <Link to={`/repuestos/${r.idRepuesto}`} state={desde} className="font-bold text-primary underline-offset-4 hover:underline">{r.codigo}</Link> },
    { clave: 'nombre', titulo: 'Repuesto', recortar: true },
    { clave: 'unidadMedida', titulo: 'Unidad' },
    { clave: 'stockActual', titulo: 'Stock', derecha: true, render: (r) => formatNumero(r.stockActual) },
    { clave: 'stockMinimo', titulo: 'Mínimo', derecha: true, render: (r) => formatNumero(r.stockMinimo) },
    { clave: 'faltante', titulo: 'Faltante', derecha: true, render: (r) => (r.faltante ? <strong>{formatNumero(r.faltante)}</strong> : '0') },
    { clave: 'stockBajo', titulo: 'Estado del stock', render: (r) => (r.stockBajo ? <BadgeEstado estado="Stock bajo" /> : <span className="text-sm text-muted-foreground">Suficiente</span>) },
  ]
  if (esAdmin) {
    columnas.push({
      clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
      render: (r) => <Button asChild variant="ghost" size="sm"><Link to={`/repuestos/${r.idRepuesto}/editar`} state={desde}><Pencil aria-hidden="true" />Editar</Link></Button>,
    })
  }

  return (
    <>
      <EncabezadoPagina
        titulo="Repuestos"
        detalle="El inventario de repuestos e insumos. El stock sube con las entradas y baja con el consumo en las órdenes."
        acciones={esAdmin && <Button asChild><Link to="/repuestos/nuevo"><Plus aria-hidden="true" />Nuevo repuesto</Link></Button>}
      />
      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
        <label className="flex min-h-10 items-center gap-3 text-[0.9375rem] font-semibold">
          <Checkbox checked={bajoMinimo} onCheckedChange={(v) => filtros.cambiar('bajoMinimo', v === true ? 'true' : '')} />
          Solo stock bajo el mínimo
        </label>
      </BarraFiltros>
      <TablaPaginada
        titulo={bajoMinimo ? 'Repuestos con stock bajo el mínimo' : 'Repuestos'}
        nombre="repuestos"
        columnas={columnas}
        filas={lista.datos?.data ?? []}
        claveFila={(r) => r.idRepuesto}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={bajoMinimo
          ? { titulo: 'No hay repuestos bajo el mínimo', tipo: 'positivo', mensaje: 'Todo el inventario está sobre su stock mínimo.' }
          : { titulo: 'No hay repuestos registrados' }}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={filtros.irAPagina}
      />
    </>
  )
}

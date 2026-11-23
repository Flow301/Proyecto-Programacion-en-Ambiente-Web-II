import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

/**
 * Barra de filtros siempre visible encima de la tabla. Los filtros van en la URL (useFiltrosUrl),
 * así recargar o volver desde un detalle conserva el filtro y la página.
 * children: los controles (cada uno con su etiqueta visible). onLimpiar muestra "Limpiar filtros".
 */
export default function BarraFiltros({ children, onLimpiar, hayFiltros = false, className }) {
  return (
    <div role="search" aria-label="Filtros" className={cn('flex flex-wrap items-end gap-x-6 gap-y-3 rounded-sm border bg-card px-4 py-3.5', className)}>
      {children}
      {onLimpiar && (
        <Button variant="ghost" className="ml-auto" disabled={!hayFiltros} onClick={onLimpiar}>
          Limpiar filtros
        </Button>
      )}
    </div>
  )
}

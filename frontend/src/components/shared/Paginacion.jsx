import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { formatNumero } from '@/utils/format'

// La misma paginación de TablaPaginada, para el historial (que es una línea de tiempo y no una tabla)
export default function Paginacion({ pagina, totalPaginas, totalRegistros, nombre, cargando, onCambiar }) {
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

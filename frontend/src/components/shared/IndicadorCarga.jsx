import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

/**
 * Indicador de carga con la forma de lo que viene, no un spinner a pantalla completa.
 * - forma "tabla": filas como las de la tabla (columnas = cuántas).
 * - forma "detalle": encabezado y bloques de un detalle.
 * - forma "bloque": unas líneas dentro de una sección.
 * El texto oculto avisa al lector de pantalla que se está cargando.
 */
export default function IndicadorCarga({ forma = 'tabla', filas = 5, columnas = 5, texto = 'Cargando…', className }) {
  return (
    <div role="status" aria-live="polite" className={cn('flex flex-col gap-3', className)}>
      <span className="sr-only">{texto}</span>
      {forma === 'tabla' && (
        <div aria-hidden="true" className="flex flex-col">
          {Array.from({ length: filas }, (_, f) => (
            <div key={f} className="flex h-11 items-center gap-4 border-b border-border-suave px-4">
              {Array.from({ length: columnas }, (_, c) => (
                <Skeleton key={c} className={cn('h-3.5', c === 0 ? 'w-28' : 'flex-1')} />
              ))}
            </div>
          ))}
        </div>
      )}
      {forma === 'detalle' && (
        <div aria-hidden="true" className="flex flex-col gap-5">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-4 w-96 max-w-full" />
          <Skeleton className="h-24 w-full" />
          <div className="grid gap-5 md:grid-cols-[2fr_1fr]">
            <Skeleton className="h-48" />
            <Skeleton className="h-48" />
          </div>
        </div>
      )}
      {forma === 'bloque' && (
        <div aria-hidden="true" className="flex flex-col gap-2.5">
          {Array.from({ length: filas }, (_, f) => (
            <Skeleton key={f} className={cn('h-3.5', f % 2 ? 'w-2/3' : 'w-full')} />
          ))}
        </div>
      )}
    </div>
  )
}

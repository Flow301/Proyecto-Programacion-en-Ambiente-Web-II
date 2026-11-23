import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCaption, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { cn } from '@/lib/utils'
import { formatNumero } from '@/utils/format'
import EstadoError from './EstadoError'
import EstadoVacio from './EstadoVacio'
import IndicadorCarga from './IndicadorCarga'

/**
 * Tabla paginada única del sistema (enunciado Avance 2, 4.6). La paginación es la de la API
 * (pageNumber, pageSize, totalRecords, totalPages): la tabla no pagina por su cuenta.
 *
 * - titulo: describe la tabla para lectores de pantalla (caption).
 * - columnas: [{ clave, titulo, derecha?, render?(fila) }]. Los montos y cantidades van con derecha.
 * - claveFila(fila): la llave de cada fila.
 * - vacio: { titulo, mensaje?, tipo?, accion? } para cuando no hay registros o el filtro no encontró nada.
 * - nombre: cómo se llaman los registros en el pie ("órdenes", "vehículos").
 * Las acciones por fila se limitan a ver y editar; las de proceso van en el detalle.
 */
export default function TablaPaginada({
  titulo, columnas, filas = [], claveFila, cargando = false, error, onReintentar, vacio,
  pagina = 1, tamano = 10, totalRegistros = 0, totalPaginas = 0, onCambiarPagina, nombre = 'registros', className,
}) {
  if (error) return <EstadoError error={error} onReintentar={onReintentar} className={className} />

  if (!cargando && filas.length === 0) {
    return (
      <EstadoVacio titulo={vacio?.titulo ?? `No hay ${nombre}`} mensaje={vacio?.mensaje} tipo={vacio?.tipo} className={cn('bg-card', className)}>
        {vacio?.accion}
      </EstadoVacio>
    )
  }

  const desde = (pagina - 1) * tamano + 1
  const hasta = Math.min(pagina * tamano, totalRegistros)

  return (
    <div className={cn('rounded-sm border bg-card', className)}>
      {cargando ? (
        <IndicadorCarga forma="tabla" columnas={columnas.length} texto={`Cargando ${nombre}…`} />
      ) : (
        <Table className="text-[0.9375rem] tabular-nums">
          <TableCaption className="sr-only">{titulo}</TableCaption>
          <TableHeader className="[&_tr]:border-border">
            <TableRow className="hover:bg-transparent">
              {columnas.map(({ clave, titulo: encabezado, derecha }) => (
                <TableHead key={clave} scope="col" className={cn('h-10 px-4 text-sm font-semibold text-muted-foreground', derecha && 'text-right')}>
                  {encabezado}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {filas.map((fila) => (
              <TableRow key={claveFila(fila)} className="border-border-suave hover:bg-muted/40">
                {columnas.map(({ clave, derecha, render }) => (
                  <TableCell key={clave} className={cn('h-11 px-4', derecha && 'text-right')}>
                    {render ? render(fila) : fila[clave]}
                  </TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      {totalPaginas > 0 && (
        <nav aria-label="Paginación" className="flex flex-wrap items-center justify-between gap-3 border-t border-border-suave px-4 py-3 text-sm tabular-nums">
          <span className="text-muted-foreground">
            {formatNumero(desde)}–{formatNumero(hasta)} de {formatNumero(totalRegistros)} {nombre}
          </span>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" disabled={cargando || pagina <= 1} onClick={() => onCambiarPagina(pagina - 1)}>
              <ChevronLeft aria-hidden="true" />
              Anterior
            </Button>
            <span aria-live="polite">Página {pagina} de {totalPaginas}</span>
            <Button variant="outline" size="sm" disabled={cargando || pagina >= totalPaginas} onClick={() => onCambiarPagina(pagina + 1)}>
              Siguiente
              <ChevronRight aria-hidden="true" />
            </Button>
          </div>
        </nav>
      )}
    </div>
  )
}

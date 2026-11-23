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
 * - columnas: [{ clave, titulo, derecha?, fija?, recortar?, completo?(fila), render?(fila) }].
 *   · derecha: montos y cantidades.
 *   · fija: la columna (el código) queda a la vista al desplazar la tabla de lado.
 *   · recortar: el texto largo (nombre del vehículo, tipo) se corta en una línea; completo(fila) da el texto
 *     entero para el tooltip. Así las filas mantienen sus 44 px. Los códigos nunca se parten.
 * - claveFila(fila): la llave de cada fila.
 * - vacio: { titulo, mensaje?, tipo?, accion? } para cuando no hay registros o el filtro no encontró nada.
 * - nombre: cómo se llaman los registros en el pie ("órdenes", "vehículos").
 * Las acciones por fila se limitan a ver y editar; las de proceso van en el detalle.
 */
// Columna fija a la izquierda: fondo opaco y un borde para que se note el desplazamiento por debajo
const FIJA = 'sticky left-0 z-[1] bg-card shadow-[1px_0_0_var(--border-suave)]'

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
              {columnas.map(({ clave, titulo: encabezado, derecha, fija }) => (
                <TableHead key={clave} scope="col" className={cn('h-10 px-4 text-sm font-semibold text-muted-foreground', derecha && 'text-right', fija && FIJA)}>
                  {encabezado}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {filas.map((fila) => (
              <TableRow key={claveFila(fila)} className="border-border-suave hover:bg-muted/40">
                {columnas.map(({ clave, derecha, fija, recortar, completo, render }) => {
                  const contenido = render ? render(fila) : fila[clave]
                  return (
                    // Una columna recortada no tiene ancho propio (max-w-0): se reparte lo que deja el resto
                    <TableCell key={clave} className={cn('h-11 px-4', derecha && 'text-right', fija && FIJA, recortar && 'w-1/4 max-w-0 min-w-32')}>
                      {recortar
                        ? <span className="block truncate" title={completo?.(fila) ?? (typeof contenido === 'string' ? contenido : undefined)}>{contenido}</span>
                        : contenido}
                    </TableCell>
                  )
                })}
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
            <Button variant="outline" disabled={cargando || pagina <= 1} onClick={() => onCambiarPagina(pagina - 1)}>
              <ChevronLeft aria-hidden="true" />
              Anterior
            </Button>
            <span aria-live="polite">Página {pagina} de {totalPaginas}</span>
            <Button variant="outline" disabled={cargando || pagina >= totalPaginas} onClick={() => onCambiarPagina(pagina + 1)}>
              Siguiente
              <ChevronRight aria-hidden="true" />
            </Button>
          </div>
        </nav>
      )}
    </div>
  )
}

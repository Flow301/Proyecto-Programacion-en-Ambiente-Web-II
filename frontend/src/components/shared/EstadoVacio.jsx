import { CircleCheckBig, Inbox, SearchX } from 'lucide-react'
import { cn } from '@/lib/utils'

/**
 * Estado vacío: la consulta salió bien pero no hay registros.
 * - tipo "vacio": no hay registros todavía ("No hay órdenes En Espera de Repuesto").
 * - tipo "sinResultados": el filtro no encontró nada; la acción sugerida suele ser limpiar filtros.
 * - tipo "positivo": que esté vacío es buena noticia ("No hay repuestos bajo el mínimo").
 * children: la acción sugerida (un botón o enlace), cuando aplica.
 */
const variantes = {
  vacio: { Icono: Inbox, color: 'text-muted-foreground' },
  sinResultados: { Icono: SearchX, color: 'text-muted-foreground' },
  positivo: { Icono: CircleCheckBig, color: 'text-exito' },
}

export default function EstadoVacio({ titulo, mensaje, tipo = 'vacio', children, className }) {
  const { Icono, color } = variantes[tipo]
  return (
    // Sin role="status": es contenido de la página, no un aviso; si no, cada estado vacío se anunciaría al cargar
    <div className={cn('flex flex-col items-start gap-2 rounded-sm border border-dashed px-6 py-6', className)}>
      <Icono className={cn('size-6', color)} aria-hidden="true" />
      <p className="text-base font-semibold">{titulo}</p>
      {mensaje && <p className="max-w-prose text-sm text-muted-foreground">{mensaje}</p>}
      {children && <div className="mt-1">{children}</div>}
    </div>
  )
}

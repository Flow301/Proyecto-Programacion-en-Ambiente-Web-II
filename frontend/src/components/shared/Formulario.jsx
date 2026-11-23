import { Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

/**
 * Piezas de los formularios de página (enunciado Avance 2, 4.4):
 * - Formulario: la superficie blanca, sin validación nativa (la hace Zod y la API).
 * - BloqueFormulario: un bloque con subtítulo ("Datos generales", "Regla de mantenimiento preventivo").
 * - AccionesFormulario: Cancelar (no guarda y regresa) y el envío, deshabilitado con progreso mientras procesa.
 */
export function Formulario({ onSubmit, children, className }) {
  return (
    <form noValidate onSubmit={onSubmit} className={cn('max-w-[55rem] rounded-sm border bg-card', className)}>
      {children}
    </form>
  )
}

export function BloqueFormulario({ titulo, detalle, columnas = 2, children }) {
  return (
    <fieldset className="border-b border-border-suave px-7 py-6 last-of-type:border-b-0">
      <legend className="float-left mb-4 w-full">
        <span className="block text-lg font-semibold">{titulo}</span>
        {detalle && <span className="mt-0.5 block text-sm text-muted-foreground">{detalle}</span>}
      </legend>
      <div className={cn('clear-both grid gap-x-5 gap-y-4', columnas === 2 && 'md:grid-cols-2', columnas === 3 && 'md:grid-cols-3')}>
        {children}
      </div>
    </fieldset>
  )
}

export function AccionesFormulario({ procesando, textoEnviar = 'Guardar', onCancelar }) {
  return (
    <div className="flex justify-end gap-2 border-t border-border-suave bg-muted/30 px-7 py-4">
      <Button type="button" variant="outline" disabled={procesando} onClick={onCancelar}>Cancelar</Button>
      <Button type="submit" disabled={procesando}>
        {procesando && <Loader2 className="animate-spin" aria-hidden="true" />}
        {procesando ? 'Guardando…' : textoEnviar}
      </Button>
    </div>
  )
}

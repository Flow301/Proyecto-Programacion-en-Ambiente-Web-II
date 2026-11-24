import { CircleAlert } from 'lucide-react'
import { cn } from '@/lib/utils'

/**
 * Campo de formulario con su etiqueta visible, ayuda y error (enunciado Avance 2, 4.4).
 * children es una función que recibe las props de accesibilidad para el control:
 *
 *   <CampoFormulario id="placa" etiqueta="Placa" requerido error={fieldState.error?.message}>
 *     {(props) => <Input {...field} {...props} />}
 *   </CampoFormulario>
 *
 * - requerido: agrega el * a la etiqueta.
 * - unidad: texto fijo dentro del campo, a la derecha ("km", "h").
 * - error: el mensaje del cliente (Zod) o el que mandó la API en un 400, junto al campo.
 */
export default function CampoFormulario({ id, etiqueta, requerido = false, ayuda, error, unidad, children, className }) {
  const idAyuda = `${id}-ayuda`
  const idError = `${id}-error`
  const describedBy = [error && idError, ayuda && idAyuda].filter(Boolean).join(' ') || undefined
  const control = children({
    id,
    'aria-invalid': error ? true : undefined,
    'aria-describedby': describedBy,
    'aria-required': requerido || undefined,
    className: unidad ? 'pr-12 text-right tabular-nums' : undefined,
  })

  return (
    <div className={cn('flex min-w-0 flex-col gap-1.5', className)}>
      <label htmlFor={id} className="w-fit text-[0.9375rem] font-semibold">
        {etiqueta}
        {requerido && <span aria-hidden="true"> *</span>}
      </label>
      {unidad ? (
        <div className="relative">
          {control}
          <span aria-hidden="true" className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-[0.9375rem] text-muted-foreground">
            {unidad}
          </span>
        </div>
      ) : control}
      {error && (
        <p id={idError} className="flex items-start gap-1.5 text-sm font-semibold text-alto-texto">
          <CircleAlert className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
          {error}
        </p>
      )}
      {ayuda && <p id={idAyuda} className="text-sm text-muted-foreground">{ayuda}</p>}
    </div>
  )
}

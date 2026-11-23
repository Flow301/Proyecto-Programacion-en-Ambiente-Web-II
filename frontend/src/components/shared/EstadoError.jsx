import { RotateCcw, Unplug, CircleAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

/**
 * Error al cargar datos, dentro de la pantalla (no la reemplaza).
 * Sin conexión (status 0): "No fue posible conectar con el servidor" con Reintentar (enunciado Avance 2, 4.5).
 * Otros errores: el mensaje de negocio de la API, nunca un código HTTP.
 */
export default function EstadoError({ error, onReintentar, className }) {
  const sinConexion = !error?.status
  const Icono = sinConexion ? Unplug : CircleAlert
  return (
    <div role="alert" className={cn('flex flex-col items-start gap-2 rounded-sm border bg-card px-6 py-6', className)}>
      <Icono className="size-6 text-alto" aria-hidden="true" />
      <p className="text-base font-semibold">
        {sinConexion ? 'No fue posible conectar con el servidor' : 'No se pudo cargar la información'}
      </p>
      <p className="max-w-prose text-sm text-muted-foreground">
        {sinConexion ? 'Revise su conexión e intente de nuevo.' : error.message}
      </p>
      {onReintentar && (
        <Button className="mt-1" onClick={onReintentar}>
          <RotateCcw aria-hidden="true" />
          Reintentar
        </Button>
      )}
    </div>
  )
}

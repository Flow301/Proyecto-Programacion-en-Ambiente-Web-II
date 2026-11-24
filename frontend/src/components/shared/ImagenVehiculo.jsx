import { useState } from 'react'
import { Truck } from 'lucide-react'
import { cn } from '@/lib/utils'
import { urlImagen } from '@/utils/format'

/**
 * Foto del vehículo: miniatura en el listado, grande en el detalle y vista previa en el formulario.
 * - ruta: la que devuelve la API ("imagenes/activos/PKP-0001.jpg") o una URL local de vista previa (blob:).
 * - Sin foto, o si no carga, se muestra un recuadro con el ícono de un camión: nunca una imagen rota.
 * - alt: el código y el nombre del vehículo. En el listado va vacío: la fila ya dice cuál es la unidad.
 */
export default function ImagenVehiculo({ ruta, alt, className }) {
  const src = ruta?.startsWith('blob:') ? ruta : urlImagen(ruta)
  const [fallida, setFallida] = useState(null)

  if (!src || fallida === src) {
    return (
      <div className={cn('flex shrink-0 items-center justify-center rounded-sm border bg-muted text-muted-foreground', className)}>
        <Truck className="size-1/2 max-h-12 max-w-12" aria-hidden="true" />
        {alt && <span className="sr-only">{alt}: sin foto</span>}
      </div>
    )
  }
  return (
    <img
      src={src}
      alt={alt}
      loading="lazy"
      onError={() => setFallida(src)}
      className={cn('shrink-0 rounded-sm border bg-muted object-cover', className)}
    />
  )
}
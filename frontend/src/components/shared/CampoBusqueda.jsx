import { useEffect, useRef, useState } from 'react'
import { Search, X } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'

const ESPERA_MS = 350
const MAX_LARGO = 50   // el mismo límite que valida la API (Busqueda.MaxLargo)

/**
 * Búsqueda por texto de los listados grandes. Va dentro de BarraFiltros y queda en la URL como los demás filtros.
 * Busca cuando el usuario deja de escribir un momento (o de inmediato con Enter), para no consultar la API por cada tecla.
 * - valor: el texto que está en la URL (useFiltrosUrl().valor('buscar')).
 * - onCambiar(texto): guarda el texto en la URL; vacío quita la búsqueda.
 * - ayuda: en qué campos busca (ej. "Código, vehículo o placa"); se muestra como texto de ayuda del campo.
 */
export default function CampoBusqueda({ valor, onCambiar, ayuda, id = 'buscar', className }) {
  const [texto, setTexto] = useState(valor)
  const cambiarRef = useRef(onCambiar)
  useEffect(() => { cambiarRef.current = onCambiar })

  // Si la búsqueda cambia por fuera (Limpiar filtros, volver desde un detalle), el campo la refleja
  const [anterior, setAnterior] = useState(valor)
  if (valor !== anterior) {
    setAnterior(valor)
    setTexto(valor)
  }

  useEffect(() => {
    if (texto.trim() === valor) return
    const temporizador = setTimeout(() => cambiarRef.current(texto.trim()), ESPERA_MS)
    return () => clearTimeout(temporizador)
  }, [texto, valor])

  return (
    <div className={cn('flex w-72 flex-col gap-1.5', className)}>
      <label htmlFor={id} className="text-sm font-semibold">Buscar</label>
      <div className="relative">
        <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
        <Input
          id={id}
          type="search"
          value={texto}
          maxLength={MAX_LARGO}
          autoComplete="off"
          spellCheck={false}
          aria-describedby={ayuda ? `${id}-ayuda` : undefined}
          className="pr-9 pl-9 [&::-webkit-search-cancel-button]:appearance-none"
          onChange={(e) => setTexto(e.target.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); cambiarRef.current(texto.trim()) } }}
        />
        {texto && (
          <button
            type="button"
            aria-label="Borrar la búsqueda"
            className="absolute top-1/2 right-1.5 grid size-7 -translate-y-1/2 place-items-center rounded-sm text-muted-foreground hover:bg-muted hover:text-foreground"
            onClick={() => { setTexto(''); cambiarRef.current('') }}
          >
            <X className="size-4" aria-hidden="true" />
          </button>
        )}
      </div>
      {ayuda && <span id={`${id}-ayuda`} className="text-sm text-muted-foreground">{ayuda}</span>}
    </div>
  )
}

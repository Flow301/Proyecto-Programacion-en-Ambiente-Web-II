import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { cn } from '@/lib/utils'

/**
 * Lista desplegable para relaciones y valores fijos (enunciado Avance 2, 4.4): el usuario elige, no escribe ids.
 * Las opciones vienen de la API. Los valores viajan como texto (Radix no admite valores vacíos):
 * '' significa "sin elegir" y se muestra el texto de ayuda.
 *
 * - opciones: [{ valor, texto, deshabilitada? }]
 * - todas: texto de una opción para "sin filtro" (en barras de filtros), con valor 'todas'.
 * El resto de props (id, aria-invalid, aria-describedby) llega de CampoFormulario y va al botón.
 */
export default function ListaOpciones({ value, onChange, opciones = [], placeholder = 'Elija una opción', todas, disabled, className, ...props }) {
  // Siempre controlado: '' (sin elegir) muestra el texto de ayuda; undefined haría que React lo tome como no controlado
  const actual = value === '' || value == null ? (todas ? 'todas' : '') : String(value)
  return (
    <Select value={actual} onValueChange={(v) => onChange(v === 'todas' ? '' : v)} disabled={disabled}>
      <SelectTrigger className={cn('w-full', className)} {...props}>
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent>
        {todas && <SelectItem value="todas">{todas}</SelectItem>}
        {opciones.map((o) => (
          <SelectItem key={o.valor} value={String(o.valor)} disabled={o.deshabilitada}>{o.texto}</SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}

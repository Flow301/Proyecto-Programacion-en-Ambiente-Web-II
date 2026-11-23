import { useId, useState } from 'react'
import { Loader2 } from 'lucide-react'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription,
  AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Textarea } from '@/components/ui/textarea'

/**
 * Confirmación antes de lo irreversible (enunciado Avance 2, 4.4): dar de baja, Fuera de Servicio,
 * rechazar o cancelar una orden, desactivar un usuario. Dice qué y sobre qué registro, y permite
 * una observación opcional. Radix atrapa el foco y lo devuelve al botón que la abrió.
 *
 * - titulo: la pregunta completa ("¿Cancelar la orden OM-2026-00042 del vehículo PKP-0003?").
 * - mensaje: qué pasa después (opcional).
 * - conObservacion: muestra el campo "Observación (opcional)"; onConfirmar la recibe.
 * - peligro: la acción es destructiva (botón rojo).
 * - procesando: deshabilita y muestra progreso mientras la API responde; no se cierra sola.
 */
export default function DialogoConfirmacion({ abierto, procesando = false, onCancelar, ...contenido }) {
  return (
    <AlertDialog open={abierto} onOpenChange={(siguiente) => !siguiente && !procesando && onCancelar()}>
      <AlertDialogContent className="gap-5 p-6 shadow-lg data-[size=default]:sm:max-w-lg">
        {/* Vive dentro del contenido: al cerrarse se desmonta y la observación no queda para la próxima vez */}
        <Contenido procesando={procesando} {...contenido} />
      </AlertDialogContent>
    </AlertDialog>
  )
}

function Contenido({ titulo, mensaje, textoConfirmar = 'Confirmar', textoCancelar = 'Volver', conObservacion = false, peligro = false, procesando, onConfirmar }) {
  const [observacion, setObservacion] = useState('')
  const idObservacion = useId()

  return (
    <>
      <AlertDialogHeader className="place-items-start gap-2 text-left">
        <AlertDialogTitle className="text-lg font-semibold">{titulo}</AlertDialogTitle>
        {mensaje && <AlertDialogDescription className="text-base text-muted-foreground">{mensaje}</AlertDialogDescription>}
      </AlertDialogHeader>
      {conObservacion && (
        <div className="flex flex-col gap-1.5">
          <label htmlFor={idObservacion} className="text-[0.9375rem] font-semibold">Observación (opcional)</label>
          <Textarea
            id={idObservacion}
            value={observacion}
            maxLength={500}
            disabled={procesando}
            onChange={(e) => setObservacion(e.target.value)}
          />
        </div>
      )}
      <AlertDialogFooter className="-mx-6 -mb-6 rounded-b-xl bg-muted/40 px-6 py-4">
        <AlertDialogCancel disabled={procesando}>{textoCancelar}</AlertDialogCancel>
        <AlertDialogAction
          variant={peligro ? 'peligro' : 'default'}
          disabled={procesando}
          onClick={(e) => {
            e.preventDefault() // se cierra cuando la API responde, no antes
            onConfirmar(observacion.trim() || null)
          }}
        >
          {procesando && <Loader2 className="animate-spin" aria-hidden="true" />}
          {procesando ? 'Procesando…' : textoConfirmar}
        </AlertDialogAction>
      </AlertDialogFooter>
    </>
  )
}

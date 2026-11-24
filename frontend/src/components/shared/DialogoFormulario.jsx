import { useLayoutEffect, useRef } from 'react'
import { Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'

/**
 * Ventana con un formulario corto: crear o editar especialidades y proveedores, asociar una especialidad
 * a un técnico, un proveedor a un repuesto, registrar una entrada de stock (enunciado Avance 2, 2.2 y 5.2).
 * - onEnviar: el handleSubmit del formulario; la ventana no se cierra sola mientras procesa.
 * - Al cerrar, el foco vuelve al botón que la abrió.
 */
export default function DialogoFormulario({ abierto, titulo, descripcion, textoEnviar = 'Guardar', procesando = false, onEnviar, onCerrar, children }) {
  const origen = useRef(null)
  useLayoutEffect(() => {
    if (abierto) origen.current = document.activeElement
  }, [abierto])

  return (
    <Dialog open={abierto} onOpenChange={(siguiente) => !siguiente && !procesando && onCerrar()}>
      <DialogContent
        className="max-h-[90svh] gap-5 overflow-y-auto p-6 shadow-lg sm:max-w-lg"
        showCloseButton={false}
        onCloseAutoFocus={(e) => {
          if (!origen.current?.isConnected) return
          e.preventDefault()
          origen.current.focus()
        }}
      >
        <form noValidate onSubmit={onEnviar} className="flex flex-col gap-5">
          <DialogHeader className="gap-2 text-left">
            <DialogTitle className="text-lg font-semibold text-balance">{titulo}</DialogTitle>
            {descripcion && <DialogDescription className="text-base text-muted-foreground">{descripcion}</DialogDescription>}
          </DialogHeader>
          <div className="flex flex-col gap-4">{children}</div>
          <DialogFooter className="-mx-6 -mb-6 rounded-b-xl bg-muted/40 px-6 py-4">
            <Button type="button" variant="outline" disabled={procesando} onClick={onCerrar}>Cancelar</Button>
            <Button type="submit" disabled={procesando}>
              {procesando && <Loader2 className="animate-spin" aria-hidden="true" />}
              {procesando ? 'Guardando…' : textoEnviar}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

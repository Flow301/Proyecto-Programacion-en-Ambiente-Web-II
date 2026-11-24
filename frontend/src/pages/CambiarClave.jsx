import { useNavigate } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import CampoFormulario from '@/components/shared/CampoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { cambioClaveSchema } from '@/schemas/authSchemas'
import { cambiarClave } from '@/services/authService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'

const campos = [
  { name: 'claveActual', etiqueta: 'Contraseña actual', autoComplete: 'current-password' },
  { name: 'claveNueva', etiqueta: 'Nueva contraseña', autoComplete: 'new-password', ayuda: 'Entre 8 y 100 caracteres, distinta de la actual.' },
  { name: 'confirmacion', etiqueta: 'Repita la nueva contraseña', autoComplete: 'new-password' },
]

// Cambio de la contraseña propia, para los tres roles (plan de trabajo, semana 9)
export default function CambiarClave() {
  const navigate = useNavigate()
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(cambioClaveSchema),
    defaultValues: { claveActual: '', claveNueva: '', confirmacion: '' },
  })

  const enviar = async ({ claveActual, claveNueva }) => {
    try {
      await cambiarClave(claveActual, claveNueva)
      avisarExito('La contraseña se cambió correctamente.')
      navigate(-1)
    } catch (err) {
      // La API responde 400 si la contraseña actual no es correcta: el error va junto a ese campo
      if (err.status === 400 && /actual/i.test(err.message)) setError('claveActual', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, campos.map((c) => c.name))) avisarError(err)
    }
  }

  return (
    <>
      <EncabezadoPagina titulo="Cambiar contraseña" detalle="Al terminar, la nueva contraseña se usa desde el próximo ingreso." />
      <form noValidate onSubmit={handleSubmit(enviar)} className="flex max-w-md flex-col gap-5 rounded-sm border bg-card p-6">
        {campos.map(({ name, etiqueta, autoComplete, ayuda }) => (
          <Controller key={name} name={name} control={control} render={({ field, fieldState }) => (
            <CampoFormulario id={name} etiqueta={etiqueta} requerido ayuda={ayuda} error={fieldState.error?.message}>
              {(props) => <Input {...field} {...props} type="password" autoComplete={autoComplete} />}
            </CampoFormulario>
          )} />
        ))}
        <div className="flex justify-end gap-2 border-t border-border-suave pt-4">
          <Button type="button" variant="outline" disabled={isSubmitting} onClick={() => navigate(-1)}>Cancelar</Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Loader2 className="animate-spin" aria-hidden="true" />}
            {isSubmitting ? 'Guardando…' : 'Cambiar contraseña'}
          </Button>
        </div>
      </form>
    </>
  )
}

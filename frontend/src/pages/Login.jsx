import { Navigate, useLocation } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Check, CircleAlert, Clock, Loader2, LogIn, Wrench } from 'lucide-react'
import CampoFormulario from '@/components/shared/CampoFormulario'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/context/AuthContext'
import { cn } from '@/lib/utils'
import { loginSchema } from '@/schemas/authSchemas'

const PASOS = ['Solicitada', 'Diagnosticada', 'Aprobada', 'En Ejecución', 'Completada']

const campos = [
  { name: 'usuario', etiqueta: 'Usuario', type: 'text', autoComplete: 'username' },
  { name: 'clave', etiqueta: 'Contraseña', type: 'password', autoComplete: 'current-password' },
]

export default function Login() {
  const { sesion, expirada, iniciarSesion } = useAuth()
  const location = useLocation()
  const { control, handleSubmit, setError, setFocus, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(loginSchema),
    defaultValues: { usuario: '', clave: '' },
  })

  // Con sesión (también justo después de ingresar) se va a la pantalla que se pidió antes del login
  if (sesion) return <Navigate to={location.state?.desde ?? '/'} replace />

  // Solo se ejecuta si Zod aprobó los datos. Un error no borra lo escrito (enunciado Avance 2, 5.1)
  const enviar = async ({ usuario, clave }) => {
    try {
      await iniciarSesion(usuario, clave)
    } catch (err) {
      setError('root', { message: err.status ? err.message : 'No fue posible conectar con el servidor. Intente de nuevo.' })
      setFocus('clave', { shouldSelect: true }) // listo para corregir la contraseña sin buscar el campo
    }
  }

  return (
    <div className="flex min-h-svh flex-wrap">
      <title>Ingresar · SGMA</title>
      <div className="flex flex-[1_1_28rem] flex-col justify-between gap-12 bg-primary px-16 py-14 text-primary-foreground">
        <div className="flex flex-col gap-3">
          <p className="text-5xl leading-none font-bold tracking-[0.06em]">SGMA</p>
          <p className="max-w-sm text-xl leading-snug">Sistema de Gestión de Mantenimiento de Activos</p>
        </div>
        {/* La ruta de una orden es la protagonista: el mismo recorrido que el sistema muestra en cada orden */}
        <div className="flex flex-col gap-6">
          <p className="max-w-md text-2xl leading-snug font-semibold">Cada orden de la flota, de la solicitud al cierre.</p>
          <ol className="grid max-w-xl grid-cols-5 gap-x-2">
            {PASOS.map((paso, i) => (
              <li key={paso} className="relative flex flex-col items-center gap-2.5 text-center">
                {i < PASOS.length - 1 && <span aria-hidden="true" className={cn('absolute top-[18px] left-1/2 w-full', i < 3 ? 'h-1 bg-primary-foreground/85' : 'border-t-[3px] border-dashed border-primary-foreground/50')} />}
                <span
                  aria-hidden="true"
                  className={cn(
                    'relative grid place-items-center rounded-t-full rounded-b-[3px]',
                    i < 3 && 'h-10 w-7 bg-primary-foreground/85 text-primary',
                    i === 3 && '-mt-2 h-12 w-9 bg-primary-foreground text-primary ring-4 ring-primary-foreground/25',
                    i === 4 && 'h-10 w-7 border-2 border-primary-foreground/70',
                  )}
                >
                  {i < 3 && <Check className="size-4" />}
                  {i === 3 && <Wrench className="size-4" />}
                </span>
                <span className="text-sm font-semibold">{paso}</span>
              </li>
            ))}
          </ol>
        </div>
        <p className="text-[0.9375rem]">Mantenimiento de flota</p>
      </div>

      <main className="flex flex-[1_1_28rem] items-center px-16 py-14">
        <form onSubmit={handleSubmit(enviar)} noValidate className="flex w-full max-w-sm flex-col gap-5">
          <h1 className="text-2xl font-bold">Ingresar</h1>

          {expirada && !errors.root && (
            <p role="status" className="flex items-start gap-2.5 rounded-sm bg-aviso-tinte px-3.5 py-3 text-[0.9375rem] text-aviso-texto">
              <Clock className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
              Su sesión expiró, ingrese nuevamente.
            </p>
          )}
          {errors.root && (
            <p role="alert" className="flex items-start gap-2.5 rounded-sm bg-alto-tinte px-3.5 py-3 text-[0.9375rem] text-alto-texto">
              <CircleAlert className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
              {errors.root.message}
            </p>
          )}

          {campos.map(({ name, etiqueta, type, autoComplete }) => (
            <Controller
              key={name}
              name={name}
              control={control}
              render={({ field, fieldState }) => (
                <CampoFormulario id={name} etiqueta={etiqueta} requerido error={fieldState.error?.message}>
                  {(props) => (
                    <Input {...field} {...props} type={type} autoComplete={autoComplete} spellCheck={false} autoFocus={name === 'usuario'} className="h-11 text-base" />
                  )}
                </CampoFormulario>
              )}
            />
          ))}

          <Button type="submit" size="lg" disabled={isSubmitting}>
            {isSubmitting ? <Loader2 className="animate-spin" aria-hidden="true" /> : <LogIn aria-hidden="true" />}
            {isSubmitting ? 'Ingresando…' : 'Ingresar'}
          </Button>
          <p className="text-sm text-muted-foreground">¿Olvidó su contraseña? Solicite al Administrador que la restablezca.</p>
        </form>
      </main>
    </div>
  )
}

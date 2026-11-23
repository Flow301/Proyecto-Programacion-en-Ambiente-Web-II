import { Navigate, useLocation, useNavigate } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { CircleAlert, Clock, Loader2, LogIn } from 'lucide-react'
import CampoFormulario from '@/components/shared/CampoFormulario'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/context/AuthContext'
import { loginSchema } from '@/schemas/authSchemas'

const campos = [
  { name: 'usuario', etiqueta: 'Usuario', type: 'text', autoComplete: 'username' },
  { name: 'clave', etiqueta: 'Contraseña', type: 'password', autoComplete: 'current-password' },
]

export default function Login() {
  const { sesion, expirada, iniciarSesion } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(loginSchema),
    defaultValues: { usuario: '', clave: '' },
  })

  if (sesion) return <Navigate to="/" replace />

  // Solo se ejecuta si Zod aprobó los datos. Un error no borra lo escrito (enunciado Avance 2, 5.1)
  const enviar = async ({ usuario, clave }) => {
    try {
      await iniciarSesion(usuario, clave)
      navigate(location.state?.desde ?? '/', { replace: true })
    } catch (err) {
      setError('root', { message: err.status ? err.message : 'No fue posible conectar con el servidor. Intente de nuevo.' })
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
        <div className="flex flex-col gap-4">
          {/* La ruta de una orden: el mismo gesto del indicador de progreso */}
          <div aria-hidden="true" className="relative grid max-w-md grid-cols-5 items-end">
            <span className="absolute inset-x-2 bottom-2 h-[3px] bg-primary-foreground/35" />
            <span className="relative h-[22px] w-4 rounded-t-full rounded-b-[2px] bg-primary-foreground/85" />
            <span className="relative h-[22px] w-4 rounded-t-full rounded-b-[2px] bg-primary-foreground/85" />
            <span className="relative h-[22px] w-4 rounded-t-full rounded-b-[2px] bg-primary-foreground/85" />
            <span className="relative h-8 w-[22px] rounded-t-full rounded-b-[3px] bg-primary-foreground" />
            <span className="relative h-[22px] w-4 rounded-t-full rounded-b-[2px] border-2 border-primary-foreground/60" />
          </div>
          <p className="text-[0.9375rem]">Mantenimiento de la flota, de la solicitud al cierre.</p>
        </div>
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
                    <Input {...field} {...props} type={type} autoComplete={autoComplete} autoFocus={name === 'usuario'} className="h-11 text-base" />
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

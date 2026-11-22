import { Navigate, useLocation, useNavigate } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/context/AuthContext'
import { loginSchema } from '@/schemas/authSchemas'

const campos = [
  { name: 'usuario', label: 'Usuario', type: 'text', autoComplete: 'username' },
  { name: 'clave', label: 'Contraseña', type: 'password', autoComplete: 'current-password' },
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
      setError('root', { message: err.message })
    }
  }

  return (
    <main className="flex min-h-svh items-center justify-center p-4">
      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle className="text-2xl text-primary">SGMA</CardTitle>
          <CardDescription>Sistema de Gestión de Mantenimiento de Activos</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(enviar)} noValidate>
            <FieldGroup>
              {expirada && !errors.root && (
                <p role="status" className="rounded-md border-l-4 border-secondary bg-secondary/15 px-3 py-2 text-sm">
                  Su sesión expiró, ingrese nuevamente.
                </p>
              )}
              {errors.root && (
                <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {errors.root.message}
                </p>
              )}

              {campos.map(({ name, label, type, autoComplete }) => (
                <Controller key={name} name={name} control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor={name}>{label} *</FieldLabel>
                      <Input {...field} id={name} type={type} autoComplete={autoComplete}
                        autoFocus={name === 'usuario'} aria-invalid={fieldState.invalid} />
                      {fieldState.invalid && <FieldError errors={[fieldState.error]} />}
                    </Field>
                  )}
                />
              ))}

              <Button type="submit" className="w-full" disabled={isSubmitting}>
                {isSubmitting && <Loader2 className="animate-spin" />}
                {isSubmitting ? 'Ingresando…' : 'Ingresar'}
              </Button>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}

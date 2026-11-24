import { useCallback } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import CampoFormulario from '@/components/shared/CampoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import { AccionesFormulario, BloqueFormulario, Formulario } from '@/components/shared/Formulario'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import PaginaError from '@/components/shared/PaginaError'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import useCarga from '@/hooks/useCarga'
import { correo, numero, telefono, texto } from '@/schemas/comunes'
import { actualizarTecnico, crearTecnico, obtenerTecnico } from '@/services/tecnicoService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'

// Mismas reglas que TecnicoCreateDto
const esquema = z.object({
  nombreCompleto: texto('el nombre completo', 100),
  identificacion: z.string().trim().min(1, 'Ingrese la cédula.').regex(/^\d-\d{4}-\d{4}$/, 'Use el formato 1-0845-0231.'),
  telefono,
  correo,
  tarifaHora: numero('la tarifa por hora', { min: 0.01, entero: false }),
  disponible: z.boolean(),
})
const CAMPOS = Object.keys(esquema.shape)
const sinTecnico = () => Promise.resolve(null)

// Formulario del técnico: nuevo (/tecnicos/nuevo) o edición (/tecnicos/:id/editar). Solo Administrador.
export default function TecnicoFormulario() {
  const { id } = useParams()
  const cargar = useCallback(() => (id ? obtenerTecnico(id) : sinTecnico()), [id])
  const tecnico = useCarga(cargar)
  const titulo = id ? 'Editar técnico' : 'Nuevo técnico'

  if (tecnico.error?.status === 404) return <PaginaError codigo={404} />
  if (tecnico.cargando) return <><EncabezadoPagina titulo={titulo} /><IndicadorCarga forma="detalle" /></>
  if (tecnico.error) return <><EncabezadoPagina titulo={titulo} /><EstadoError error={tecnico.error} onReintentar={tecnico.recargar} /></>
  return <Contenido tecnico={tecnico.datos?.data} titulo={titulo} />
}

function Contenido({ tecnico, titulo }) {
  const navigate = useNavigate()
  const { state } = useLocation()
  // Al cancelar o guardar se vuelve a donde se estaba: el detalle o el listado con su filtro
  const regreso = tecnico ? `/tecnicos/${tecnico.idTecnico}` : (state?.desde ?? '/tecnicos')
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    defaultValues: {
      nombreCompleto: tecnico?.nombreCompleto ?? '',
      identificacion: tecnico?.identificacion ?? '',
      telefono: tecnico?.telefono ?? '',
      correo: tecnico?.correo ?? '',
      tarifaHora: tecnico ? String(tecnico.tarifaHora) : '',
      disponible: tecnico?.disponible ?? true,
    },
  })

  const enviar = async (datos) => {
    try {
      const { data } = tecnico ? await actualizarTecnico(tecnico.idTecnico, datos) : await crearTecnico(datos)
      avisarExito(`Técnico ${datos.nombreCompleto} ${tecnico ? 'actualizado' : 'registrado'}`)
      navigate(tecnico ? regreso : `/tecnicos/${data?.idTecnico ?? ''}`, { state })
    } catch (err) {
      // 409: la identificación ya está registrada
      if (err.status === 409) setError('identificacion', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, CAMPOS)) avisarError(err)
    }
  }

  const campo = (name, etiqueta, opciones = {}) => (
    <Controller name={name} control={control} render={({ field, fieldState }) => (
      <CampoFormulario id={name} etiqueta={etiqueta} requerido ayuda={opciones.ayuda} unidad={opciones.unidad} error={fieldState.error?.message}>
        {(props) => <Input {...field} {...props} type={opciones.type ?? 'text'} inputMode={opciones.inputMode} autoComplete="off" />}
      </CampoFormulario>
    )} />
  )

  return (
    <>
      <EncabezadoPagina titulo={titulo} pestana={tecnico ? `Editar ${tecnico.nombreCompleto}` : titulo} volver={{ to: regreso, texto: tecnico ? 'Volver al técnico' : 'Volver a Técnicos' }} />
      <p className="-mt-2 text-sm text-muted-foreground">Los campos con * son obligatorios.</p>
      <Formulario onSubmit={handleSubmit(enviar)}>
        <BloqueFormulario titulo="Datos generales">
          {campo('nombreCompleto', 'Nombre completo')}
          {campo('identificacion', 'Cédula', { ayuda: 'Formato 1-0845-0231.' })}
          {campo('telefono', 'Teléfono', { type: 'tel', ayuda: 'Formato 8888-8888.' })}
          {campo('correo', 'Correo', { type: 'email' })}
        </BloqueFormulario>
        <BloqueFormulario titulo="Datos del taller" detalle="Las especialidades se agregan desde el detalle del técnico.">
          {campo('tarifaHora', 'Tarifa por hora (₡)', { inputMode: 'decimal', ayuda: 'Se registra en cada asignación; un cambio no altera órdenes cerradas.' })}
          <Controller name="disponible" control={control} render={({ field }) => (
            <div className="flex flex-col gap-1.5">
              <span className="text-[0.9375rem] font-semibold">Disponibilidad</span>
              <label className="flex min-h-10 items-center gap-3 text-[0.9375rem]">
                <Checkbox checked={field.value} onCheckedChange={(v) => field.onChange(v === true)} />
                Disponible para asignar órdenes
              </label>
              <span className="text-sm text-muted-foreground">Desmárquelo durante vacaciones o incapacidades.</span>
            </div>
          )} />
        </BloqueFormulario>
        <AccionesFormulario procesando={isSubmitting} textoEnviar={tecnico ? 'Guardar cambios' : 'Registrar técnico'} onCancelar={() => navigate(regreso, { state })} />
      </Formulario>
    </>
  )
}

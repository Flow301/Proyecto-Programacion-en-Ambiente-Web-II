import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import { Input } from '@/components/ui/input'
import { numero } from '@/schemas/comunes'
import { registrarLectura } from '@/services/activoService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'
import { aFechaIso, formatFecha, formatKm } from '@/utils/format'

// "Registrar lectura de uso" (enunciado Avance 2, 5.2): la única forma de cambiar el kilometraje fuera del diagnóstico
// de una orden, porque alimenta el mantenimiento preventivo. El kilometraje es acumulativo: la lectura no puede ser
// menor que la actual. Aquí se avisa antes de enviar; la API lo valida de todos modos (400 junto al campo).
export default function DialogoLectura({ vehiculo, onCerrar, onRegistrada }) {
  const unidad = vehiculo.preventivo?.variableControlUnidad ?? 'km'
  const hoy = aFechaIso()
  const esquema = z.object({
    kilometraje: numero('la lectura', { min: vehiculo.kilometrajeActual }),
    fecha: z.string().refine((f) => !f || f <= hoy, 'La fecha no puede ser futura.'),
  })
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    defaultValues: { kilometraje: '', fecha: hoy },
  })

  const enviar = async ({ kilometraje, fecha }) => {
    try {
      await registrarLectura(vehiculo.idActivo, { kilometraje, fecha: fecha || null })
      avisarExito(`Lectura de ${formatKm(kilometraje, unidad)} registrada para ${vehiculo.codigo}`)
      onRegistrada()
    } catch (err) {
      if (!aplicarErroresApi(err, setError, ['kilometraje', 'fecha'])) avisarError(err)
    }
  }

  const ultima = vehiculo.preventivo?.fechaUltimaLectura
  return (
    <DialogoFormulario
      abierto
      titulo={`Registrar lectura de uso de ${vehiculo.codigo}`}
      descripcion={`Lectura actual: ${formatKm(vehiculo.kilometrajeActual, unidad)}${ultima ? ` (${formatFecha(ultima)})` : ''}. La nueva no puede ser menor.`}
      textoEnviar="Registrar lectura"
      procesando={isSubmitting}
      onEnviar={handleSubmit(enviar)}
      onCerrar={onCerrar}
    >
      <Controller name="kilometraje" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="kilometraje" etiqueta="Lectura del odómetro" requerido unidad={unidad} error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} inputMode="numeric" autoComplete="off" />}
        </CampoFormulario>
      )} />
      <Controller name="fecha" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="fecha" etiqueta="Fecha de la lectura" ayuda="Si la deja vacía, se toma la de hoy." error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} type="date" max={hoy} />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

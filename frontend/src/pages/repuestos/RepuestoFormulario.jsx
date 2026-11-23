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
import { Input } from '@/components/ui/input'
import useCarga from '@/hooks/useCarga'
import { numero, texto } from '@/schemas/comunes'
import { actualizarRepuesto, crearRepuesto, obtenerRepuesto } from '@/services/repuestoService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'
import { formatNumero } from '@/utils/format'

// Mismas reglas que RepuestoCreateDto. El stock inicial solo se indica al crear: después sube con las
// entradas y baja con el consumo en órdenes (requerimientos 7.4), así que la edición no lo toca.
const base = {
  codigo: texto('el código', 20),
  nombre: texto('el nombre', 100),
  unidadMedida: texto('la unidad de medida', 20),
  stockMinimo: numero('el stock mínimo', { min: 0 }),
  costoUnitario: numero('el costo unitario', { min: 0.01, entero: false }),
}
const esquemaNuevo = z.object({ ...base, stockActual: numero('el stock inicial', { min: 0 }) })
const esquemaEdicion = z.object(base)
const sinRepuesto = () => Promise.resolve(null)

// Formulario del repuesto: nuevo (/repuestos/nuevo) o edición (/repuestos/:id/editar). Solo Administrador.
export default function RepuestoFormulario() {
  const { id } = useParams()
  const cargar = useCallback(() => (id ? obtenerRepuesto(id) : sinRepuesto()), [id])
  const repuesto = useCarga(cargar)
  const titulo = id ? 'Editar repuesto' : 'Nuevo repuesto'

  if (repuesto.error?.status === 404) return <PaginaError codigo={404} />
  if (repuesto.cargando) return <><EncabezadoPagina titulo={titulo} /><IndicadorCarga forma="detalle" /></>
  if (repuesto.error) return <><EncabezadoPagina titulo={titulo} /><EstadoError error={repuesto.error} onReintentar={repuesto.recargar} /></>
  return <Contenido repuesto={repuesto.datos?.data} titulo={titulo} />
}

function Contenido({ repuesto, titulo }) {
  const navigate = useNavigate()
  const { state } = useLocation()
  const regreso = repuesto ? `/repuestos/${repuesto.idRepuesto}` : (state?.desde ?? '/repuestos')
  const esquema = repuesto ? esquemaEdicion : esquemaNuevo
  const campos = Object.keys(esquema.shape)
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    defaultValues: {
      codigo: repuesto?.codigo ?? '',
      nombre: repuesto?.nombre ?? '',
      unidadMedida: repuesto?.unidadMedida ?? '',
      stockMinimo: repuesto ? String(repuesto.stockMinimo) : '',
      costoUnitario: repuesto ? String(repuesto.costoUnitario) : '',
      ...(repuesto ? {} : { stockActual: '0' }),
    },
  })

  const enviar = async (datos) => {
    try {
      const { data } = repuesto ? await actualizarRepuesto(repuesto.idRepuesto, datos) : await crearRepuesto(datos)
      avisarExito(`Repuesto ${datos.codigo} ${repuesto ? 'actualizado' : 'registrado'}`)
      navigate(repuesto ? regreso : `/repuestos/${data?.idRepuesto ?? ''}`, { state })
    } catch (err) {
      // 409: el código ya está registrado
      if (err.status === 409) setError('codigo', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, campos)) avisarError(err)
    }
  }

  const campo = (name, etiqueta, opciones = {}) => (
    <Controller name={name} control={control} render={({ field, fieldState }) => (
      <CampoFormulario id={name} etiqueta={etiqueta} requerido ayuda={opciones.ayuda} unidad={opciones.unidad} error={fieldState.error?.message}>
        {(props) => <Input {...field} {...props} inputMode={opciones.inputMode} autoComplete="off" spellCheck={opciones.codigo ? false : undefined} />}
      </CampoFormulario>
    )} />
  )

  return (
    <>
      <EncabezadoPagina titulo={titulo} pestana={repuesto ? `Editar ${repuesto.codigo}` : titulo} volver={{ to: regreso, texto: repuesto ? 'Volver al repuesto' : 'Volver a Repuestos' }} />
      <p className="-mt-2 text-sm text-muted-foreground">Los campos con * son obligatorios.</p>
      <Formulario onSubmit={handleSubmit(enviar)}>
        <BloqueFormulario titulo="Datos generales">
          {campo('codigo', 'Código', { codigo: true, ayuda: 'Por ejemplo FIL-ACE-001.' })}
          {campo('nombre', 'Nombre')}
          {campo('unidadMedida', 'Unidad de medida', { ayuda: 'Por ejemplo unidad, juego o galón.' })}
          {campo('costoUnitario', 'Costo unitario (₡)', { inputMode: 'decimal', ayuda: 'Se copia a cada orden en el momento del uso.' })}
        </BloqueFormulario>
        <BloqueFormulario titulo="Inventario" detalle={repuesto
          ? `Stock actual: ${formatNumero(repuesto.stockActual)} ${repuesto.unidadMedida}. Para sumar stock, use "Registrar entrada" en el detalle.`
          : 'Después del registro, el stock solo cambia con entradas y con el consumo en las órdenes.'}>
          {!repuesto && campo('stockActual', 'Stock inicial', { inputMode: 'numeric' })}
          {campo('stockMinimo', 'Stock mínimo', { inputMode: 'numeric', ayuda: 'Debajo de este número el repuesto aparece como alerta.' })}
        </BloqueFormulario>
        <AccionesFormulario procesando={isSubmitting} textoEnviar={repuesto ? 'Guardar cambios' : 'Registrar repuesto'} onCancelar={() => navigate(regreso, { state })} />
      </Formulario>
    </>
  )
}

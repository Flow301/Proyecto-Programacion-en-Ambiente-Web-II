import { useCallback } from 'react'
import { useNavigate, useParams } from 'react-router'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import CampoFormulario from '@/components/shared/CampoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import { AccionesFormulario, BloqueFormulario, Formulario } from '@/components/shared/Formulario'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import ListaOpciones from '@/components/shared/ListaOpciones'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import useCarga from '@/hooks/useCarga'
import { eleccion, numero, texto, textoOpcional } from '@/schemas/comunes'
import { variablesControl } from '@/services/catalogoService'
import { actualizarCategoria, crearCategoria, obtenerCategoria } from '@/services/categoriaService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'

// Mismas reglas que CategoriaActivoCreateDto
const esquema = z.object({
  nombre: texto('el nombre', 60),
  descripcion: textoOpcional(250),
  prefijo: z.string().trim().regex(/^[A-Za-z]{3}$/, 'Use tres letras, por ejemplo PKP.').transform((v) => v.toUpperCase()),
  idVariableControl: eleccion('la variable de control'),
  intervaloKm: numero('el umbral', { min: 1 }),
  porcentajeAviso: numero('el porcentaje de aviso', { min: 1, max: 99 }),
})
const CAMPOS = Object.keys(esquema.shape)

const sinCategoria = () => Promise.resolve(null)

// Formulario de categoría: nueva (/categorias/nueva) o edición (/categorias/:id/editar). Solo Administrador.
export default function CategoriaFormulario() {
  const { id } = useParams()
  const cargarCategoria = useCallback(() => (id ? obtenerCategoria(id) : sinCategoria()), [id])
  const categoria = useCarga(cargarCategoria)
  const variables = useCarga(variablesControl)

  const titulo = id ? 'Editar categoría' : 'Nueva categoría'
  const volver = { to: '/categorias', texto: 'Volver a Categorías' }

  if (categoria.cargando || variables.cargando) return <><EncabezadoPagina titulo={titulo} volver={volver} /><IndicadorCarga forma="detalle" /></>
  const error = categoria.error ?? variables.error
  if (error) return <><EncabezadoPagina titulo={titulo} volver={volver} /><EstadoError error={error} onReintentar={() => { categoria.recargar(); variables.recargar() }} /></>

  return <Contenido categoria={categoria.datos?.data} variables={variables.datos.data} titulo={titulo} volver={volver} />
}

function Contenido({ categoria, variables, titulo, volver }) {
  const navigate = useNavigate()
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    defaultValues: {
      nombre: categoria?.nombre ?? '',
      descripcion: categoria?.descripcion ?? '',
      prefijo: categoria?.prefijo ?? '',
      // con una sola variable (Kilometraje), viene elegida
      idVariableControl: String(categoria?.idVariableControl ?? (variables.length === 1 ? variables[0].idVariableControl : '')),
      intervaloKm: categoria ? String(categoria.intervaloKm) : '',
      porcentajeAviso: categoria ? String(categoria.porcentajeAviso) : '90',
    },
  })
  const idVariable = useWatch({ control, name: 'idVariableControl' })
  const unidad = variables.find((v) => String(v.idVariableControl) === idVariable)?.unidad

  const enviar = async (datos) => {
    try {
      if (categoria) await actualizarCategoria(categoria.idCategoria, datos)
      else await crearCategoria(datos)
      avisarExito(`Categoría ${datos.nombre} ${categoria ? 'actualizada' : 'registrada'}`)
      navigate('/categorias')
    } catch (err) {
      // 409: nombre o prefijo repetidos, o prefijo que no se puede cambiar porque ya hay vehículos
      if (err.status === 409) setError(/prefijo/i.test(err.message) ? 'prefijo' : 'nombre', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, CAMPOS)) avisarError(err)
    }
  }

  const campo = (name, etiqueta, opciones = {}) => (
    <Controller name={name} control={control} render={({ field, fieldState }) => (
      <CampoFormulario id={name} etiqueta={etiqueta} requerido={opciones.requerido} ayuda={opciones.ayuda} unidad={opciones.unidad} error={fieldState.error?.message} className={opciones.className}>
        {(props) => opciones.render ? opciones.render(field, props) : <Input {...field} {...props} autoComplete="off" inputMode={opciones.inputMode} />}
      </CampoFormulario>
    )} />
  )

  return (
    <>
      <EncabezadoPagina titulo={titulo} volver={volver} pestana={categoria ? `Editar ${categoria.nombre}` : titulo} />
      <p className="-mt-2 text-sm text-muted-foreground">Los campos con * son obligatorios.</p>
      <Formulario onSubmit={handleSubmit(enviar)}>
        <BloqueFormulario titulo="Datos generales">
          {campo('nombre', 'Nombre', { requerido: true })}
          {campo('prefijo', 'Prefijo del código', {
            requerido: true,
            ayuda: categoria?.cantidadActivos
              ? 'No se puede cambiar: la categoría ya tiene vehículos con códigos generados.'
              : 'Tres letras. Los vehículos se codifican con él: PKP-0001.',
            render: (field, props) => (
              <Input {...field} {...props} maxLength={3} autoComplete="off" spellCheck={false}
                readOnly={Boolean(categoria?.cantidadActivos)} className={categoria?.cantidadActivos ? 'bg-muted uppercase' : 'uppercase'} />
            ),
          })}
          {campo('descripcion', 'Descripción', { className: 'md:col-span-2', render: (field, props) => <Textarea {...field} {...props} rows={2} /> })}
        </BloqueFormulario>
        <BloqueFormulario titulo="Regla de mantenimiento preventivo" detalle="El sistema avisa cuando el uso acumulado de un vehículo llega al porcentaje de aviso del umbral." columnas={3}>
          {campo('idVariableControl', 'Variable de control', {
            requerido: true,
            render: (field, props) => (
              <ListaOpciones {...props} value={field.value} onChange={field.onChange}
                opciones={variables.map((v) => ({ valor: v.idVariableControl, texto: `${v.nombre} (${v.unidad})` }))} />
            ),
          })}
          {campo('intervaloKm', 'Umbral', { requerido: true, unidad, inputMode: 'numeric', ayuda: 'Cada cuánto toca el preventivo.' })}
          {campo('porcentajeAviso', 'Aviso', { requerido: true, unidad: '%', inputMode: 'numeric', ayuda: 'De 1 a 99. Desde ahí el vehículo queda Próximo.' })}
        </BloqueFormulario>
        <AccionesFormulario procesando={isSubmitting} textoEnviar={categoria ? 'Guardar cambios' : 'Registrar categoría'} onCancelar={() => navigate('/categorias')} />
      </Formulario>
    </>
  )
}

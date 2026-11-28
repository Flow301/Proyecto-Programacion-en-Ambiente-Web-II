import { useCallback, useEffect, useId, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { CircleAlert, ImageUp, X } from 'lucide-react'
import CampoFormulario from '@/components/shared/CampoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import { AccionesFormulario, BloqueFormulario, Formulario } from '@/components/shared/Formulario'
import ImagenVehiculo from '@/components/shared/ImagenVehiculo'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import ListaOpciones from '@/components/shared/ListaOpciones'
import PaginaError from '@/components/shared/PaginaError'
import Separador from '@/components/shared/Separador'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import useCarga from '@/hooks/useCarga'
import { eleccion, numero, texto, textoOpcional } from '@/schemas/comunes'
import { actualizarActivo, crearActivo, obtenerActivo, subirImagenActivo } from '@/services/activoService'
import { listarCategorias } from '@/services/categoriaService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'
import { aFechaIso, formatKm } from '@/utils/format'

// Mismas reglas que ActivoCreateDto y ActivoUpdateDto. Los kilometrajes solo se indican al registrar: después el
// actual cambia con "Registrar lectura de uso" y el del último preventivo al completar una orden preventiva.
const hoy = () => aFechaIso()

// Número entero opcional: vacío se envía como null (intervalo propio: sin él, se usa el de la categoría)
const enteroOpcional = (min) =>
  z.string().trim()
    .refine((v) => v === '' || /^\d+$/.test(v), 'Use solo números enteros.')
    .transform((v) => (v === '' ? null : Number(v)))
    .refine((v) => v === null || v >= min, `El valor mínimo es ${min}.`)

const base = {
  nombre: texto('el nombre', 100),
  idCategoria: eleccion('la categoría'),
  descripcion: textoOpcional(250),
  fechaAdquisicion: z.string().min(1, 'Ingrese la fecha de adquisición.').refine((f) => f <= hoy(), 'La fecha no puede ser futura.'),
  valorReferencia: numero('el valor de referencia', { min: 0, entero: false }),
  // Letras y números con guiones simples entre grupos (CL-284731, C-158327, BKL-482)
  placa: texto('la placa', 10)
    .regex(/^[A-Za-z0-9]+(-[A-Za-z0-9]+)*$/, 'Use solo letras, números y guiones, por ejemplo CL-284731.')
    .transform((v) => v.toUpperCase()),
  marca: texto('la marca', 40),
  modelo: texto('el modelo', 40),
  anio: numero('el año de fabricación', { min: 1950, max: new Date().getFullYear() }),
  intervaloKmPropio: enteroOpcional(1),
}
// La empresa no puede comprar un vehículo antes de que se fabrique (la API valida lo mismo en el DTO)
const fechaDespuesDeFabricacion = (d, ctx) => {
  if (Number(d.fechaAdquisicion.slice(0, 4)) < d.anio) {
    ctx.addIssue({ code: 'custom', path: ['fechaAdquisicion'], message: `La fecha de adquisición no puede ser anterior al año de fabricación (${d.anio}).` })
  }
}
const esquemaNuevo = z.object({
  ...base,
  kilometrajeActual: numero('el kilometraje actual', { min: 0 }),
  kilometrajeUltimoPreventivo: numero('el kilometraje del último preventivo', { min: 0 }),
}).refine((d) => d.kilometrajeUltimoPreventivo <= d.kilometrajeActual, {
  path: ['kilometrajeUltimoPreventivo'],
  message: 'No puede ser mayor que el kilometraje actual.',
}).superRefine(fechaDespuesDeFabricacion)
const esquemaEdicion = z.object(base).superRefine(fechaDespuesDeFabricacion)
const CAMPOS = [...Object.keys(base), 'kilometrajeActual', 'kilometrajeUltimoPreventivo']

const sinVehiculo = () => Promise.resolve(null)

// Las mismas reglas de la API para la imagen (PUT api/Activo/{id}/imagen): JPG, PNG o WebP y máximo 2 MB.
// La API además reconoce el tipo por el contenido del archivo, no solo por su nombre.
const TIPOS_IMAGEN = ['image/jpeg', 'image/png', 'image/webp']
const MAX_IMAGEN = 2 * 1024 * 1024
const marcaImagen = () => Date.now()

// Formulario del vehículo: nuevo (/vehiculos/nuevo) o edición (/vehiculos/:id/editar). Solo Administrador.
// El código lo genera la API con el prefijo de la categoría; el estado no se edita aquí (tiene sus acciones).
export default function VehiculoFormulario() {
  const { id } = useParams()
  const cargar = useCallback(() => Promise.all([id ? obtenerActivo(id) : sinVehiculo(), listarCategorias()]), [id])
  const carga = useCarga(cargar)
  const titulo = id ? 'Editar vehículo' : 'Nuevo vehículo'

  if (carga.error?.status === 404) return <PaginaError codigo={404} />
  if (carga.cargando) return <><EncabezadoPagina titulo={titulo} /><IndicadorCarga forma="detalle" /></>
  if (carga.error) return <><EncabezadoPagina titulo={titulo} /><EstadoError error={carga.error} onReintentar={carga.recargar} /></>
  const [vehiculo, categorias] = carga.datos
  return <Contenido vehiculo={vehiculo?.data} categorias={categorias.data} titulo={titulo} />
}

function Contenido({ vehiculo, categorias, titulo }) {
  const navigate = useNavigate()
  const { state } = useLocation()
  const regreso = vehiculo ? `/vehiculos/${vehiculo.idActivo}` : (state?.desde ?? '/vehiculos')
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(vehiculo ? esquemaEdicion : esquemaNuevo),
    defaultValues: {
      nombre: vehiculo?.nombre ?? '',
      idCategoria: vehiculo ? String(vehiculo.idCategoria) : '',
      descripcion: vehiculo?.descripcion ?? '',
      fechaAdquisicion: vehiculo ? aFechaIso(vehiculo.fechaAdquisicion) : '',
      valorReferencia: vehiculo ? String(vehiculo.valorReferencia) : '',
      placa: vehiculo?.placa ?? '',
      marca: vehiculo?.marca ?? '',
      modelo: vehiculo?.modelo ?? '',
      anio: vehiculo ? String(vehiculo.anio) : '',
      intervaloKmPropio: vehiculo?.intervaloKmPropio == null ? '' : String(vehiculo.intervaloKmPropio),
      ...(vehiculo ? {} : { kilometrajeActual: '', kilometrajeUltimoPreventivo: '' }),
    },
  })
  const idCategoria = useWatch({ control, name: 'idCategoria' })
  const categoria = categorias.find((c) => String(c.idCategoria) === idCategoria)
  const [imagen, setImagen] = useState({ archivo: null, url: null, error: null })

  const enviar = async (datos) => {
    if (imagen.error) return
    try {
      const { data } = vehiculo ? await actualizarActivo(vehiculo.idActivo, datos) : await crearActivo(datos)
      const siguiente = vehiculo ? { ...state } : { desde: regreso }
      avisarExito(`Vehículo ${data.codigo} ${vehiculo ? 'actualizado' : 'registrado'}`)
      // La imagen va después, en su propio endpoint: los datos ya quedaron guardados aunque la imagen falle
      if (imagen.archivo) {
        try {
          await subirImagenActivo(data.idActivo, imagen.archivo)
          siguiente.imagen = marcaImagen() // el archivo nuevo puede tener la misma ruta: se pide de nuevo, no del caché
        } catch (err) {
          avisarError({ ...err, message: `Los datos se guardaron, pero la imagen no: ${err.message}` })
        }
      }
      navigate(`/vehiculos/${data.idActivo}`, { state: siguiente })
    } catch (err) {
      // 409: la placa ya es de otro vehículo
      if (err.status === 409 && /placa/i.test(err.message)) setError('placa', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, CAMPOS)) avisarError(err)
    }
  }

  const campo = (name, etiqueta, opciones = {}) => (
    <Controller name={name} control={control} render={({ field, fieldState }) => (
      <CampoFormulario id={name} etiqueta={etiqueta} requerido={!opciones.opcional} ayuda={opciones.ayuda} unidad={opciones.unidad} error={fieldState.error?.message} className={opciones.className}>
        {(props) => <Input {...field} {...props} type={opciones.type} max={opciones.max} inputMode={opciones.inputMode} autoComplete="off" spellCheck={opciones.codigo ? false : undefined} />}
      </CampoFormulario>
    )} />
  )

  return (
    <>
      <EncabezadoPagina
        titulo={titulo}
        pestana={vehiculo ? `Editar ${vehiculo.codigo}` : titulo}
        detalle={vehiculo && <><strong>{vehiculo.codigo}</strong> <Separador /> {vehiculo.nombre}</>}
        volver={{ to: regreso, texto: vehiculo ? 'Volver al vehículo' : 'Volver a Vehículos' }}
      />
      <p className="-mt-2 text-sm text-muted-foreground">Los campos con * son obligatorios.</p>
      <Formulario onSubmit={handleSubmit(enviar)}>
        <BloqueFormulario titulo="Datos generales" detalle={vehiculo ? undefined : 'El código se genera con el prefijo de la categoría (por ejemplo PKP-0004) y el vehículo nace Operativo.'}>
          {campo('nombre', 'Nombre', { ayuda: 'Para qué se usa: "Pickup de supervisión de rutas".' })}
          <Controller name="idCategoria" control={control} render={({ field, fieldState }) => (
            <CampoFormulario id="idCategoria" etiqueta="Categoría" requerido error={fieldState.error?.message}
              ayuda={categoria && `Preventivo cada ${formatKm(categoria.intervaloKm, categoria.variableControlUnidad)}.`}>
              {(props) => <ListaOpciones {...props} ref={field.ref} value={field.value} onChange={field.onChange} placeholder="Elija la categoría"
                opciones={categorias.map((c) => ({ valor: c.idCategoria, texto: c.nombre }))} />}
            </CampoFormulario>
          )} />
          <Controller name="descripcion" control={control} render={({ field, fieldState }) => (
            <CampoFormulario id="descripcion" etiqueta="Descripción" error={fieldState.error?.message} className="md:col-span-2">
              {(props) => <Textarea {...field} {...props} rows={2} maxLength={250} />}
            </CampoFormulario>
          )} />
          {campo('fechaAdquisicion', 'Fecha de adquisición', { type: 'date', max: hoy() })}
          {campo('valorReferencia', 'Valor de referencia (₡)', { inputMode: 'decimal' })}
        </BloqueFormulario>

        <BloqueFormulario titulo="Datos del vehículo" detalle="Los datos propios de una unidad de la flota.">
          {campo('placa', 'Placa', { codigo: true, ayuda: 'No se repite entre vehículos.' })}
          {campo('anio', 'Año de fabricación', { inputMode: 'numeric', ayuda: 'El año en que se fabricó el vehículo.' })}
          {campo('marca', 'Marca')}
          {campo('modelo', 'Modelo')}
          {!vehiculo && campo('kilometrajeActual', 'Kilometraje actual', { inputMode: 'numeric', unidad: 'km', ayuda: 'Después cambia con "Registrar lectura de uso".' })}
          {!vehiculo && campo('kilometrajeUltimoPreventivo', 'Kilometraje del último preventivo', { inputMode: 'numeric', unidad: 'km', ayuda: 'Desde aquí se cuenta el uso para el próximo preventivo.' })}
          {campo('intervaloKmPropio', 'Intervalo de preventivo propio', {
            opcional: true, inputMode: 'numeric', unidad: 'km',
            ayuda: categoria ? `Vacío: usa el de la categoría (${formatKm(categoria.intervaloKm, categoria.variableControlUnidad)}).` : 'Vacío: usa el de la categoría.',
          })}
        </BloqueFormulario>

        <BloqueFormulario titulo="Imagen" detalle="Una foto de la unidad para reconocerla en el listado. JPG, PNG o WebP, máximo 2 MB." columnas={1}>
          <SelectorImagen actual={vehiculo?.imagen} alt={vehiculo ? `${vehiculo.codigo} · ${vehiculo.nombre}` : 'Vehículo nuevo'} valor={imagen} onCambiar={setImagen} />
        </BloqueFormulario>

        <AccionesFormulario procesando={isSubmitting} textoEnviar={vehiculo ? 'Guardar cambios' : 'Registrar vehículo'} onCancelar={() => navigate(regreso, { state })} />
      </Formulario>
    </>
  )
}

// Selector de archivo con vista previa: se ve la foto elegida antes de guardar. La validación de tipo y tamaño
// avisa de inmediato; la API vuelve a validar al recibirla.
function SelectorImagen({ actual, alt, valor, onCambiar }) {
  const id = useId()

  // La vista previa es una URL local del archivo elegido; se libera al elegir otro, al quitarlo o al salir
  useEffect(() => () => {
    if (valor.url) URL.revokeObjectURL(valor.url)
  }, [valor.url])

  const elegir = (e) => {
    const archivo = e.target.files?.[0]
    e.target.value = '' // permite volver a elegir el mismo archivo
    if (!archivo) return
    if (!TIPOS_IMAGEN.includes(archivo.type)) onCambiar({ archivo: null, url: null, error: 'El archivo debe ser una imagen JPG, PNG o WebP.' })
    else if (archivo.size > MAX_IMAGEN) onCambiar({ archivo: null, url: null, error: 'La imagen pesa más de 2 MB. Elija una más liviana.' })
    else onCambiar({ archivo, url: URL.createObjectURL(archivo), error: null })
  }
  const quitar = () => onCambiar({ archivo: null, url: null, error: null })

  return (
    <div className="flex flex-wrap items-start gap-5">
      <ImagenVehiculo ruta={valor.url ?? actual} alt={alt} className="h-36 w-52" />
      <div className="flex min-w-0 flex-col gap-2">
        <div className="flex flex-wrap gap-2">
          {/* El control real es el input de archivo, oculto dentro de la etiqueta: con teclado recibe el foco y el
              anillo de foco se ve en el botón */}
          <Button asChild variant="outline">
            <label className="cursor-pointer focus-within:border-ring focus-within:ring-[3px] focus-within:ring-ring/50">
              <ImageUp aria-hidden="true" />
              {actual || valor.archivo ? 'Cambiar imagen' : 'Elegir imagen'}
              <input id={id} type="file" accept={TIPOS_IMAGEN.join(',')} className="sr-only" onChange={elegir}
                aria-describedby={valor.error ? `${id}-error` : undefined} aria-invalid={valor.error ? true : undefined} />
            </label>
          </Button>
          {valor.archivo && <Button type="button" variant="ghost" onClick={quitar}><X aria-hidden="true" />Quitar selección</Button>}
        </div>
        <p className="text-sm text-muted-foreground">
          {valor.archivo ? `${valor.archivo.name}: se sube al guardar.` : actual ? 'Imagen actual.' : 'Sin imagen todavía.'}
        </p>
        {valor.error && (
          <p id={`${id}-error`} className="flex items-start gap-1.5 text-sm font-semibold text-alto-texto">
            <CircleAlert className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
            {valor.error}
          </p>
        )}
      </div>
    </div>
  )
}

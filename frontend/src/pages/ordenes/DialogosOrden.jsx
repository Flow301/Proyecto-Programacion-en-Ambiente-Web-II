import { useCallback, useState } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import EstadoError from '@/components/shared/EstadoError'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import ListaOpciones from '@/components/shared/ListaOpciones'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import useCarga from '@/hooks/useCarga'
import { eleccion, numero, texto, textoOpcional } from '@/schemas/comunes'
import { obtenerActivo } from '@/services/activoService'
import { prioridades, tiposMantenimiento } from '@/services/catalogoService'
import {
  aprobarOrden, asignarTecnicos, diagnosticarOrden, editarOrden, iniciarOrden, registrarHoras, registrarRepuestos,
} from '@/services/ordenService'
import { listarRepuestos } from '@/services/repuestoService'
import { tecnicosDisponibles } from '@/services/tecnicoService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'
import { aFechaIso, formatColones, formatKm, formatNumero } from '@/utils/format'

// Ventanas de las acciones de la orden que piden datos (enunciado Avance 2, 5.4). Las que solo confirman
// (rechazar, reanudar, completar, cancelar) usan DialogoConfirmacion en el detalle. La API valida cada regla;
// cada ventana recibe la orden y avisa con onHecho() para que el detalle se recargue.

// Envía y traduce los errores de la API a los campos; el resto va a un aviso
function useEnvio(setError, campos, accion, onHecho) {
  return async (datos) => {
    try {
      const respuesta = await accion(datos)
      onHecho(respuesta)
    } catch (err) {
      if (!aplicarErroresApi(err, setError, campos)) avisarError(err)
    }
  }
}

const observacionCampo = (control) => (
  <Controller name="observacion" control={control} render={({ field, fieldState }) => (
    <CampoFormulario id="observacion" etiqueta="Observación (opcional)" ayuda="Queda en el historial de la orden." error={fieldState.error?.message}>
      {(props) => <Textarea {...field} {...props} rows={2} maxLength={500} />}
    </CampoFormulario>
  )} />
)

const cargarDiagnostico = (idActivo) => Promise.all([obtenerActivo(idActivo), prioridades(), tiposMantenimiento()])
const esquemaDiagnostico = z.object({
  diagnostico: texto('el diagnóstico', 1000),
  idPrioridad: eleccion('la prioridad'),
  kilometrajeDiagnostico: numero('el kilometraje', { min: 0 }),
  idTipoMantenimiento: eleccion('el tipo de mantenimiento'),
  observacion: textoOpcional(500),
})

export function DialogoDiagnostico({ orden, onCerrar, onHecho }) {
  const cargar = useCallback(() => cargarDiagnostico(orden.idActivo), [orden.idActivo])
  const catalogos = useCarga(cargar)
  const [activo, niveles, tipos] = catalogos.datos?.map((r) => r.data) ?? [null, [], []]
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaDiagnostico),
    values: {
      diagnostico: orden.diagnostico ?? '',
      idPrioridad: orden.idPrioridad ? String(orden.idPrioridad) : '',
      kilometrajeDiagnostico: activo ? String(activo.kilometrajeActual) : '',
      idTipoMantenimiento: String(orden.idTipoMantenimiento),
      observacion: '',
    },
    resetOptions: { keepDirtyValues: true },
  })
  const enviar = useEnvio(setError, Object.keys(esquemaDiagnostico.shape),
    (datos) => diagnosticarOrden(orden.idOrden, datos),
    () => { avisarExito(`Orden ${orden.codigo} diagnosticada`); onHecho() })

  return (
    <DialogoFormulario abierto titulo={`Diagnosticar ${orden.codigo}`} descripcion={`${orden.activoCodigo} · ${orden.activoNombre}`}
      textoEnviar="Registrar diagnóstico" procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      {catalogos.error && <EstadoError error={catalogos.error} />}
      <Controller name="diagnostico" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="diagnostico" etiqueta="Diagnóstico" requerido error={fieldState.error?.message}>
          {(props) => <Textarea {...field} {...props} rows={4} maxLength={1000} />}
        </CampoFormulario>
      )} />
      <div className="grid gap-4 sm:grid-cols-2">
        <Controller name="kilometrajeDiagnostico" control={control} render={({ field, fieldState }) => (
          <CampoFormulario id="kilometrajeDiagnostico" etiqueta="Kilometraje" requerido unidad="km" error={fieldState.error?.message}
            ayuda={activo ? `Última lectura: ${formatKm(activo.kilometrajeActual)}. No puede ser menor.` : undefined}>
            {(props) => <Input {...field} {...props} inputMode="numeric" autoComplete="off" />}
          </CampoFormulario>
        )} />
        <Controller name="idPrioridad" control={control} render={({ field, fieldState }) => (
          <CampoFormulario id="idPrioridad" etiqueta="Prioridad" requerido error={fieldState.error?.message}>
            {(props) => <ListaOpciones {...props} value={field.value} onChange={field.onChange} opciones={niveles.map((p) => ({ valor: p.idPrioridad, texto: p.nombre }))} />}
          </CampoFormulario>
        )} />
      </div>
      <Controller name="idTipoMantenimiento" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idTipoMantenimiento" etiqueta="Tipo de mantenimiento" requerido ayuda="Corríjalo si el diagnóstico muestra otro tipo de trabajo." error={fieldState.error?.message}>
          {(props) => <ListaOpciones {...props} value={field.value} onChange={field.onChange} opciones={tipos.map((t) => ({ valor: t.idTipoMantenimiento, texto: `${t.nombre} (${t.naturalezaNombre})` }))} />}
        </CampoFormulario>
      )} />
      {observacionCampo(control)}
    </DialogoFormulario>
  )
}

const esquemaAprobacion = z.object({
  fechaProgramada: z.string().min(1, 'Elija la fecha programada.').refine((v) => v >= aFechaIso(), 'La fecha no puede ser anterior a hoy.'),
  observacion: textoOpcional(500),
})

export function DialogoAprobacion({ orden, onCerrar, onHecho }) {
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaAprobacion),
    defaultValues: { fechaProgramada: aFechaIso(), observacion: '' },
  })
  const enviar = useEnvio(setError, ['fechaProgramada', 'observacion'],
    (datos) => aprobarOrden(orden.idOrden, datos),
    () => { avisarExito(`Orden ${orden.codigo} aprobada`); onHecho() })
  return (
    <DialogoFormulario abierto titulo={`Aprobar ${orden.codigo}`} descripcion="La orden queda lista para que el taller la inicie en la fecha programada."
      textoEnviar="Aprobar" procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      <Controller name="fechaProgramada" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="fechaProgramada" etiqueta="Fecha programada" requerido error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} type="date" min={aFechaIso()} className="w-48" />}
        </CampoFormulario>
      )} />
      {observacionCampo(control)}
    </DialogoFormulario>
  )
}

// Iniciar (asigna los primeros técnicos) o agregar técnicos a una orden en ejecución.
// Solo se ofrecen los elegibles: activos, disponibles y con la especialidad que pide el tipo (lo decide la API).
const cargarElegibles = async (idTipoMantenimiento) => {
  const { data: tipos } = await tiposMantenimiento()
  const tipo = tipos.find((t) => t.idTipoMantenimiento === idTipoMantenimiento)
  return tipo ? tecnicosDisponibles(tipo.idEspecialidadRequerida) : { data: [] }
}

export function DialogoTecnicos({ orden, iniciar, onCerrar, onHecho }) {
  const cargar = useCallback(() => cargarElegibles(orden.idTipoMantenimiento), [orden.idTipoMantenimiento])
  const elegibles = useCarga(cargar)
  const asignados = new Set((orden.tecnicos ?? []).map((t) => t.idTecnico))
  const opciones = (elegibles.datos?.data ?? []).filter((t) => !asignados.has(t.idTecnico))
  const [elegidos, setElegidos] = useState([])
  const [observacion, setObservacion] = useState('')
  const [error, setErrorLocal] = useState('')
  const [procesando, setProcesando] = useState(false)

  const alternar = (id, marcado) => {
    setErrorLocal('')
    setElegidos((previos) => (marcado ? [...previos, id] : previos.filter((e) => e !== id)))
  }

  const enviar = async (e) => {
    e.preventDefault()
    if (elegidos.length === 0) { setErrorLocal('Elija al menos un técnico.'); return }
    if (/[<>]/.test(observacion)) { setErrorLocal('No use los signos < ni > en la observación.'); return }
    setProcesando(true)
    try {
      if (iniciar) await iniciarOrden(orden.idOrden, elegidos, observacion.trim() || null)
      else await asignarTecnicos(orden.idOrden, elegidos)
      avisarExito(iniciar ? `Orden ${orden.codigo} en ejecución` : `${elegidos.length === 1 ? 'Técnico asignado' : 'Técnicos asignados'} a ${orden.codigo}`)
      onHecho()
    } catch (err) {
      avisarError(err)
    } finally {
      setProcesando(false)
    }
  }

  return (
    <DialogoFormulario abierto titulo={iniciar ? `Iniciar ${orden.codigo}` : `Asignar técnicos a ${orden.codigo}`}
      descripcion={`Técnicos disponibles con la especialidad ${orden.tipoMantenimientoEspecialidadRequeridaNombre}.`}
      textoEnviar={iniciar ? 'Iniciar orden' : 'Asignar'} procesando={procesando} onEnviar={enviar} onCerrar={onCerrar}>
      {elegibles.cargando && <IndicadorCarga forma="bloque" />}
      {elegibles.error && <EstadoError error={elegibles.error} onReintentar={elegibles.recargar} />}
      {elegibles.datos && (
        <fieldset aria-describedby={error ? 'tecnicos-error' : undefined}>
          <legend className="mb-2 text-sm font-semibold">Técnicos <span aria-hidden="true">*</span></legend>
          {opciones.length === 0
            ? <p className="text-sm text-muted-foreground">No hay técnicos disponibles con esa especialidad que no estén ya en la orden.</p>
            : (
              <ul className="flex flex-col divide-y divide-border-suave rounded-sm border">
                {opciones.map((t) => (
                  <li key={t.idTecnico}>
                    <label className="flex cursor-pointer items-center gap-3 px-4 py-3 hover:bg-muted/40">
                      <Checkbox checked={elegidos.includes(t.idTecnico)} onCheckedChange={(v) => alternar(t.idTecnico, v === true)} />
                      <span className="flex min-w-0 flex-1 flex-col">
                        <strong className="truncate">{t.tecnicoNombreCompleto}</strong>
                        <span className="text-sm text-muted-foreground">{t.nivelCertificacionNombre} · {formatColones(t.tecnicoTarifaHora)} por hora</span>
                      </span>
                    </label>
                  </li>
                ))}
              </ul>
            )}
          {error && <p id="tecnicos-error" className="mt-2 text-sm font-semibold text-destructive">{error}</p>}
        </fieldset>
      )}
      {iniciar && (
        <CampoFormulario id="observacion" etiqueta="Observación (opcional)" ayuda="Queda en el historial de la orden.">
          {(props) => <Textarea {...props} value={observacion} onChange={(e) => setObservacion(e.target.value)} rows={2} maxLength={500} />}
        </CampoFormulario>
      )}
    </DialogoFormulario>
  )
}

const esquemaHoras = z.object({ horas: numero('las horas', { min: 0.01, max: 24, entero: false }) })

export function DialogoHoras({ orden, tecnico, onCerrar, onHecho }) {
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({ resolver: zodResolver(esquemaHoras), defaultValues: { horas: '' } })
  const enviar = useEnvio(setError, ['horas'],
    ({ horas }) => registrarHoras(orden.idOrden, tecnico.idTecnico, horas),
    () => { avisarExito(`Horas de ${tecnico.tecnicoNombreCompleto} registradas`); onHecho() })
  return (
    <DialogoFormulario abierto titulo={`Registrar horas de ${tecnico.tecnicoNombreCompleto}`}
      descripcion="Las horas se suman a las que ya tiene en esta orden. Cada registro va de más de 0 a 24 horas."
      textoEnviar="Registrar horas" procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      <Controller name="horas" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="horas" etiqueta="Horas trabajadas" requerido unidad="h" error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} inputMode="decimal" autoComplete="off" className="w-40" />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

const esquemaRepuesto = z.object({ idRepuesto: eleccion('el repuesto'), cantidad: numero('la cantidad', { min: 1 }) })

export function DialogoRepuesto({ orden, onCerrar, onHecho }) {
  const repuestos = useCarga(listarRepuestos)
  const lista = repuestos.datos?.data ?? []
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({ resolver: zodResolver(esquemaRepuesto), defaultValues: { idRepuesto: '', cantidad: '1' } })
  const idRepuesto = useWatch({ control, name: 'idRepuesto' })
  const elegido = lista.find((r) => String(r.idRepuesto) === idRepuesto)
  const enviar = useEnvio(setError, ['idRepuesto', 'cantidad'],
    (linea) => registrarRepuestos(orden.idOrden, [linea]),
    // La API dice si alcanzó el stock o si la orden pasó a En Espera de Repuesto
    (respuesta) => { avisarExito(respuesta?.detail ?? 'Repuesto registrado'); onHecho() })
  return (
    <DialogoFormulario abierto titulo={`Registrar repuesto en ${orden.codigo}`}
      descripcion="Si el stock alcanza, se descuenta y se cobra al costo unitario de hoy. Si no alcanza, la orden pasa a En Espera de Repuesto."
      textoEnviar="Registrar repuesto" procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      {repuestos.error && <EstadoError error={repuestos.error} />}
      <Controller name="idRepuesto" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idRepuesto" etiqueta="Repuesto" requerido error={fieldState.error?.message}
          ayuda={elegido ? `Stock actual: ${formatNumero(elegido.stockActual)} ${elegido.unidadMedida} · ${formatColones(elegido.costoUnitario)} c/u` : undefined}>
          {(props) => <ListaOpciones {...props} value={field.value} onChange={field.onChange} disabled={repuestos.cargando}
            placeholder={repuestos.cargando ? 'Cargando…' : 'Elija un repuesto'}
            opciones={lista.map((r) => ({ valor: r.idRepuesto, texto: `${r.codigo} · ${r.nombre} (stock ${formatNumero(r.stockActual)})` }))} />}
        </CampoFormulario>
      )} />
      <Controller name="cantidad" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="cantidad" etiqueta="Cantidad" requerido unidad={elegido?.unidadMedida} error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} inputMode="numeric" autoComplete="off" className="w-40" />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

// Edición de la solicitud (Administrador, solo mientras está Solicitada)
const esquemaEdicion = z.object({ idTipoMantenimiento: eleccion('el tipo de mantenimiento'), motivo: texto('el motivo', 500) })

export function DialogoEdicion({ orden, onCerrar, onHecho }) {
  const tipos = useCarga(tiposMantenimiento)
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaEdicion),
    defaultValues: { idTipoMantenimiento: String(orden.idTipoMantenimiento), motivo: orden.motivo ?? '' },
  })
  const enviar = useEnvio(setError, ['idTipoMantenimiento', 'motivo'],
    (datos) => editarOrden(orden.idOrden, datos),
    () => { avisarExito(`Solicitud ${orden.codigo} actualizada`); onHecho() })
  return (
    <DialogoFormulario abierto titulo={`Editar la solicitud ${orden.codigo}`} textoEnviar="Guardar cambios"
      procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      <Controller name="idTipoMantenimiento" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idTipoMantenimiento" etiqueta="Tipo de mantenimiento" requerido error={fieldState.error?.message}>
          {(props) => <ListaOpciones {...props} value={field.value} onChange={field.onChange} disabled={tipos.cargando}
            opciones={(tipos.datos?.data ?? []).map((t) => ({ valor: t.idTipoMantenimiento, texto: `${t.nombre} (${t.naturalezaNombre})` }))} />}
        </CampoFormulario>
      )} />
      <Controller name="motivo" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="motivo" etiqueta="Motivo" requerido error={fieldState.error?.message}>
          {(props) => <Textarea {...field} {...props} rows={4} maxLength={500} />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

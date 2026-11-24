import { useLocation, useNavigate } from 'react-router'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { TriangleAlert } from 'lucide-react'
import CampoFormulario from '@/components/shared/CampoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import { AccionesFormulario, BloqueFormulario, Formulario } from '@/components/shared/Formulario'
import ListaOpciones from '@/components/shared/ListaOpciones'
import { Textarea } from '@/components/ui/textarea'
import useCarga from '@/hooks/useCarga'
import useRol from '@/hooks/useRol'
import { eleccion, texto } from '@/schemas/comunes'
import { listarTodosLosActivos } from '@/services/activoService'
import { prioridades, tiposMantenimiento } from '@/services/catalogoService'
import { crearOrden } from '@/services/ordenService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'

// Mismas reglas que OrdenMantenimientoCreateDto
const esquema = z.object({
  idActivo: eleccion('el vehículo'),
  idTipoMantenimiento: eleccion('el tipo de mantenimiento'),
  idPrioridad: eleccion('la prioridad'),
  motivo: texto('el motivo', 500),
})
const CAMPOS = Object.keys(esquema.shape)

const cargarCatalogos = () => Promise.all([listarTodosLosActivos(), tiposMantenimiento(), prioridades()])

// Nueva solicitud de mantenimiento (enunciado Avance 2, 5.3). Administrador y Solicitante/Consulta.
// La API genera el código y deja la orden en Solicitada; aquí solo se elige y se describe.
export default function OrdenNueva() {
  const navigate = useNavigate()
  const { state } = useLocation()
  const { esSolicitante } = useRol()
  const textoVolver = esSolicitante ? 'Volver a Mis solicitudes' : 'Volver a Órdenes'
  const regreso = state?.desde ?? '/ordenes'
  const catalogos = useCarga(cargarCatalogos)
  const [activos, tipos, niveles] = catalogos.datos?.map((r) => r.data) ?? [[], [], []]
  // Un vehículo dado de baja no recibe órdenes (requerimientos 7.2)
  const elegibles = activos.filter((a) => a.estadoActivoNombre !== 'Dado de Baja')
  const media = niveles.find((p) => p.nombre === 'Media')

  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    values: { idActivo: '', idTipoMantenimiento: '', idPrioridad: media ? String(media.idPrioridad) : '', motivo: '' },
    resetOptions: { keepDirtyValues: true },
  })
  const idActivo = useWatch({ control, name: 'idActivo' })
  const elegido = elegibles.find((a) => String(a.idActivo) === idActivo)

  const enviar = async (datos) => {
    try {
      const { data } = await crearOrden(datos)
      avisarExito(`Solicitud ${data.codigo} registrada`)
      navigate(`/ordenes/${data.idOrden}`, { state: { desde: regreso } })
    } catch (err) {
      if (!aplicarErroresApi(err, setError, CAMPOS)) avisarError(err)
    }
  }

  const lista = (name, etiqueta, opciones, ayuda) => (
    <Controller name={name} control={control} render={({ field, fieldState }) => (
      <CampoFormulario id={name} etiqueta={etiqueta} requerido ayuda={ayuda} error={fieldState.error?.message}>
        {(props) => <ListaOpciones {...props} value={field.value} onChange={field.onChange} opciones={opciones}
          disabled={catalogos.cargando} placeholder={catalogos.cargando ? 'Cargando…' : 'Elija una opción'} />}
      </CampoFormulario>
    )} />
  )

  return (
    <>
      <EncabezadoPagina titulo="Nueva solicitud" volver={{ to: regreso, texto: textoVolver }} />
      <p className="-mt-2 text-sm text-muted-foreground">Los campos con * son obligatorios. El código de la orden se genera al registrarla.</p>
      {catalogos.error && <EstadoError error={catalogos.error} onReintentar={catalogos.recargar} />}
      <Formulario onSubmit={handleSubmit(enviar)}>
        <BloqueFormulario titulo="Vehículo" columnas={1}>
          {lista('idActivo', 'Vehículo', elegibles.map((a) => ({ valor: a.idActivo, texto: `${a.codigo} · ${a.nombre} · ${a.estadoActivoNombre}` })),
            'Los vehículos dados de baja no aparecen.')}
          {elegido?.estadoActivoNombre === 'En Mantenimiento' && (
            <p role="status" className="flex items-start gap-2 rounded-sm border border-aviso-borde bg-aviso-tinte px-4 py-3 text-sm text-aviso-texto">
              <TriangleAlert className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
              {elegido.codigo} está En Mantenimiento: solo admite solicitudes de naturaleza Emergencia.
            </p>
          )}
        </BloqueFormulario>
        <BloqueFormulario titulo="Solicitud">
          {lista('idTipoMantenimiento', 'Tipo de mantenimiento', tipos.map((t) => ({ valor: t.idTipoMantenimiento, texto: `${t.nombre} (${t.naturalezaNombre})` })))}
          {lista('idPrioridad', 'Prioridad', niveles.map((p) => ({ valor: p.idPrioridad, texto: p.nombre })), 'El Coordinador puede ajustarla al diagnosticar.')}
          <div className="md:col-span-2">
            <Controller name="motivo" control={control} render={({ field, fieldState }) => (
              <CampoFormulario id="motivo" etiqueta="Motivo" requerido ayuda="Qué se observó o por qué se pide el mantenimiento." error={fieldState.error?.message}>
                {(props) => <Textarea {...field} {...props} rows={4} maxLength={500} />}
              </CampoFormulario>
            )} />
          </div>
        </BloqueFormulario>
        <AccionesFormulario procesando={isSubmitting} textoEnviar="Registrar solicitud" onCancelar={() => navigate(regreso)} />
      </Formulario>
    </>
  )
}

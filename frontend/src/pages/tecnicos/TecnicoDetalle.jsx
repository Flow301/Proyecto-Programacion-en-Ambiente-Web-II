import { useCallback, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Ban, Pencil, Plus, X } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BloqueSeccion from '@/components/shared/BloqueSeccion'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoError from '@/components/shared/EstadoError'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import ListaOpciones from '@/components/shared/ListaOpciones'
import PaginaError from '@/components/shared/PaginaError'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import useCarga from '@/hooks/useCarga'
import useRol from '@/hooks/useRol'
import { eleccion } from '@/schemas/comunes'
import { nivelesCertificacion } from '@/services/catalogoService'
import { listarEspecialidades } from '@/services/especialidadService'
import { asociarEspecialidad, inactivarTecnico, obtenerTecnico, quitarEspecialidad } from '@/services/tecnicoService'
import { avisarError, avisarExito, rechazoDefinitivo } from '@/utils/avisos'
import { aFechaIso, formatAntiguedad, formatColones, formatFecha } from '@/utils/format'

// Detalle del técnico con su tabla de especialidades (enunciado Avance 2, 5.2). El Administrador agrega y quita
// especialidades aquí mismo: no hay pantalla aparte para la asociación (enunciado 2.2).
export default function TecnicoDetalle() {
  const { id } = useParams()
  const { state } = useLocation()
  const navigate = useNavigate()
  const { esAdmin } = useRol()
  const cargar = useCallback(() => obtenerTecnico(id), [id])
  const tecnico = useCarga(cargar)
  const [agregando, setAgregando] = useState(false)
  const [quitando, setQuitando] = useState(null)
  const [inactivando, setInactivando] = useState(false)
  const [procesando, setProcesando] = useState(false)

  const volver = { to: state?.desde ?? '/tecnicos', texto: 'Volver a Técnicos' }

  if (tecnico.error?.status === 404) return <PaginaError codigo={404} />
  if (tecnico.cargando && !tecnico.datos) return <><EncabezadoPagina titulo="Técnico" volver={volver} /><IndicadorCarga forma="detalle" /></>
  if (tecnico.error) return <><EncabezadoPagina titulo="Técnico" volver={volver} /><EstadoError error={tecnico.error} onReintentar={tecnico.recargar} /></>

  const t = tecnico.datos.data

  const ejecutar = async (accion, exito, cerrar) => {
    setProcesando(true)
    try {
      await accion()
      avisarExito(exito)
      cerrar()
      return true
    } catch (err) {
      avisarError(err)
      if (rechazoDefinitivo(err)) { setQuitando(null); setInactivando(false) }
      return false
    } finally {
      setProcesando(false)
    }
  }

  return (
    <>
      <EncabezadoPagina
        titulo={t.nombreCompleto}
        pestana={`Técnico ${t.nombreCompleto}`}
        volver={volver}
        estado={<BadgeEstado estado={t.disponible ? 'Disponible' : 'No disponible'} />}
        detalle={`Identificación ${t.identificacion}`}
        acciones={esAdmin && (
          <>
            <Button variant="destructive" onClick={() => setInactivando(true)}><Ban aria-hidden="true" />Inactivar</Button>
            <Button asChild variant="outline"><Link to={`/tecnicos/${t.idTecnico}/editar`} state={state}><Pencil aria-hidden="true" />Editar</Link></Button>
          </>
        )}
      />

      <div className="grid items-start gap-5 2xl:grid-cols-[minmax(0,1fr)_minmax(0,2fr)]">
        <BloqueSeccion titulo="Datos del técnico">
          <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2.5 text-[0.9375rem]">
            <dt className="text-muted-foreground">Teléfono</dt><dd>{t.telefono}</dd>
            <dt className="text-muted-foreground">Correo</dt><dd className="break-all">{t.correo}</dd>
            <dt className="text-muted-foreground">Tarifa por hora</dt><dd className="tabular-nums">{formatColones(t.tarifaHora)}</dd>
            <dt className="text-muted-foreground">Disponibilidad</dt><dd>{t.disponible ? 'Disponible para asignar' : 'No disponible (no se le asignan órdenes)'}</dd>
          </dl>
        </BloqueSeccion>

        <BloqueSeccion
          titulo="Especialidades"
          detalle="Solo se le asignan órdenes cuyo tipo de mantenimiento requiere una de estas especialidades."
          sinRelleno
          acciones={esAdmin && <Button variant="outline" onClick={() => setAgregando(true)}><Plus aria-hidden="true" />Agregar especialidad</Button>}
        >
          <TablaPaginada
            titulo={`Especialidades de ${t.nombreCompleto}`}
            nombre="especialidades"
            className="rounded-none border-0 border-t"
            filas={t.tecnicoEspecialidades}
            claveFila={(e) => e.idEspecialidad}
            vacio={{ titulo: 'Todavía no tiene especialidades', mensaje: 'Sin especialidades no se le pueden asignar órdenes.' }}
            columnas={[
              { clave: 'especialidadNombre', titulo: 'Especialidad', render: (e) => <strong>{e.especialidadNombre}</strong> },
              { clave: 'nivelCertificacionNombre', titulo: 'Nivel de certificación' },
              { clave: 'fecha', titulo: 'Desde', render: (e) => formatFecha(e.fechaCertificacion) },
              { clave: 'antiguedad', titulo: 'Antigüedad', render: (e) => formatAntiguedad(e.fechaCertificacion) },
              ...(esAdmin ? [{
                clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
                render: (e) => <Button variant="ghost" size="sm" className="text-alto-texto hover:bg-alto-tinte" onClick={() => setQuitando(e)}><X aria-hidden="true" />Quitar</Button>,
              }] : []),
            ]}
          />
        </BloqueSeccion>
      </div>

      {agregando && (
        <FormularioEspecialidad
          tecnico={t}
          onCerrar={() => setAgregando(false)}
          onGuardada={() => { setAgregando(false); tecnico.recargar() }}
        />
      )}

      <DialogoConfirmacion
        abierto={Boolean(quitando)}
        titulo={`¿Quitar ${quitando?.especialidadNombre ?? ''} a ${t.nombreCompleto}?`}
        mensaje="Deja de poder asignarse a órdenes que requieran esta especialidad. Las asignaciones ya hechas se conservan."
        textoConfirmar="Quitar especialidad"
        peligro
        procesando={procesando}
        onCancelar={() => setQuitando(null)}
        onConfirmar={() => ejecutar(
          () => quitarEspecialidad(t.idTecnico, quitando.idEspecialidad),
          `${quitando.especialidadNombre} quitada a ${t.nombreCompleto}`,
          () => { setQuitando(null); tecnico.recargar() },
        )}
      />

      <DialogoConfirmacion
        abierto={inactivando}
        titulo={`¿Inactivar al técnico ${t.nombreCompleto}?`}
        mensaje="Deja de aparecer en el taller y no se le pueden asignar órdenes. Sus horas y asignaciones se conservan."
        textoConfirmar="Inactivar"
        peligro
        procesando={procesando}
        onCancelar={() => setInactivando(false)}
        onConfirmar={() => ejecutar(
          () => inactivarTecnico(t.idTecnico),
          `Técnico ${t.nombreCompleto} inactivado`,
          () => navigate(volver.to),
        )}
      />
    </>
  )
}

const esquemaEspecialidad = z.object({
  idEspecialidad: eleccion('la especialidad'),
  idNivelCertificacion: eleccion('el nivel de certificación'),
  fechaCertificacion: z.string().min(1, 'Indique la fecha de certificación.')
    .refine((v) => v <= aFechaIso(), 'La fecha no puede ser futura.'),
})

function FormularioEspecialidad({ tecnico, onCerrar, onGuardada }) {
  const especialidades = useCarga(listarEspecialidades)
  const niveles = useCarga(nivelesCertificacion)
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaEspecialidad),
    defaultValues: { idEspecialidad: '', idNivelCertificacion: '', fechaCertificacion: aFechaIso() },
  })

  // Las que ya tiene no se ofrecen (la API rechaza el duplicado con 409 de todos modos)
  const yaTiene = new Set(tecnico.tecnicoEspecialidades.map((e) => e.idEspecialidad))
  const opciones = (especialidades.datos?.data ?? []).filter((e) => !yaTiene.has(e.idEspecialidad))

  const enviar = async (datos) => {
    try {
      await asociarEspecialidad(tecnico.idTecnico, datos)
      const nombre = opciones.find((e) => e.idEspecialidad === datos.idEspecialidad)?.nombre
      avisarExito(`${nombre} agregada a ${tecnico.nombreCompleto}`)
      onGuardada()
    } catch (err) {
      if (err.status === 409) setError('idEspecialidad', { message: err.message }, { shouldFocus: true })
      else avisarError(err)
    }
  }

  return (
    <DialogoFormulario
      abierto
      titulo={`Agregar especialidad a ${tecnico.nombreCompleto}`}
      textoEnviar="Agregar"
      procesando={isSubmitting}
      onEnviar={handleSubmit(enviar)}
      onCerrar={onCerrar}
    >
      {(especialidades.error || niveles.error) && <EstadoError error={especialidades.error ?? niveles.error} />}
      <Controller name="idEspecialidad" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idEspecialidad" etiqueta="Especialidad" requerido error={fieldState.error?.message}>
          {(props) => <ListaOpciones {...props} ref={field.ref} value={field.value} onChange={field.onChange} disabled={especialidades.cargando}
            placeholder={especialidades.cargando ? 'Cargando…' : 'Elija una especialidad'}
            opciones={opciones.map((e) => ({ valor: e.idEspecialidad, texto: e.nombre }))} />}
        </CampoFormulario>
      )} />
      <Controller name="idNivelCertificacion" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idNivelCertificacion" etiqueta="Nivel de certificación" requerido error={fieldState.error?.message}>
          {(props) => <ListaOpciones {...props} ref={field.ref} value={field.value} onChange={field.onChange} disabled={niveles.cargando}
            opciones={(niveles.datos?.data ?? []).map((n) => ({ valor: n.idNivelCertificacion, texto: n.nombre }))} />}
        </CampoFormulario>
      )} />
      <Controller name="fechaCertificacion" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="fechaCertificacion" etiqueta="Certificado desde" requerido ayuda="No puede ser una fecha futura." error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} type="date" max={aFechaIso()} />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

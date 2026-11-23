import { useCallback, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { texto, textoOpcional } from '@/schemas/comunes'
import { actualizarEspecialidad, crearEspecialidad, eliminarEspecialidad, listarEspecialidadesPagina } from '@/services/especialidadService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'

const TAMANO = 10
const esquema = z.object({ nombre: texto('el nombre', 60), descripcion: textoOpcional(250) })

// Especialidades del taller: listado con creación y edición en ventana, sin pantalla de detalle (enunciado 2.2).
// El Administrador escribe; el Coordinador/Técnico consulta.
export default function Especialidades() {
  const { esAdmin } = useRol()
  const { pagina, irAPagina } = useFiltrosUrl()
  const cargar = useCallback(() => listarEspecialidadesPagina(pagina, TAMANO), [pagina])
  const lista = useCarga(cargar)
  const [editando, setEditando] = useState(null) // null cerrado, {} nueva, o la especialidad
  const [eliminando, setEliminando] = useState(null)
  const [procesando, setProcesando] = useState(false)

  const confirmarEliminar = async () => {
    setProcesando(true)
    try {
      await eliminarEspecialidad(eliminando.idEspecialidad)
      avisarExito(`Especialidad ${eliminando.nombre} eliminada`)
      setEliminando(null)
      lista.recargar()
    } catch (err) {
      avisarError(err)
    } finally {
      setProcesando(false)
    }
  }

  const columnas = [
    { clave: 'nombre', titulo: 'Nombre', render: (e) => <strong>{e.nombre}</strong> },
    { clave: 'descripcion', titulo: 'Descripción', recortar: true, render: (e) => e.descripcion || <span className="text-muted-foreground">Sin descripción</span> },
  ]
  if (esAdmin) {
    columnas.push({
      clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
      render: (e) => (
        <span className="inline-flex gap-1">
          <Button variant="ghost" size="sm" onClick={() => setEditando(e)}><Pencil aria-hidden="true" />Editar</Button>
          <Button variant="ghost" size="sm" className="text-alto-texto hover:bg-alto-tinte" onClick={() => setEliminando(e)}><Trash2 aria-hidden="true" />Eliminar</Button>
        </span>
      ),
    })
  }

  return (
    <>
      <EncabezadoPagina
        titulo="Especialidades"
        detalle="Las competencias de los técnicos del taller."
        acciones={esAdmin && <Button onClick={() => setEditando({})}><Plus aria-hidden="true" />Nueva especialidad</Button>}
      />
      <TablaPaginada
        titulo="Especialidades del taller"
        nombre="especialidades"
        columnas={columnas}
        filas={lista.datos?.data ?? []}
        claveFila={(e) => e.idEspecialidad}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={{ titulo: 'No hay especialidades registradas', accion: esAdmin && <Button onClick={() => setEditando({})}>Nueva especialidad</Button> }}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={irAPagina}
      />

      {editando && (
        <FormularioEspecialidad
          especialidad={editando}
          onCerrar={() => setEditando(null)}
          onGuardada={() => { setEditando(null); lista.recargar() }}
        />
      )}

      <DialogoConfirmacion
        abierto={Boolean(eliminando)}
        titulo={`¿Eliminar la especialidad ${eliminando?.nombre ?? ''}?`}
        mensaje="Solo se puede eliminar si ningún técnico ni tipo de mantenimiento la usa."
        textoConfirmar="Eliminar"
        peligro
        procesando={procesando}
        onCancelar={() => setEliminando(null)}
        onConfirmar={confirmarEliminar}
      />
    </>
  )
}

function FormularioEspecialidad({ especialidad, onCerrar, onGuardada }) {
  const nueva = !especialidad.idEspecialidad
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    defaultValues: { nombre: especialidad.nombre ?? '', descripcion: especialidad.descripcion ?? '' },
  })

  const enviar = async (datos) => {
    try {
      if (nueva) await crearEspecialidad(datos)
      else await actualizarEspecialidad(especialidad.idEspecialidad, datos)
      avisarExito(`Especialidad ${datos.nombre} ${nueva ? 'registrada' : 'actualizada'}`)
      onGuardada()
    } catch (err) {
      // 409: el nombre ya existe; va junto al campo
      if (err.status === 409) setError('nombre', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, ['nombre', 'descripcion'])) avisarError(err)
    }
  }

  return (
    <DialogoFormulario
      abierto
      titulo={nueva ? 'Nueva especialidad' : `Editar ${especialidad.nombre}`}
      procesando={isSubmitting}
      onEnviar={handleSubmit(enviar)}
      onCerrar={onCerrar}
    >
      <Controller name="nombre" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="nombre" etiqueta="Nombre" requerido error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} autoComplete="off" />}
        </CampoFormulario>
      )} />
      <Controller name="descripcion" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="descripcion" etiqueta="Descripción" error={fieldState.error?.message}>
          {(props) => <Textarea {...field} {...props} rows={3} />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

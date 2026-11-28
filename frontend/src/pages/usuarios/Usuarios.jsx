import { useCallback, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Pencil, Plus, RotateCcw, UserX } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import ListaOpciones from '@/components/shared/ListaOpciones'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/context/AuthContext'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import { correo, eleccion, texto } from '@/schemas/comunes'
import {
  actualizarUsuario, cambiarRol, crearUsuario, desactivarUsuario, listarRoles, listarUsuariosPagina, reactivarUsuario, resetearClave,
} from '@/services/usuarioService'
import { avisarError, avisarExito, rechazoDefinitivo } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'
import { formatFecha, formatFechaHora } from '@/utils/format'

const TAMANO = 10

// Mismas reglas que UsuarioCreateDto, UsuarioUpdateDto, UsuarioRolDto y UsuarioClaveDto (contraseña de 8 a 100)
const datos = { nombreCompleto: texto('el nombre completo', 100), login: texto('el usuario', 50), correo, idRol: eleccion('el rol') }
const coincide = (d) => d.clave === d.confirmacion
const errorCoincide = { path: ['confirmacion'], message: 'No coincide con la contraseña.' }
const esquemaNuevo = z.object({
  ...datos,
  clave: z.string().min(8, 'La contraseña debe tener al menos 8 caracteres.').max(100, 'Máximo 100 caracteres.'),
  confirmacion: z.string().min(1, 'Repita la contraseña.'),
}).refine(coincide, errorCoincide)
// En edición la contraseña es opcional: vacía conserva la actual (enunciado Avance 2, 5.1)
const esquemaEdicion = z.object({
  ...datos,
  clave: z.string().refine((v) => v === '' || (v.length >= 8 && v.length <= 100), 'Si escribe una contraseña, debe tener entre 8 y 100 caracteres.'),
  confirmacion: z.string(),
}).refine(coincide, errorCoincide)

// Usuarios y roles (enunciado Avance 2, 5.1; requerimientos 7.10). Solo Administrador.
// Las reglas (no desactivarse a sí mismo, no dejar el sistema sin Administrador, login y correo únicos) las valida la API;
// la interfaz además no ofrece al Administrador desactivarse ni cambiarse el rol a sí mismo.
export default function Usuarios() {
  const { usuario: yo } = useAuth()
  const filtros = useFiltrosUrl()
  const idRol = filtros.valor('idRol')
  const estado = filtros.valor('estado')
  const pagina = filtros.pagina
  const roles = useCarga(listarRoles)
  const cargar = useCallback(() => listarUsuariosPagina(pagina, TAMANO, { idRol, estado }), [pagina, idRol, estado])
  const lista = useCarga(cargar)
  const [dialogo, setDialogo] = useState(null) // { tipo: 'nuevo' | 'editar' | 'estado', usuario? }
  const [procesando, setProcesando] = useState(false)

  const opcionesRol = (roles.datos?.data ?? []).map((r) => ({ valor: r.idRol, texto: r.nombre }))
  const esYo = (x) => x.login === yo?.login
  const cerrar = () => setDialogo(null)
  const hecho = () => { setDialogo(null); lista.recargar() }
  const u = dialogo?.usuario

  const cambiarEstado = async () => {
    setProcesando(true)
    try {
      if (u.estado) await desactivarUsuario(u.idUsuario)
      else await reactivarUsuario(u.idUsuario)
      avisarExito(`Usuario ${u.login} ${u.estado ? 'desactivado' : 'activado'}`)
      hecho()
    } catch (err) {
      avisarError(err)
      if (rechazoDefinitivo(err)) cerrar()
    } finally {
      setProcesando(false)
    }
  }

  return (
    <>
      <EncabezadoPagina
        titulo="Usuarios"
        detalle="Las cuentas de acceso al sistema y su rol. Los usuarios no se eliminan: se desactivan."
        acciones={<Button onClick={() => setDialogo({ tipo: 'nuevo' })}><Plus aria-hidden="true" />Nuevo usuario</Button>}
      />
      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
        <div className="flex w-60 flex-col gap-1.5">
          <label htmlFor="filtro-rol" className="text-sm font-semibold">Rol</label>
          <ListaOpciones id="filtro-rol" value={idRol} onChange={(v) => filtros.cambiar('idRol', v)} todas="Todos los roles" opciones={opcionesRol} />
        </div>
        <div className="flex w-48 flex-col gap-1.5">
          <label htmlFor="filtro-estado" className="text-sm font-semibold">Estado</label>
          <ListaOpciones id="filtro-estado" value={estado} onChange={(v) => filtros.cambiar('estado', v)} todas="Todos"
            opciones={[{ valor: 'true', texto: 'Activos' }, { valor: 'false', texto: 'Inactivos' }]} />
        </div>
      </BarraFiltros>
      <TablaPaginada
        titulo="Usuarios del sistema"
        nombre="usuarios"
        filas={lista.datos?.data ?? []}
        claveFila={(x) => x.idUsuario}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={filtros.hayFiltros
          ? { titulo: 'Ningún usuario coincide con el filtro', tipo: 'sinResultados', accion: <Button variant="outline" onClick={filtros.limpiar}>Limpiar filtros</Button> }
          : { titulo: 'No hay usuarios registrados' }}
        columnas={[
          {
            clave: 'nombreCompleto', titulo: 'Nombre',
            render: (x) => (
              <span className="flex flex-col">
                <span><strong>{x.nombreCompleto}</strong>{esYo(x) && <span className="text-muted-foreground"> (usted)</span>}</span>
                <span className="text-sm text-muted-foreground">{x.correo}</span>
              </span>
            ),
          },
          { clave: 'login', titulo: 'Usuario' },
          { clave: 'rolNombre', titulo: 'Rol' },
          { clave: 'estado', titulo: 'Estado', render: (x) => <BadgeEstado estado={x.estado ? 'Activo' : 'Inactivo'} /> },
          { clave: 'ultimoAcceso', titulo: 'Último acceso', render: (x) => (x.ultimoAcceso ? <span title={formatFechaHora(x.ultimoAcceso)}>{formatFecha(x.ultimoAcceso)}</span> : <span className="text-muted-foreground">Nunca</span>) },
          {
            // Acciones por fila: editar y activar/desactivar (enunciado Avance 2, 5.1). Nadie se desactiva a sí mismo.
            clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
            render: (x) => (
              <span className="inline-flex gap-1">
                <Button variant="ghost" size="sm" onClick={() => setDialogo({ tipo: 'editar', usuario: x })}><Pencil aria-hidden="true" />Editar</Button>
                {!x.estado
                  ? <Button variant="ghost" size="sm" onClick={() => setDialogo({ tipo: 'estado', usuario: x })}><RotateCcw aria-hidden="true" />Activar</Button>
                  : !esYo(x) && <Button variant="ghost" size="sm" className="text-alto-texto hover:bg-alto-tinte" onClick={() => setDialogo({ tipo: 'estado', usuario: x })}><UserX aria-hidden="true" />Desactivar</Button>}
              </span>
            ),
          },
        ]}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={filtros.irAPagina}
      />

      {(dialogo?.tipo === 'nuevo' || dialogo?.tipo === 'editar') && (
        <FormularioUsuario usuario={u} propio={u ? esYo(u) : false} roles={opcionesRol} onCerrar={cerrar} onGuardado={hecho} />
      )}
      <DialogoConfirmacion
        abierto={dialogo?.tipo === 'estado'}
        titulo={u ? `¿${u.estado ? 'Desactivar' : 'Activar'} al usuario ${u.login} (${u.nombreCompleto})?` : ''}
        mensaje={u?.estado
          ? `No podrá iniciar sesión. ${u.tieneActividad ? 'Su actividad (órdenes, cambios de estado y lecturas) se conserva.' : 'Puede activarse después.'}`
          : 'Podrá volver a iniciar sesión con su contraseña actual.'}
        textoConfirmar={u?.estado ? 'Desactivar' : 'Activar'}
        peligro={u?.estado}
        procesando={procesando}
        onCancelar={cerrar}
        onConfirmar={cambiarEstado}
      />
    </>
  )
}

// 409 de login o correo repetidos: va junto al campo que corresponde
function errorConflicto(err, setError) {
  if (err.status !== 409) return false
  const campo = /correo/i.test(err.message) ? 'correo' : /login|usuario/i.test(err.message) ? 'login' : null
  if (!campo) return false
  setError(campo, { message: err.message }, { shouldFocus: true })
  return true
}

function campoTexto(control, name, etiqueta, props = {}) {
  return (
    <Controller name={name} control={control} render={({ field, fieldState }) => (
      <CampoFormulario id={name} etiqueta={etiqueta} requerido={props.requerido ?? true} ayuda={props.ayuda} error={fieldState.error?.message}>
        {(p) => <Input {...field} {...p} type={props.type} autoComplete={props.autoComplete ?? 'off'} spellCheck={false} />}
      </CampoFormulario>
    )} />
  )
}

// Nuevo usuario o edición: nombre, login, correo, rol y contraseña (opcional al editar). Al editar se llama solo a lo
// que cambió: los datos (PUT), el rol si es otro (PUT rol) y la contraseña si se escribió una (PUT clave).
function FormularioUsuario({ usuario, propio, roles, onCerrar, onGuardado }) {
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(usuario ? esquemaEdicion : esquemaNuevo),
    defaultValues: {
      nombreCompleto: usuario?.nombreCompleto ?? '',
      login: usuario?.login ?? '',
      correo: usuario?.correo ?? '',
      idRol: usuario ? String(usuario.idRol) : '',
      clave: '',
      confirmacion: '',
    },
  })

  const enviar = async ({ nombreCompleto, login, correo: email, idRol, clave }) => {
    let paso = 'datos'
    try {
      if (!usuario) {
        await crearUsuario({ nombreCompleto, login, correo: email, idRol, clave })
        avisarExito(`Usuario ${login} registrado`)
      } else {
        await actualizarUsuario(usuario.idUsuario, { nombreCompleto, login, correo: email })
        if (idRol !== usuario.idRol) { paso = 'rol'; await cambiarRol(usuario.idUsuario, idRol) }
        if (clave) { paso = 'clave'; await resetearClave(usuario.idUsuario, clave) }
        avisarExito(`Usuario ${login} actualizado${clave ? ', con contraseña nueva' : ''}`)
      }
      onGuardado()
    } catch (err) {
      if (paso === 'rol' && err.status === 409) setError('idRol', { message: err.message }, { shouldFocus: true })
      else if (!errorConflicto(err, setError) && !aplicarErroresApi(err, setError, ['nombreCompleto', 'login', 'correo', 'idRol', 'clave'])) avisarError(err)
    }
  }

  return (
    <DialogoFormulario abierto titulo={usuario ? `Editar a ${usuario.login}` : 'Nuevo usuario'}
      descripcion={usuario
        ? 'Si cambia el rol de alguien con la sesión abierta, el nuevo rol aplica desde su siguiente inicio de sesión.'
        : 'Entregue la contraseña inicial al usuario.'}
      textoEnviar={usuario ? 'Guardar cambios' : 'Registrar usuario'} procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      {campoTexto(control, 'nombreCompleto', 'Nombre completo')}
      <div className="grid gap-4 sm:grid-cols-2">
        {campoTexto(control, 'login', 'Usuario', { ayuda: 'Con el que inicia sesión.' })}
        {campoTexto(control, 'correo', 'Correo', { type: 'email' })}
      </div>
      <Controller name="idRol" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idRol" etiqueta="Rol" requerido error={fieldState.error?.message}
          ayuda={propio ? 'No puede cambiarse su propio rol de Administrador.' : undefined}>
          {(p) => <ListaOpciones {...p} ref={field.ref} value={field.value} onChange={field.onChange} opciones={roles} disabled={propio} />}
        </CampoFormulario>
      )} />
      <div className="grid gap-4 sm:grid-cols-2">
        {campoTexto(control, 'clave', usuario ? 'Contraseña nueva' : 'Contraseña inicial', {
          type: 'password', autoComplete: 'new-password', requerido: !usuario,
          ayuda: usuario ? 'Déjela vacía para conservar la actual.' : 'De 8 a 100 caracteres.',
        })}
        {campoTexto(control, 'confirmacion', 'Repita la contraseña', { type: 'password', autoComplete: 'new-password', requerido: !usuario })}
      </div>
    </DialogoFormulario>
  )
}

import { useCallback, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Ellipsis, KeyRound, Pencil, Plus, RotateCcw, UserCog, UserX } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoConfirmacion from '@/components/shared/DialogoConfirmacion'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import ListaOpciones from '@/components/shared/ListaOpciones'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/context/AuthContext'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import { correo, eleccion, texto } from '@/schemas/comunes'
import {
  actualizarUsuario, cambiarRol, crearUsuario, desactivarUsuario, listarRoles, listarUsuariosPagina, reactivarUsuario, resetearClave,
} from '@/services/usuarioService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'
import { formatFecha, formatFechaHora } from '@/utils/format'

const TAMANO = 10

// Contraseña inicial o reseteada: 8 a 100 caracteres (UsuarioCreateDto, UsuarioClaveDto), escrita dos veces
const clave = z.string().min(8, 'La contraseña debe tener al menos 8 caracteres.').max(100, 'Máximo 100 caracteres.')
const coincide = (d) => d.clave === d.confirmacion
const errorCoincide = { path: ['confirmacion'], message: 'No coincide con la contraseña.' }

const datos = { nombreCompleto: texto('el nombre completo', 100), login: texto('el usuario', 50), correo }
const esquemaNuevo = z.object({ ...datos, idRol: eleccion('el rol'), clave, confirmacion: z.string().min(1, 'Repita la contraseña.') }).refine(coincide, errorCoincide)
const esquemaEdicion = z.object(datos)
const esquemaRol = z.object({ idRol: eleccion('el rol') })
const esquemaClave = z.object({ clave, confirmacion: z.string().min(1, 'Repita la contraseña.') }).refine(coincide, errorCoincide)

// Usuarios y roles (enunciado Avance 2, 5.1; requerimientos 7.10). Solo Administrador.
// Las reglas (no desactivarse a sí mismo, no dejar el sistema sin Administrador, login y correo únicos) las valida la API.
export default function Usuarios() {
  const { usuario: yo } = useAuth()
  const filtros = useFiltrosUrl()
  const idRol = filtros.valor('idRol')
  const estado = filtros.valor('estado')
  const pagina = filtros.pagina
  const roles = useCarga(listarRoles)
  const cargar = useCallback(() => listarUsuariosPagina(pagina, TAMANO, { idRol, estado }), [pagina, idRol, estado])
  const lista = useCarga(cargar)
  const [dialogo, setDialogo] = useState(null) // { tipo: 'nuevo' | 'editar' | 'rol' | 'clave' | 'estado', usuario? }
  const [procesando, setProcesando] = useState(false)

  const opcionesRol = (roles.datos?.data ?? []).map((r) => ({ valor: r.idRol, texto: r.nombre }))
  const cerrar = () => setDialogo(null)
  const hecho = () => { setDialogo(null); lista.recargar() }
  const u = dialogo?.usuario

  const cambiarEstado = async () => {
    setProcesando(true)
    try {
      if (u.estado) await desactivarUsuario(u.idUsuario)
      else await reactivarUsuario(u.idUsuario)
      avisarExito(`Usuario ${u.login} ${u.estado ? 'desactivado' : 'reactivado'}`)
      hecho()
    } catch (err) {
      avisarError(err)
      if (err.status === 409) cerrar() // regla de negocio: el aviso explica por qué y no hay nada que corregir en la ventana
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
                <span><strong>{x.nombreCompleto}</strong>{x.login === yo?.login && <span className="text-muted-foreground"> (usted)</span>}</span>
                <span className="text-sm text-muted-foreground">{x.correo}</span>
              </span>
            ),
          },
          { clave: 'login', titulo: 'Usuario' },
          { clave: 'rolNombre', titulo: 'Rol' },
          { clave: 'estado', titulo: 'Estado', render: (x) => <BadgeEstado estado={x.estado ? 'Activo' : 'Inactivo'} /> },
          { clave: 'ultimoAcceso', titulo: 'Último acceso', render: (x) => (x.ultimoAcceso ? <span title={formatFechaHora(x.ultimoAcceso)}>{formatFecha(x.ultimoAcceso)}</span> : <span className="text-muted-foreground">Nunca</span>) },
          {
            clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
            render: (x) => (
              <span className="inline-flex gap-1">
                <Button variant="ghost" size="sm" onClick={() => setDialogo({ tipo: 'editar', usuario: x })}><Pencil aria-hidden="true" />Editar</Button>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="sm" aria-label={`Más acciones para ${x.login}`}><Ellipsis aria-hidden="true" /></Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem onSelect={() => setDialogo({ tipo: 'rol', usuario: x })}><UserCog aria-hidden="true" />Cambiar rol</DropdownMenuItem>
                    <DropdownMenuItem onSelect={() => setDialogo({ tipo: 'clave', usuario: x })}><KeyRound aria-hidden="true" />Resetear contraseña</DropdownMenuItem>
                    <DropdownMenuSeparator />
                    {x.estado
                      ? <DropdownMenuItem variant="destructive" onSelect={() => setDialogo({ tipo: 'estado', usuario: x })}><UserX aria-hidden="true" />Desactivar</DropdownMenuItem>
                      : <DropdownMenuItem onSelect={() => setDialogo({ tipo: 'estado', usuario: x })}><RotateCcw aria-hidden="true" />Reactivar</DropdownMenuItem>}
                  </DropdownMenuContent>
                </DropdownMenu>
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

      {(dialogo?.tipo === 'nuevo' || dialogo?.tipo === 'editar') && <FormularioUsuario usuario={u} roles={opcionesRol} onCerrar={cerrar} onGuardado={hecho} />}
      {dialogo?.tipo === 'rol' && <FormularioRol usuario={u} roles={opcionesRol} onCerrar={cerrar} onGuardado={hecho} />}
      {dialogo?.tipo === 'clave' && <FormularioClave usuario={u} onCerrar={cerrar} onGuardado={cerrar} />}
      <DialogoConfirmacion
        abierto={dialogo?.tipo === 'estado'}
        titulo={u ? `¿${u.estado ? 'Desactivar' : 'Reactivar'} al usuario ${u.login} (${u.nombreCompleto})?` : ''}
        mensaje={u?.estado
          ? `No podrá iniciar sesión. ${u.tieneActividad ? 'Su actividad (órdenes, cambios de estado y lecturas) se conserva.' : 'Puede reactivarse después.'}`
          : 'Podrá volver a iniciar sesión con su contraseña actual.'}
        textoConfirmar={u?.estado ? 'Desactivar' : 'Reactivar'}
        peligro={u?.estado}
        procesando={procesando}
        onCancelar={cerrar}
        onConfirmar={cambiarEstado}
      />
    </>
  )
}

// 409: login o correo repetidos, o una regla del Administrador; se pone junto al campo que corresponde
function errorConflicto(err, setError) {
  if (err.status !== 409) return false
  const campo = /correo/i.test(err.message) ? 'correo' : /login/i.test(err.message) ? 'login' : null
  if (!campo) return false
  setError(campo, { message: err.message }, { shouldFocus: true })
  return true
}

function campoTexto(control, name, etiqueta, props = {}) {
  return (
    <Controller name={name} control={control} render={({ field, fieldState }) => (
      <CampoFormulario id={name} etiqueta={etiqueta} requerido ayuda={props.ayuda} error={fieldState.error?.message}>
        {(p) => <Input {...field} {...p} type={props.type} autoComplete={props.autoComplete ?? 'off'} spellCheck={false} />}
      </CampoFormulario>
    )} />
  )
}

function campoRol(control, roles) {
  return (
    <Controller name="idRol" control={control} render={({ field, fieldState }) => (
      <CampoFormulario id="idRol" etiqueta="Rol" requerido error={fieldState.error?.message}>
        {(p) => <ListaOpciones {...p} value={field.value} onChange={field.onChange} opciones={roles} />}
      </CampoFormulario>
    )} />
  )
}

function FormularioUsuario({ usuario, roles, onCerrar, onGuardado }) {
  const esquema = usuario ? esquemaEdicion : esquemaNuevo
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    defaultValues: usuario
      ? { nombreCompleto: usuario.nombreCompleto, login: usuario.login, correo: usuario.correo }
      : { nombreCompleto: '', login: '', correo: '', idRol: '', clave: '', confirmacion: '' },
  })
  const enviar = async (formulario) => {
    const d = { ...formulario }
    delete d.confirmacion
    try {
      if (usuario) await actualizarUsuario(usuario.idUsuario, d)
      else await crearUsuario(d)
      avisarExito(`Usuario ${d.login} ${usuario ? 'actualizado' : 'registrado'}`)
      onGuardado()
    } catch (err) {
      if (!errorConflicto(err, setError) && !aplicarErroresApi(err, setError, ['nombreCompleto', 'login', 'correo', 'idRol', 'clave'])) avisarError(err)
    }
  }
  return (
    <DialogoFormulario abierto titulo={usuario ? `Editar a ${usuario.login}` : 'Nuevo usuario'}
      descripcion={usuario ? 'El rol y la contraseña se cambian con sus propias acciones.' : 'Entregue la contraseña inicial al usuario; podrá cambiarla al ingresar.'}
      textoEnviar={usuario ? 'Guardar cambios' : 'Registrar usuario'} procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      {campoTexto(control, 'nombreCompleto', 'Nombre completo')}
      <div className="grid gap-4 sm:grid-cols-2">
        {campoTexto(control, 'login', 'Usuario', { ayuda: 'Con el que inicia sesión.' })}
        {campoTexto(control, 'correo', 'Correo', { type: 'email' })}
      </div>
      {!usuario && (
        <>
          {campoRol(control, roles)}
          <div className="grid gap-4 sm:grid-cols-2">
            {campoTexto(control, 'clave', 'Contraseña inicial', { type: 'password', autoComplete: 'new-password', ayuda: 'De 8 a 100 caracteres.' })}
            {campoTexto(control, 'confirmacion', 'Repita la contraseña', { type: 'password', autoComplete: 'new-password' })}
          </div>
        </>
      )}
    </DialogoFormulario>
  )
}

function FormularioRol({ usuario, roles, onCerrar, onGuardado }) {
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaRol),
    defaultValues: { idRol: String(usuario.idRol) },
  })
  const enviar = async ({ idRol }) => {
    try {
      await cambiarRol(usuario.idUsuario, idRol)
      avisarExito(`${usuario.login} ahora es ${roles.find((r) => r.valor === idRol)?.texto}`)
      onGuardado()
    } catch (err) {
      if (err.status === 409) setError('idRol', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, ['idRol'])) avisarError(err)
    }
  }
  return (
    <DialogoFormulario abierto titulo={`Cambiar el rol de ${usuario.login}`}
      descripcion={`Rol actual: ${usuario.rolNombre}. Si tiene la sesión abierta, el nuevo rol aplica desde su siguiente inicio de sesión.`}
      textoEnviar="Cambiar rol" procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      {campoRol(control, roles)}
    </DialogoFormulario>
  )
}

function FormularioClave({ usuario, onCerrar, onGuardado }) {
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaClave),
    defaultValues: { clave: '', confirmacion: '' },
  })
  const enviar = async (d) => {
    try {
      await resetearClave(usuario.idUsuario, d.clave)
      avisarExito(`Contraseña de ${usuario.login} restablecida`)
      onGuardado()
    } catch (err) {
      if (!aplicarErroresApi(err, setError, ['clave'])) avisarError(err)
    }
  }
  return (
    <DialogoFormulario abierto titulo={`Resetear la contraseña de ${usuario.login}`}
      descripcion={`No pide la contraseña actual. Entregue la nueva a ${usuario.nombreCompleto}.${usuario.estado ? '' : ' La cuenta sigue inactiva hasta que la reactive.'}`}
      textoEnviar="Resetear contraseña" procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      <div className="grid gap-4 sm:grid-cols-2">
        {campoTexto(control, 'clave', 'Contraseña nueva', { type: 'password', autoComplete: 'new-password', ayuda: 'De 8 a 100 caracteres.' })}
        {campoTexto(control, 'confirmacion', 'Repita la contraseña', { type: 'password', autoComplete: 'new-password' })}
      </div>
    </DialogoFormulario>
  )
}

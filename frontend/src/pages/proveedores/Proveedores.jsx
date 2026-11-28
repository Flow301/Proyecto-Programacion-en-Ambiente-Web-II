import { useCallback, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Ban, Pencil, Plus } from 'lucide-react'
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
import { correo, telefono, texto, textoOpcional } from '@/schemas/comunes'
import { actualizarProveedor, crearProveedor, inactivarProveedor, listarProveedoresPagina } from '@/services/proveedorService'
import { avisarError, avisarExito, rechazoDefinitivo } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'

const TAMANO = 10

// Mismas reglas que ProveedorCreateDto
const esquema = z.object({
  nombre: texto('el nombre', 100),
  cedulaJuridica: z.string().trim().min(1, 'Ingrese la cédula jurídica.').regex(/^\d-\d{3}-\d{6}$/, 'Use el formato 3-101-123456.'),
  personaContacto: texto('la persona de contacto', 100),
  telefono,
  correo,
  direccion: texto('la dirección', 200),
  condicionesGenerales: textoOpcional(300),
})
const CAMPOS = Object.keys(esquema.shape)

// Proveedores: listado con creación y edición en ventana, sin pantalla de detalle (enunciado 2.2).
// Eliminar es inactivar: sus datos aparecen en asociaciones con repuestos (requerimientos 7.4).
export default function Proveedores() {
  const { esAdmin } = useRol()
  const { pagina, irAPagina } = useFiltrosUrl()
  const cargar = useCallback(() => listarProveedoresPagina(pagina, TAMANO), [pagina])
  const lista = useCarga(cargar)
  const [editando, setEditando] = useState(null)
  const [inactivando, setInactivando] = useState(null)
  const [procesando, setProcesando] = useState(false)

  const confirmarInactivar = async () => {
    setProcesando(true)
    try {
      await inactivarProveedor(inactivando.idProveedor)
      avisarExito(`Proveedor ${inactivando.nombre} inactivado`)
      setInactivando(null)
      lista.recargar()
    } catch (err) {
      avisarError(err)
      if (rechazoDefinitivo(err)) setInactivando(null)
    } finally {
      setProcesando(false)
    }
  }

  const columnas = [
    { clave: 'nombre', titulo: 'Proveedor', recortar: true, render: (p) => <strong>{p.nombre}</strong>, completo: (p) => p.nombre },
    { clave: 'cedulaJuridica', titulo: 'Cédula jurídica' },
    { clave: 'personaContacto', titulo: 'Contacto', recortar: true },
    { clave: 'telefono', titulo: 'Teléfono' },
    { clave: 'correo', titulo: 'Correo', recortar: true },
  ]
  if (esAdmin) {
    columnas.push({
      clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
      render: (p) => (
        <span className="inline-flex gap-1">
          <Button variant="ghost" size="sm" onClick={() => setEditando(p)}><Pencil aria-hidden="true" />Editar</Button>
          <Button variant="ghost" size="sm" className="text-alto-texto hover:bg-alto-tinte" onClick={() => setInactivando(p)}><Ban aria-hidden="true" />Inactivar</Button>
        </span>
      ),
    })
  }

  return (
    <>
      <EncabezadoPagina
        titulo="Proveedores"
        detalle="Quienes suministran los repuestos. Las condiciones por repuesto se definen en el detalle de cada repuesto."
        acciones={esAdmin && <Button onClick={() => setEditando({})}><Plus aria-hidden="true" />Nuevo proveedor</Button>}
      />
      <TablaPaginada
        titulo="Proveedores activos"
        nombre="proveedores"
        columnas={columnas}
        filas={lista.datos?.data ?? []}
        claveFila={(p) => p.idProveedor}
        cargando={lista.cargando}
        error={lista.error}
        onReintentar={lista.recargar}
        vacio={{ titulo: 'No hay proveedores activos', accion: esAdmin && <Button onClick={() => setEditando({})}>Nuevo proveedor</Button> }}
        pagina={pagina}
        tamano={TAMANO}
        totalRegistros={lista.datos?.totalRecords}
        totalPaginas={lista.datos?.totalPages}
        onCambiarPagina={irAPagina}
      />

      {editando && (
        <FormularioProveedor proveedor={editando} onCerrar={() => setEditando(null)} onGuardado={() => { setEditando(null); lista.recargar() }} />
      )}

      <DialogoConfirmacion
        abierto={Boolean(inactivando)}
        titulo={`¿Inactivar el proveedor ${inactivando?.nombre ?? ''}?`}
        mensaje="Deja de aparecer en los listados y no participa en la recomendación de proveedores. Sus datos se conservan en las asociaciones existentes."
        textoConfirmar="Inactivar"
        peligro
        procesando={procesando}
        onCancelar={() => setInactivando(null)}
        onConfirmar={confirmarInactivar}
      />
    </>
  )
}

const definicion = [
  { name: 'nombre', etiqueta: 'Nombre', requerido: true },
  { name: 'cedulaJuridica', etiqueta: 'Cédula jurídica', requerido: true, ayuda: 'Formato 3-101-123456.' },
  { name: 'personaContacto', etiqueta: 'Persona de contacto', requerido: true },
  { name: 'telefono', etiqueta: 'Teléfono', requerido: true, type: 'tel', autoComplete: 'off' },
  { name: 'correo', etiqueta: 'Correo', requerido: true, type: 'email', autoComplete: 'off' },
  { name: 'direccion', etiqueta: 'Dirección', requerido: true },
  { name: 'condicionesGenerales', etiqueta: 'Condiciones generales', area: true, ayuda: 'Por ejemplo: crédito a 30 días, garantía de 6 meses.' },
]

function FormularioProveedor({ proveedor, onCerrar, onGuardado }) {
  const nuevo = !proveedor.idProveedor
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    defaultValues: Object.fromEntries(CAMPOS.map((c) => [c, proveedor[c] ?? ''])),
  })

  const enviar = async (datos) => {
    try {
      if (nuevo) await crearProveedor(datos)
      else await actualizarProveedor(proveedor.idProveedor, datos)
      avisarExito(`Proveedor ${datos.nombre} ${nuevo ? 'registrado' : 'actualizado'}`)
      onGuardado()
    } catch (err) {
      // 409: la cédula jurídica ya está registrada
      if (err.status === 409) setError('cedulaJuridica', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, CAMPOS)) avisarError(err)
    }
  }

  return (
    <DialogoFormulario
      abierto
      titulo={nuevo ? 'Nuevo proveedor' : `Editar ${proveedor.nombre}`}
      procesando={isSubmitting}
      onEnviar={handleSubmit(enviar)}
      onCerrar={onCerrar}
    >
      {definicion.map(({ name, etiqueta, requerido, ayuda, area, type, autoComplete }) => (
        <Controller key={name} name={name} control={control} render={({ field, fieldState }) => (
          <CampoFormulario id={name} etiqueta={etiqueta} requerido={requerido} ayuda={ayuda} error={fieldState.error?.message}>
            {(props) => area
              ? <Textarea {...field} {...props} rows={2} />
              : <Input {...field} {...props} type={type ?? 'text'} autoComplete={autoComplete ?? 'off'} />}
          </CampoFormulario>
        )} />
      ))}
    </DialogoFormulario>
  )
}

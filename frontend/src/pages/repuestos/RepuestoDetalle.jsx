import { useCallback, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Ban, PackagePlus, Pencil, Plus, X } from 'lucide-react'
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
import { eleccion, numero } from '@/schemas/comunes'
import { listarProveedores } from '@/services/proveedorService'
import {
  actualizarCondiciones, asociarProveedor, inactivarRepuesto, obtenerRepuesto, proveedorRecomendado,
  proveedoresDeRepuesto, quitarProveedor, registrarEntrada,
} from '@/services/repuestoService'
import { avisarError, avisarExito, rechazoDefinitivo } from '@/utils/avisos'
import { formatColones, formatNumero } from '@/utils/format'
import { aplicarErroresApi } from '@/utils/errores'

// Detalle del repuesto (enunciado Avance 2, 5.2 y 5.6): datos, campos calculados y la tabla de proveedores
// ordenada por puntaje con el recomendado destacado. El puntaje lo calcula la API; aquí solo se muestra.
// El Administrador asocia proveedores, edita sus condiciones y registra entradas de stock aquí mismo (enunciado 2.2).
export default function RepuestoDetalle() {
  const { id } = useParams()
  const { state } = useLocation()
  const navigate = useNavigate()
  const { esAdmin } = useRol()
  const cargar = useCallback(
    () => Promise.all([obtenerRepuesto(id), proveedoresDeRepuesto(id), proveedorRecomendado(id)]),
    [id],
  )
  const carga = useCarga(cargar)
  const [dialogo, setDialogo] = useState(null) // { tipo: 'entrada' | 'proveedor', proveedor? }
  const [quitando, setQuitando] = useState(null)
  const [inactivando, setInactivando] = useState(false)
  const [procesando, setProcesando] = useState(false)

  const volver = { to: state?.desde ?? '/repuestos', texto: 'Volver a Repuestos' }

  if (carga.error?.status === 404) return <PaginaError codigo={404} />
  if (carga.cargando && !carga.datos) return <><EncabezadoPagina titulo="Repuesto" volver={volver} /><IndicadorCarga forma="detalle" /></>
  if (carga.error) return <><EncabezadoPagina titulo="Repuesto" volver={volver} /><EstadoError error={carga.error} onReintentar={carga.recargar} /></>

  const [{ data: r }, { data: asociaciones }, { data: recomendacion }] = carga.datos
  // Une cada asociación con su puntaje; los inactivos no participan en la recomendación (no tienen puntaje)
  const puntajes = new Map(recomendacion.opciones.map((o) => [o.idProveedor, o.puntaje]))
  const proveedores = asociaciones
    .map((a) => ({ ...a, puntaje: puntajes.get(a.idProveedor) }))
    .sort((a, b) => (b.puntaje ?? -1) - (a.puntaje ?? -1))
  const idRecomendado = recomendacion.recomendado?.idProveedor

  const ejecutar = async (accion, exito, despues) => {
    setProcesando(true)
    try {
      await accion()
      avisarExito(exito)
      despues()
    } catch (err) {
      avisarError(err)
      if (rechazoDefinitivo(err)) { setQuitando(null); setInactivando(false) }
    } finally {
      setProcesando(false)
    }
  }

  const alGuardar = () => { setDialogo(null); carga.recargar() }

  return (
    <>
      <EncabezadoPagina
        codigo={r.codigo}
        pestana={`Repuesto ${r.codigo}`}
        volver={volver}
        estado={r.stockBajo && <BadgeEstado estado="Stock bajo" />}
        detalle={r.nombre}
        acciones={esAdmin && (
          <>
            <Button variant="destructive" onClick={() => setInactivando(true)}><Ban aria-hidden="true" />Inactivar</Button>
            <Button asChild variant="outline"><Link to={`/repuestos/${r.idRepuesto}/editar`} state={state}><Pencil aria-hidden="true" />Editar</Link></Button>
            <Button onClick={() => setDialogo({ tipo: 'entrada' })}><PackagePlus aria-hidden="true" />Registrar entrada</Button>
          </>
        )}
      />

      <div className="grid items-start gap-5 2xl:grid-cols-[minmax(0,1fr)_minmax(0,2fr)]">
        <BloqueSeccion titulo="Inventario">
          <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2.5 text-[0.9375rem] tabular-nums">
            <dt className="text-muted-foreground">Unidad de medida</dt><dd>{r.unidadMedida}</dd>
            <dt className="text-muted-foreground">Stock actual</dt><dd className="text-lg font-bold">{formatNumero(r.stockActual)}</dd>
            <dt className="text-muted-foreground">Stock mínimo</dt><dd>{formatNumero(r.stockMinimo)}</dd>
            <dt className="text-muted-foreground">Faltante</dt><dd className={r.faltante ? 'font-bold text-alto-texto' : ''}>{formatNumero(r.faltante)}</dd>
            <dt className="text-muted-foreground">Costo unitario</dt><dd>{formatColones(r.costoUnitario)}</dd>
          </dl>
          <p className="mt-4 text-sm text-muted-foreground">El costo unitario se copia al detalle de cada orden en el momento en que se usa el repuesto.</p>
        </BloqueSeccion>

        <BloqueSeccion
          titulo="Proveedores"
          detalle="Del mayor al menor puntaje; el recomendado va destacado."
          sinRelleno
          acciones={esAdmin && <Button variant="outline" onClick={() => setDialogo({ tipo: 'proveedor' })}><Plus aria-hidden="true" />Agregar proveedor</Button>}
        >
          <TablaPaginada
            titulo={`Proveedores de ${r.codigo}, del mayor al menor puntaje`}
            nombre="proveedores"
            className="rounded-none border-0 border-t"
            filas={proveedores}
            claveFila={(p) => p.idProveedor}
            vacio={{ titulo: 'Este repuesto todavía no tiene proveedores', mensaje: 'Sin proveedores no hay recomendación de compra.' }}
            columnas={[
              {
                clave: 'proveedorNombre', titulo: 'Proveedor',
                render: (p) => (
                  <span className="flex flex-wrap items-center gap-2">
                    <strong>{p.proveedorNombre}</strong>
                    {p.idProveedor === idRecomendado && <BadgeEstado estado="Recomendado" />}
                    {!p.proveedorEstado && <BadgeEstado estado="Inactivo" />}
                  </span>
                ),
              },
              { clave: 'precioOfrecido', titulo: 'Precio ofrecido', derecha: true, render: (p) => formatColones(p.precioOfrecido) },
              { clave: 'tiempoEntregaDias', titulo: 'Entrega', derecha: true, render: (p) => `${p.tiempoEntregaDias} ${p.tiempoEntregaDias === 1 ? 'día' : 'días'}` },
              { clave: 'puntaje', titulo: 'Puntaje', derecha: true, render: (p) => (p.puntaje == null ? <span className="text-sm text-muted-foreground">No participa</span> : <strong>{p.puntaje.toFixed(3).replace('.', ',')}</strong>) },
              ...(esAdmin ? [{
                clave: 'acciones', titulo: <span className="sr-only">Acciones</span>, derecha: true,
                render: (p) => (
                  <span className="inline-flex gap-1">
                    <Button variant="ghost" size="sm" onClick={() => setDialogo({ tipo: 'proveedor', proveedor: p })}><Pencil aria-hidden="true" />Condiciones</Button>
                    <Button variant="ghost" size="sm" className="text-alto-texto hover:bg-alto-tinte" onClick={() => setQuitando(p)}><X aria-hidden="true" />Quitar</Button>
                  </span>
                ),
              }] : []),
            ]}
          />
          {/* El criterio va debajo de la tabla (enunciado Avance 2, 5.6) */}
          <p className="border-t border-border-suave px-6 py-3 text-sm text-muted-foreground">Puntaje = 60 % tiempo de entrega + 40 % precio, cada criterio comparado con el mejor. Solo participan los proveedores activos.</p>
        </BloqueSeccion>
      </div>

      {dialogo?.tipo === 'entrada' && <FormularioEntrada repuesto={r} onCerrar={() => setDialogo(null)} onGuardada={alGuardar} />}
      {dialogo?.tipo === 'proveedor' && (
        <FormularioProveedor repuesto={r} asociado={dialogo.proveedor} yaAsociados={asociaciones} onCerrar={() => setDialogo(null)} onGuardado={alGuardar} />
      )}

      <DialogoConfirmacion
        abierto={Boolean(quitando)}
        titulo={`¿Quitar a ${quitando?.proveedorNombre ?? ''} como proveedor de ${r.codigo}?`}
        mensaje="Se borran sus condiciones (precio y tiempo de entrega) para este repuesto."
        textoConfirmar="Quitar proveedor"
        peligro
        procesando={procesando}
        onCancelar={() => setQuitando(null)}
        onConfirmar={() => ejecutar(
          () => quitarProveedor(r.idRepuesto, quitando.idProveedor),
          `${quitando.proveedorNombre} ya no es proveedor de ${r.codigo}`,
          () => { setQuitando(null); carga.recargar() },
        )}
      />
      <DialogoConfirmacion
        abierto={inactivando}
        titulo={`¿Inactivar el repuesto ${r.codigo}?`}
        mensaje="Deja de aparecer en el inventario y no se puede usar en órdenes nuevas. Sus datos se conservan en las órdenes existentes."
        textoConfirmar="Inactivar"
        peligro
        procesando={procesando}
        onCancelar={() => setInactivando(false)}
        onConfirmar={() => ejecutar(
          () => inactivarRepuesto(r.idRepuesto),
          `Repuesto ${r.codigo} inactivado`,
          () => navigate(volver.to),
        )}
      />
    </>
  )
}

const esquemaEntrada = z.object({ cantidad: numero('la cantidad', { min: 1, max: 1000000 }) })

function FormularioEntrada({ repuesto, onCerrar, onGuardada }) {
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaEntrada),
    defaultValues: { cantidad: '' },
  })
  const enviar = async ({ cantidad }) => {
    try {
      await registrarEntrada(repuesto.idRepuesto, cantidad)
      // Sin la unidad de medida: está en singular ("unidad", "juego") y no concuerda con cantidades mayores que 1
      avisarExito(`Entrada de ${formatNumero(cantidad)} registrada en ${repuesto.codigo}`)
      onGuardada()
    } catch (err) {
      if (!aplicarErroresApi(err, setError, ['cantidad'])) avisarError(err)
    }
  }
  return (
    <DialogoFormulario
      abierto
      titulo={`Registrar entrada de ${repuesto.codigo}`}
      descripcion={`Stock actual: ${formatNumero(repuesto.stockActual)} ${repuesto.unidadMedida}.`}
      textoEnviar="Registrar entrada"
      procesando={isSubmitting}
      onEnviar={handleSubmit(enviar)}
      onCerrar={onCerrar}
    >
      <Controller name="cantidad" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="cantidad" etiqueta="Cantidad que ingresa" requerido unidad={repuesto.unidadMedida} error={fieldState.error?.message}>
          {(props) => <Input {...field} {...props} inputMode="numeric" autoComplete="off" />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

const esquemaProveedor = z.object({
  idProveedor: eleccion('el proveedor'),
  precioOfrecido: numero('el precio ofrecido', { min: 0.01, entero: false }),
  tiempoEntregaDias: numero('el tiempo de entrega', { min: 1, max: 365 }),
})

// Asociar un proveedor nuevo o editar las condiciones de uno ya asociado
function FormularioProveedor({ repuesto, asociado, yaAsociados, onCerrar, onGuardado }) {
  const proveedores = useCarga(listarProveedores)
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquemaProveedor),
    defaultValues: {
      idProveedor: asociado ? String(asociado.idProveedor) : '',
      precioOfrecido: asociado ? String(asociado.precioOfrecido) : '',
      tiempoEntregaDias: asociado ? String(asociado.tiempoEntregaDias) : '',
    },
  })
  const asociados = new Set(yaAsociados.map((a) => a.idProveedor))
  const opciones = asociado
    ? [{ valor: asociado.idProveedor, texto: asociado.proveedorNombre }]
    : (proveedores.datos?.data ?? []).filter((p) => !asociados.has(p.idProveedor)).map((p) => ({ valor: p.idProveedor, texto: p.nombre }))

  const enviar = async (datos) => {
    try {
      if (asociado) {
        await actualizarCondiciones(repuesto.idRepuesto, asociado.idProveedor, { precioOfrecido: datos.precioOfrecido, tiempoEntregaDias: datos.tiempoEntregaDias })
        avisarExito(`Condiciones de ${asociado.proveedorNombre} actualizadas`)
      } else {
        await asociarProveedor(repuesto.idRepuesto, datos)
        avisarExito(`${opciones.find((o) => o.valor === datos.idProveedor)?.texto} agregado como proveedor de ${repuesto.codigo}`)
      }
      onGuardado()
    } catch (err) {
      if (err.status === 409) setError('idProveedor', { message: err.message }, { shouldFocus: true })
      else if (!aplicarErroresApi(err, setError, ['idProveedor', 'precioOfrecido', 'tiempoEntregaDias'])) avisarError(err)
    }
  }

  return (
    <DialogoFormulario
      abierto
      titulo={asociado ? `Condiciones de ${asociado.proveedorNombre}` : `Agregar proveedor a ${repuesto.codigo}`}
      textoEnviar={asociado ? 'Guardar condiciones' : 'Agregar proveedor'}
      procesando={isSubmitting}
      onEnviar={handleSubmit(enviar)}
      onCerrar={onCerrar}
    >
      {proveedores.error && <EstadoError error={proveedores.error} />}
      <Controller name="idProveedor" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idProveedor" etiqueta="Proveedor" requerido error={fieldState.error?.message}
          ayuda={!asociado && !proveedores.cargando && opciones.length === 0 ? 'Todos los proveedores activos ya están asociados.' : undefined}>
          {(props) => <ListaOpciones {...props} ref={field.ref} value={field.value} onChange={field.onChange} opciones={opciones}
            disabled={Boolean(asociado) || proveedores.cargando} placeholder={proveedores.cargando ? 'Cargando…' : 'Elija un proveedor'} />}
        </CampoFormulario>
      )} />
      <div className="grid gap-4 sm:grid-cols-2">
        <Controller name="precioOfrecido" control={control} render={({ field, fieldState }) => (
          <CampoFormulario id="precioOfrecido" etiqueta="Precio ofrecido (₡)" requerido error={fieldState.error?.message}>
            {(props) => <Input {...field} {...props} inputMode="decimal" autoComplete="off" />}
          </CampoFormulario>
        )} />
        <Controller name="tiempoEntregaDias" control={control} render={({ field, fieldState }) => (
          <CampoFormulario id="tiempoEntregaDias" etiqueta="Tiempo de entrega" requerido unidad="días" error={fieldState.error?.message}>
            {(props) => <Input {...field} {...props} inputMode="numeric" autoComplete="off" />}
          </CampoFormulario>
        )} />
      </div>
    </DialogoFormulario>
  )
}

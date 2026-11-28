import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { ClipboardPlus, Gauge, Loader2 } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import BarraPreventivo, { LeyendaPreventivo } from '@/components/shared/BarraPreventivo'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoVacio from '@/components/shared/EstadoVacio'
import ListaOpciones from '@/components/shared/ListaOpciones'
import TablaPaginada from '@/components/shared/TablaPaginada'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import useCarga from '@/hooks/useCarga'
import useFiltrosUrl from '@/hooks/useFiltrosUrl'
import useRol from '@/hooks/useRol'
import { cn } from '@/lib/utils'
import { eleccion, textoOpcional } from '@/schemas/comunes'
import { tiposMantenimiento } from '@/services/catalogoService'
import { alertasPreventivo, evaluarPreventivo, generarOrdenPreventiva } from '@/services/preventivoService'
import { avisarError, avisarExito } from '@/utils/avisos'
import { aplicarErroresApi } from '@/utils/errores'
import { formatFechaHora, formatKm, formatNumero } from '@/utils/format'

const ESTADOS = ['Vencido', 'Próximo', 'Al día', 'Sin lecturas']

// Cada evaluación recuerda cuándo se hizo, para mostrar "Última evaluación"
const evaluarFlota = async () => ({ ...(await evaluarPreventivo()), evaluadoEn: new Date() })
const evaluarAlertas = async () => ({ ...(await alertasPreventivo()), evaluadoEn: new Date() })

// Mantenimiento preventivo (enunciado Avance 2, 5.8; requerimientos 7.9). El Administrador evalúa la flota y genera la orden
// sugerida; el Coordinador/Técnico consulta las alertas (Vencidos y Próximos). Los porcentajes, estados y la sugerencia de
// orden los calcula la API en cada evaluación: aquí solo se muestran y se filtran.
export default function Preventivo() {
  const { esAdmin } = useRol()
  const navigate = useNavigate()
  const { search } = useLocation()
  const filtros = useFiltrosUrl()
  const elegidos = filtros.valores('estado')
  const evaluacion = useCarga(esAdmin ? evaluarFlota : evaluarAlertas)
  const [generando, setGenerando] = useState(null)

  const todos = evaluacion.datos?.data ?? []
  const filas = elegidos.length ? todos.filter((v) => elegidos.includes(v.estado)) : todos
  const visibles = esAdmin ? ESTADOS : ESTADOS.slice(0, 2)
  const cantidad = (e) => todos.filter((v) => v.estado === e).length
  const todoAlDia = Boolean(evaluacion.datos) && cantidad('Vencido') + cantidad('Próximo') === 0
  const alternar = (e) => filtros.cambiar('estado', elegidos.includes(e) ? elegidos.filter((x) => x !== e) : [...elegidos, e])
  const desde = { desde: `/preventivo${search}` }

  return (
    <>
      <EncabezadoPagina
        titulo="Mantenimiento preventivo"
        detalle={esAdmin
          ? 'Evalúa la flota según su kilometraje real, del mayor al menor porcentaje del umbral consumido.'
          : 'Los vehículos vencidos o próximos a su mantenimiento preventivo, según su kilometraje.'}
        acciones={
          <span className="flex flex-wrap items-center gap-3">
            {evaluacion.datos && <span className="text-sm text-muted-foreground tabular-nums">Última evaluación: {formatFechaHora(evaluacion.datos.evaluadoEn)}</span>}
            <Button onClick={evaluacion.recargar} disabled={evaluacion.cargando}>
              {evaluacion.cargando ? <Loader2 className="animate-spin" aria-hidden="true" /> : <Gauge aria-hidden="true" />}
              {evaluacion.cargando ? 'Evaluando…' : 'Evaluar'}
            </Button>
          </span>
        }
      />

      {/* Tarjetas resumen con la cantidad de cada estado; al tocarlas filtran la tabla */}
      <div role="group" aria-label="Resumen por estado preventivo" className="grid grid-cols-[repeat(auto-fit,minmax(11rem,1fr))] gap-3">
        {visibles.map((e) => {
          const activo = elegidos.includes(e)
          return (
            <button key={e} type="button" aria-pressed={activo} onClick={() => alternar(e)}
              className={cn(
                'flex flex-col items-start gap-2 rounded-sm border bg-card px-4 py-3 text-left transition-colors hover:bg-muted/40',
                activo ? 'border-primary ring-2 ring-primary/25' : 'border-border',
              )}>
              <span className="text-3xl leading-none font-bold tabular-nums">{evaluacion.datos ? formatNumero(cantidad(e)) : '–'}</span>
              <BadgeEstado estado={e} />
            </button>
          )
        })}
      </div>
      {todoAlDia && esAdmin && <EstadoVacio tipo="positivo" titulo="Todos los activos están al día según su uso registrado" className="bg-card" />}

      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
        <LeyendaPreventivo className="self-center" />
      </BarraFiltros>
      <TablaPaginada
        titulo="Resultado de la evaluación del mantenimiento preventivo"
        nombre="vehículos"
        filas={filas}
        claveFila={(v) => v.idActivo}
        cargando={evaluacion.cargando}
        error={evaluacion.error}
        onReintentar={evaluacion.recargar}
        vacio={elegidos.length
          ? { titulo: `No hay vehículos ${elegidos.length === 1 ? 'en estado' : 'en los estados'} ${elegidos.join(', ')}`, tipo: 'sinResultados', accion: <Button variant="outline" onClick={filtros.limpiar}>Limpiar filtros</Button> }
          : { titulo: 'Todos los activos están al día según su uso registrado', tipo: 'positivo' }}
        columnas={[
          {
            clave: 'vehiculo', titulo: 'Vehículo y categoría', fija: true,
            render: (v) => (
              <Link to={`/vehiculos/${v.idActivo}`} state={desde} className="flex w-32 flex-col whitespace-normal underline-offset-4 hover:underline">
                <strong className="text-primary">{v.codigo}</strong>
                <span className="text-sm">{v.nombre}</span>
                <span className="text-sm text-muted-foreground">{v.categoriaNombre}</span>
              </Link>
            ),
          },
          { clave: 'variable', titulo: 'Variable', render: (v) => <span className="flex flex-col"><span>{v.variableControlNombre}</span><span className="text-sm text-muted-foreground">en {v.variableControlUnidad}</span></span> },
          {
            // Lectura base (último preventivo) y lectura actual: la diferencia es el uso acumulado de la columna siguiente
            clave: 'lecturas', titulo: 'Lecturas', derecha: true,
            render: (v) => (
              <span className="grid grid-cols-[auto_auto] justify-end gap-x-2 text-sm">
                <span className="text-muted-foreground">Base</span><span>{formatKm(v.kilometrajeUltimoPreventivo, v.variableControlUnidad)}</span>
                <span className="text-muted-foreground">Actual</span>
                <span>{v.estado === 'Sin lecturas' ? 'Sin lecturas' : formatKm(v.kilometrajeActual, v.variableControlUnidad)}</span>
              </span>
            ),
          },
          {
            clave: 'uso', titulo: 'Uso acumulado / umbral', // el umbral propio del vehículo, si lo tiene, ya viene aplicado por la API
            render: (v) => (
              <span className="flex flex-col gap-1">
                <BarraPreventivo porcentaje={v.estado === 'Sin lecturas' ? null : v.porcentajeConsumido} porcentajeAviso={v.porcentajeAviso} estado={v.estado}
                  recorrido={v.kmRecorridos} umbral={v.intervaloKm} unidad={v.variableControlUnidad} className="min-w-36" />
              </span>
            ),
          },
          { clave: 'estado', titulo: 'Estado', render: (v) => <BadgeEstado estado={v.estado} /> },
          {
            // Generar la orden preventiva (solo Administrador) o el enlace a la que ya está abierta
            clave: 'situacion', titulo: 'Orden preventiva',
            render: (v) => (v.situacion === 'Orden en curso'
              ? <Link to={`/ordenes/${v.idOrdenEnCurso}`} state={desde} className="font-bold whitespace-nowrap text-primary underline-offset-4 hover:underline">{v.codigoOrdenEnCurso}</Link>
              : v.situacion
                ? <BadgeEstado estado={v.situacion} />
                : v.seSugiereOrden && esAdmin
                  ? <Button size="sm" variant="outline" className="h-auto w-40 justify-start py-1.5 text-left whitespace-normal" onClick={() => setGenerando(v)}><ClipboardPlus aria-hidden="true" />Generar orden preventiva</Button>
                  : <span className="text-sm text-muted-foreground">{v.seSugiereOrden ? 'Se sugiere orden' : '—'}</span>),
          },
        ]}
      />
      {generando && (
        <DialogoGenerar
          vehiculo={generando}
          onCerrar={() => setGenerando(null)}
          onGenerada={(orden) => navigate(`/ordenes/${orden.idOrden}`, { state: desde })}
        />
      )}
    </>
  )
}

const esquema = z.object({ idTipoMantenimiento: eleccion('el tipo de mantenimiento'), motivo: textoOpcional(500) })

// El sistema sugiere y el Administrador confirma (requerimientos 7.9). La prioridad y el motivo por defecto los pone la API.
function DialogoGenerar({ vehiculo, onCerrar, onGenerada }) {
  const tipos = useCarga(tiposMantenimiento)
  const preventivos = (tipos.datos?.data ?? []).filter((t) => t.naturalezaNombre === 'Preventivo')
  const { control, handleSubmit, setError, formState: { isSubmitting } } = useForm({
    resolver: zodResolver(esquema),
    values: { idTipoMantenimiento: preventivos.length === 1 ? String(preventivos[0].idTipoMantenimiento) : '', motivo: '' },
    resetOptions: { keepDirtyValues: true },
  })
  const enviar = async (datos) => {
    try {
      const { data } = await generarOrdenPreventiva(vehiculo.idActivo, datos)
      avisarExito(`Orden ${data.codigo} generada para ${vehiculo.codigo}`)
      onGenerada(data)
    } catch (err) {
      if (!aplicarErroresApi(err, setError, ['idTipoMantenimiento', 'motivo'])) avisarError(err)
    }
  }
  return (
    <DialogoFormulario abierto titulo={`Generar la orden preventiva de ${vehiculo.codigo}`}
      descripcion={`${vehiculo.estado}: ${formatNumero(vehiculo.kmRecorridos)} de ${formatNumero(vehiculo.intervaloKm)} ${vehiculo.variableControlUnidad}. La orden nace Solicitada con prioridad ${vehiculo.estado === 'Vencido' ? 'Alta' : 'Media'}.`}
      textoEnviar="Generar orden" procesando={isSubmitting} onEnviar={handleSubmit(enviar)} onCerrar={onCerrar}>
      <Controller name="idTipoMantenimiento" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="idTipoMantenimiento" etiqueta="Tipo de mantenimiento" requerido error={fieldState.error?.message}>
          {(p) => <ListaOpciones {...p} ref={field.ref} value={field.value} onChange={field.onChange} disabled={tipos.cargando}
            opciones={preventivos.map((t) => ({ valor: t.idTipoMantenimiento, texto: t.nombre }))} />}
        </CampoFormulario>
      )} />
      <Controller name="motivo" control={control} render={({ field, fieldState }) => (
        <CampoFormulario id="motivo" etiqueta="Motivo (opcional)" ayuda="Si lo deja vacío, se arma con la lectura y el porcentaje." error={fieldState.error?.message}>
          {(p) => <Textarea {...field} {...p} rows={3} maxLength={500} />}
        </CampoFormulario>
      )} />
    </DialogoFormulario>
  )
}

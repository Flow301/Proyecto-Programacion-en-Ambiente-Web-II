import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { ClipboardPlus } from 'lucide-react'
import BadgeEstado from '@/components/shared/BadgeEstado'
import BarraFiltros from '@/components/shared/BarraFiltros'
import BarraPreventivo, { LeyendaPreventivo } from '@/components/shared/BarraPreventivo'
import CampoFormulario from '@/components/shared/CampoFormulario'
import DialogoFormulario from '@/components/shared/DialogoFormulario'
import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import ListaOpciones from '@/components/shared/ListaOpciones'
import Separador from '@/components/shared/Separador'
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
import { formatFecha, formatNumero } from '@/utils/format'

const ESTADOS = ['Vencido', 'Próximo', 'Al día', 'Sin lecturas']

// Mantenimiento preventivo (enunciado Avance 2, 5.5; requerimientos 7.9). El Administrador ve la evaluación de toda la
// flota y genera la orden sugerida; el Coordinador/Técnico consulta las alertas (Vencidos y Próximos).
// Todo se calcula en la API en cada consulta: aquí solo se muestra, se filtra y se cuenta.
export default function Preventivo() {
  const { esAdmin } = useRol()
  const navigate = useNavigate()
  const filtros = useFiltrosUrl()
  const elegidos = filtros.valores('estado')
  const evaluacion = useCarga(esAdmin ? evaluarPreventivo : alertasPreventivo)
  const [generando, setGenerando] = useState(null)

  const todos = evaluacion.datos?.data ?? []
  const filas = elegidos.length ? todos.filter((v) => elegidos.includes(v.estado)) : todos
  const visibles = esAdmin ? ESTADOS : ESTADOS.slice(0, 2)
  const alternar = (e) => filtros.cambiar('estado', elegidos.includes(e) ? elegidos.filter((x) => x !== e) : [...elegidos, e])

  return (
    <>
      <EncabezadoPagina
        titulo="Mantenimiento preventivo"
        detalle={esAdmin
          ? 'La flota evaluada según su kilometraje, del mayor al menor porcentaje del intervalo consumido. Se calcula en cada consulta.'
          : 'Los vehículos vencidos o próximos a su mantenimiento preventivo, según su kilometraje.'}
      />
      <BarraFiltros onLimpiar={filtros.limpiar} hayFiltros={filtros.hayFiltros}>
        <fieldset className="flex min-w-0 flex-col gap-1.5">
          <legend className="mb-1.5 text-sm font-semibold">Estado preventivo</legend>
          <div className="flex flex-wrap gap-1.5">
            {visibles.map((e) => {
              const activo = elegidos.includes(e)
              return (
                <button key={e} type="button" aria-pressed={activo} onClick={() => alternar(e)}
                  className={cn(
                    'h-9 rounded-sm border px-3 text-sm font-semibold tabular-nums transition-colors',
                    activo ? 'border-primary bg-accent text-accent-foreground' : 'border-border bg-card text-muted-foreground hover:border-input hover:text-foreground',
                  )}>
                  {e} <span className="font-normal">({formatNumero(todos.filter((v) => v.estado === e).length)})</span>
                </button>
              )
            })}
          </div>
        </fieldset>
        <LeyendaPreventivo className="self-end pb-2" />
      </BarraFiltros>
      <TablaPaginada
        titulo="Evaluación del mantenimiento preventivo"
        nombre="vehículos"
        filas={filas}
        claveFila={(v) => v.idActivo}
        cargando={evaluacion.cargando}
        error={evaluacion.error}
        onReintentar={evaluacion.recargar}
        vacio={elegidos.length
          ? { titulo: `No hay vehículos ${elegidos.length === 1 ? 'en estado' : 'en los estados'} ${elegidos.join(', ')}`, tipo: 'sinResultados', accion: <Button variant="outline" onClick={filtros.limpiar}>Limpiar filtros</Button> }
          : { titulo: 'Todos los vehículos están al día según su uso registrado', tipo: 'positivo' }}
        columnas={[
          {
            clave: 'vehiculo', titulo: 'Vehículo', fija: true,
            render: (v) => (
              <span className="flex flex-col">
                <span className="whitespace-nowrap"><strong>{v.codigo}</strong> <Separador /> {v.placa}</span>
                <span className="text-sm text-muted-foreground">{v.categoriaNombre}</span>
              </span>
            ),
          },
          {
            clave: 'uso', titulo: 'Uso desde el último preventivo',
            render: (v) => (
              <span className="flex flex-col gap-1">
                <BarraPreventivo porcentaje={v.estado === 'Sin lecturas' ? null : v.porcentajeConsumido} porcentajeAviso={v.porcentajeAviso} estado={v.estado}
                  recorrido={v.kmRecorridos} umbral={v.intervaloKm} unidad={v.variableControlUnidad} className="min-w-56" />
                <span className="text-sm text-muted-foreground">
                  {v.intervaloPropio ? 'Intervalo propio del vehículo' : 'Intervalo de la categoría'}
                  {v.fechaUltimaLectura && <> <Separador /> Última lectura {formatFecha(v.fechaUltimaLectura)}</>}
                </span>
              </span>
            ),
          },
          { clave: 'estado', titulo: 'Estado', render: (v) => <BadgeEstado estado={v.estado} /> },
          {
            clave: 'situacion', titulo: 'Situación',
            render: (v) => (v.situacion === 'Orden en curso'
              ? <Link to={`/ordenes/${v.idOrdenEnCurso}`} className="font-bold text-primary underline-offset-4 hover:underline">{v.codigoOrdenEnCurso}</Link>
              : v.situacion
                ? <BadgeEstado estado={v.situacion} />
                : v.seSugiereOrden && esAdmin
                  ? <Button size="sm" variant="outline" onClick={() => setGenerando(v)}><ClipboardPlus aria-hidden="true" />Generar orden</Button>
                  : <span className="text-sm text-muted-foreground">{v.seSugiereOrden ? 'Se sugiere orden' : '—'}</span>),
          },
        ]}
      />
      {generando && (
        <DialogoGenerar
          vehiculo={generando}
          onCerrar={() => setGenerando(null)}
          onGenerada={(orden) => navigate(`/ordenes/${orden.idOrden}`, { state: { desde: '/preventivo' } })}
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
          {(p) => <ListaOpciones {...p} value={field.value} onChange={field.onChange} disabled={tipos.cargando}
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

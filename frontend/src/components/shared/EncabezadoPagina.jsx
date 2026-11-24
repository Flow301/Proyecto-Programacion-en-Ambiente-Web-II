import { useEffect, useRef } from 'react'
import { Link, useLocation } from 'react-router'
import { ArrowLeft } from 'lucide-react'
import { cn } from '@/lib/utils'

/**
 * Encabezado único de cada pantalla: regreso, título (único h1), detalle y acciones siempre a la derecha.
 * - volver: { to, texto } para regresar al listado conservando su filtro (to puede llevar ?query).
 * - codigo: en un detalle, el código del registro es el titular (OM-2026-00006, PKP-0003).
 * - estado: el badge que acompaña al título.
 * - pestana: texto del <title> de la pestaña ("Orden OM-2026-00006"); por defecto, el título.
 * Al cambiar de pantalla el foco va al título, para que el lector de pantalla anuncie dónde está.
 */
export default function EncabezadoPagina({ titulo, codigo, estado, detalle, volver, acciones, pestana, className }) {
  const tituloRef = useRef(null)
  const { pathname } = useLocation()

  useEffect(() => {
    tituloRef.current?.focus({ preventScroll: true })
  }, [pathname])

  return (
    <header className={cn('flex flex-col gap-3', className)}>
      <title>{`${pestana ?? codigo ?? titulo} · SGMA`}</title>
      {volver && (
        <Link to={volver.to} className="inline-flex w-fit items-center gap-1.5 text-[0.9375rem] text-primary underline-offset-4 hover:underline">
          <ArrowLeft className="size-4" aria-hidden="true" />
          {volver.texto}
        </Link>
      )}
      <div className="flex flex-wrap items-end gap-x-6 gap-y-4">
        <div className="flex min-w-0 flex-[1_1_28rem] flex-col gap-1.5">
          <div className="flex flex-wrap items-center gap-x-3.5 gap-y-2">
            <h1
              ref={tituloRef}
              tabIndex={-1}
              className={cn('font-bold', codigo ? 'text-3xl tabular-nums' : 'text-2xl')}
            >
              {codigo ?? titulo}
            </h1>
            {estado}
          </div>
          {detalle && <div className="text-base text-muted-foreground">{detalle}</div>}
        </div>
        {acciones && (
          <div role="toolbar" aria-label="Acciones" className="flex flex-wrap items-center gap-2">
            {acciones}
          </div>
        )}
      </div>
    </header>
  )
}

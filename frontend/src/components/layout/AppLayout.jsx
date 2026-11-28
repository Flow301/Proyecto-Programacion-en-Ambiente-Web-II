import { Suspense, useEffect, useState } from 'react'
import { Link, NavLink, Outlet } from 'react-router'
import { LogOut, PanelLeftClose, PanelLeftOpen } from 'lucide-react'
import IndicadorCarga from '@/components/shared/IndicadorCarga'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/context/AuthContext'
import { cn } from '@/lib/utils'
import { menuDelRol } from './menu'

const CLAVE_MENU = 'sgma.menuOculto'
const ANGOSTA = '(max-width: 1023px)'

function leerPreferencia() {
  try {
    const guardado = localStorage.getItem(CLAVE_MENU)
    return guardado === null ? null : guardado === '1'
  } catch {
    return null // sin almacenamiento, se decide por el ancho
  }
}

// Estructura base común (enunciado Avance 2, 4.2): barra superior con el sistema, el usuario, su rol y
// "Cerrar sesión"; menú lateral colapsable por rol con la sección activa; área de contenido.
export default function AppLayout() {
  const { usuario, cerrarSesion } = useAuth()
  // Lo que el usuario eligió; si nunca eligió, el menú se colapsa en pantallas angostas (tablet)
  // para dejarle espacio a las tablas y a la ruta de la orden, también al cambiar el tamaño de la ventana
  const [oculto, setOculto] = useState(() => leerPreferencia() ?? window.matchMedia(ANGOSTA).matches)
  const grupos = menuDelRol(usuario?.rolNombre)

  useEffect(() => {
    const consulta = window.matchMedia(ANGOSTA)
    const alCambiar = (e) => leerPreferencia() === null && setOculto(e.matches)
    consulta.addEventListener('change', alCambiar)
    return () => consulta.removeEventListener('change', alCambiar)
  }, [])

  const alternarMenu = () => {
    const siguiente = !oculto
    setOculto(siguiente)
    try {
      localStorage.setItem(CLAVE_MENU, siguiente ? '1' : '0')
    } catch {
      // sin almacenamiento, el menú solo recuerda su estado mientras la página está abierta
    }
  }

  return (
    <div className="flex min-h-svh flex-col">
      <a
        href="#contenido"
        className="saltar sr-only z-50 rounded-sm bg-card font-semibold text-primary focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:px-4 focus:py-2"
      >
        Saltar al contenido
      </a>

      {/* La barra queda fija arriba: usuario, rol y "Cerrar sesión" siempre a la vista (enunciado 5.1) */}
      <header className="sobre-azul sticky top-0 z-30 flex h-14 shrink-0 items-center gap-4 bg-primary px-6 text-primary-foreground">
        <span className="text-xl font-bold tracking-[0.06em]">SGMA</span>
        {/* En tablet el subtítulo cede su espacio al usuario y a las acciones, que no se parten en dos líneas */}
        <span className="hidden border-l border-primary-foreground/40 pl-4 text-[0.9375rem] whitespace-nowrap lg:inline">Mantenimiento de flota</span>
        <span className="flex-1" />
        <span className="text-right text-[0.9375rem] leading-tight">
          <strong className="block font-semibold">{usuario?.nombreCompleto}</strong>
          <span>{usuario?.rolNombre}</span>
        </span>
        <Link to="/cuenta/clave" className="rounded-sm px-2 py-1.5 text-[0.9375rem] whitespace-nowrap underline underline-offset-4 hover:no-underline">
          Cambiar contraseña
        </Link>
        <Button
          variant="ghost"
          size="sm"
          className="border-primary-foreground/70 text-primary-foreground hover:bg-primary-foreground/15"
          onClick={() => cerrarSesion()}
        >
          <LogOut aria-hidden="true" />
          Cerrar sesión
        </Button>
      </header>

      <div className="flex flex-1">
        {/* Mide la ventana menos la barra, así el menú entero y su botón siempre se ven */}
        <nav
          aria-label="Menú principal"
          className={cn('sticky top-14 flex h-[calc(100svh-3.5rem)] shrink-0 flex-col self-start overflow-y-auto border-r bg-card px-3 py-3', oculto ? 'w-[68px]' : 'w-60')}
        >
          <Button
            variant="ghost"
            className="mb-1 justify-start gap-3 px-3 font-normal text-muted-foreground hover:bg-muted aria-expanded:bg-transparent aria-expanded:hover:bg-muted"
            aria-expanded={!oculto}
            aria-label={oculto ? 'Mostrar menú' : undefined}
            title={oculto ? 'Mostrar menú' : undefined}
            onClick={alternarMenu}
          >
            {oculto ? <PanelLeftOpen className="size-5" aria-hidden="true" /> : <PanelLeftClose className="size-5" aria-hidden="true" />}
            {!oculto && 'Ocultar menú'}
          </Button>
          {grupos.map(({ grupo, items }, i) => (
            <div key={grupo ?? i} className="flex flex-col gap-0.5">
              {grupo && (
                oculto
                  ? <hr className="mx-2 my-3 border-border-suave" />
                  : <p className="mx-3 mt-5 mb-1.5 text-[0.8125rem] font-bold tracking-[0.06em] text-muted-foreground uppercase">{grupo}</p>
              )}
              {items.map(({ to, texto, Icono, fin }) => (
                <NavLink
                  key={texto}
                  to={to}
                  end={fin}
                  title={oculto ? texto : undefined}
                  aria-label={oculto ? texto : undefined}
                  className={({ isActive }) =>
                    cn(
                      'flex h-10 items-center gap-3 rounded-sm px-3 text-[0.9375rem] text-foreground transition-colors hover:bg-muted',
                      isActive && 'bg-accent font-bold text-accent-foreground hover:bg-accent',
                    )
                  }
                >
                  <Icono className="size-5 shrink-0" aria-hidden="true" />
                  {!oculto && texto}
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        <main id="contenido" tabIndex={-1} className="flex min-w-0 flex-1 flex-col gap-5 px-8 pt-6 pb-12">
          {/* Mientras se descarga una pantalla, el menú y la barra quedan quietos y el contenido muestra su esqueleto */}
          <Suspense fallback={<IndicadorCarga forma="detalle" texto="Cargando la pantalla…" />}>
            <Outlet />
          </Suspense>
        </main>
      </div>
    </div>
  )
}

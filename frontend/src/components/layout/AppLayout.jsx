import { useState } from 'react'
import { NavLink, Outlet } from 'react-router'
import { LogOut, PanelLeftClose, PanelLeftOpen } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/context/AuthContext'
import { cn } from '@/lib/utils'
import { menuDelRol } from './menu'

const CLAVE_MENU = 'sgma.menuOculto'

function leerMenuOculto() {
  try {
    return localStorage.getItem(CLAVE_MENU) === '1'
  } catch {
    return false
  }
}

// Estructura base común (enunciado Avance 2, 4.2): barra superior con el sistema, el usuario, su rol y
// "Cerrar sesión"; menú lateral colapsable por rol con la sección activa; área de contenido.
export default function AppLayout() {
  const { usuario, cerrarSesion } = useAuth()
  const [oculto, setOculto] = useState(leerMenuOculto)
  const grupos = menuDelRol(usuario?.rolNombre)

  const alternarMenu = () => {
    setOculto((actual) => {
      try {
        localStorage.setItem(CLAVE_MENU, actual ? '0' : '1')
      } catch {
        // sin almacenamiento, el menú solo recuerda su estado mientras la página está abierta
      }
      return !actual
    })
  }

  return (
    <div className="flex min-h-svh flex-col">
      <a
        href="#contenido"
        className="sr-only z-50 rounded-sm bg-card px-4 py-2 font-semibold text-primary focus:not-sr-only focus:fixed focus:top-2 focus:left-2"
      >
        Saltar al contenido
      </a>

      <header className="sobre-azul flex h-14 shrink-0 items-center gap-4 bg-primary px-6 text-primary-foreground">
        <span className="text-xl font-bold tracking-[0.06em]">SGMA</span>
        <span className="border-l border-primary-foreground/40 pl-4 text-[0.9375rem]">Mantenimiento de flota</span>
        <span className="flex-1" />
        <span className="text-right text-[0.9375rem] leading-tight">
          <strong className="block font-semibold">{usuario?.nombreCompleto}</strong>
          <span>{usuario?.rolNombre}</span>
        </span>
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
        <nav
          aria-label="Menú principal"
          className={cn('flex shrink-0 flex-col border-r bg-card px-3 py-3 transition-[width] duration-150', oculto ? 'w-[68px]' : 'w-60')}
        >
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
          <span className="flex-1" />
          <Button
            variant="ghost"
            className="mt-6 justify-start gap-3 px-3 font-normal text-muted-foreground"
            aria-expanded={!oculto}
            aria-label={oculto ? 'Mostrar menú' : undefined}
            title={oculto ? 'Mostrar menú' : undefined}
            onClick={alternarMenu}
          >
            {oculto ? <PanelLeftOpen className="size-5" aria-hidden="true" /> : <PanelLeftClose className="size-5" aria-hidden="true" />}
            {!oculto && 'Ocultar menú'}
          </Button>
        </nav>

        <main id="contenido" tabIndex={-1} className="flex min-w-0 flex-1 flex-col gap-5 px-8 pt-6 pb-12">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

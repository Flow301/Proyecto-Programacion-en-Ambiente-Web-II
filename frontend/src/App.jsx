import { lazy, Suspense } from 'react'
import { Route, Routes } from 'react-router'
import AppLayout from './components/layout/AppLayout'
import { ROLES } from './components/layout/menu'
import RutaProtegida from './components/RutaProtegida'
import PaginaError from './components/shared/PaginaError'
import Inicio from './pages/Inicio'
import Login from './pages/Login'
import Ordenes from './pages/Ordenes'
import PaginaPendiente from './pages/PaginaPendiente'

const { administrador: A, coordinador: C } = ROLES

// Solo en desarrollo: en el build de producción la rama se elimina y el muestrario no se incluye
const Muestrario = import.meta.env.DEV ? lazy(() => import('./pages/Muestrario')) : null

// Rutas por rol (enunciado general, sección 11). La API valida el rol en cada endpoint;
// aquí se evita mostrar pantallas que el rol no puede usar (403 si se escribe la ruta a mano).
export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<RutaProtegida />}>
        <Route element={<AppLayout />}>
          <Route index element={<Inicio />} />
          <Route path="ordenes" element={<Ordenes />} />
          <Route path="vehiculos" element={<PaginaPendiente titulo="Vehículos" />} />
          <Route element={<RutaProtegida roles={[A, C]} />}>
            <Route path="preventivo" element={<PaginaPendiente titulo="Mantenimiento preventivo" />} />
            <Route path="repuestos" element={<PaginaPendiente titulo="Repuestos" />} />
            <Route path="categorias" element={<PaginaPendiente titulo="Categorías" />} />
            <Route path="tecnicos" element={<PaginaPendiente titulo="Técnicos" />} />
            <Route path="especialidades" element={<PaginaPendiente titulo="Especialidades" />} />
            <Route path="proveedores" element={<PaginaPendiente titulo="Proveedores" />} />
            {Muestrario && <Route path="muestrario" element={<Suspense><Muestrario /></Suspense>} />}
          </Route>
          <Route element={<RutaProtegida roles={[A]} />}>
            <Route path="usuarios" element={<PaginaPendiente titulo="Usuarios" />} />
          </Route>
          <Route path="*" element={<PaginaError codigo={404} />} />
        </Route>
      </Route>
    </Routes>
  )
}

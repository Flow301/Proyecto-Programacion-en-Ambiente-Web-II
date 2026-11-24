import { lazy } from 'react'
import { Route, Routes } from 'react-router'
import AppLayout from './components/layout/AppLayout'
import { ROLES } from './components/layout/menu'
import RutaProtegida from './components/RutaProtegida'
import PaginaError from './components/shared/PaginaError'
import Inicio from './pages/Inicio'
import Login from './pages/Login'

// Cada pantalla se descarga cuando se abre por primera vez (AppLayout muestra un esqueleto mientras tanto)
const CambiarClave = lazy(() => import('./pages/CambiarClave'))
const Categorias = lazy(() => import('./pages/categorias/Categorias'))
const CategoriaFormulario = lazy(() => import('./pages/categorias/CategoriaFormulario'))
const Especialidades = lazy(() => import('./pages/especialidades/Especialidades'))
const Ordenes = lazy(() => import('./pages/Ordenes'))
const OrdenDetalle = lazy(() => import('./pages/ordenes/OrdenDetalle'))
const OrdenNueva = lazy(() => import('./pages/ordenes/OrdenNueva'))
const Preventivo = lazy(() => import('./pages/Preventivo'))
const ReporteCostos = lazy(() => import('./pages/ordenes/ReporteCostos'))
const Proveedores = lazy(() => import('./pages/proveedores/Proveedores'))
const RepuestoDetalle = lazy(() => import('./pages/repuestos/RepuestoDetalle'))
const RepuestoFormulario = lazy(() => import('./pages/repuestos/RepuestoFormulario'))
const Repuestos = lazy(() => import('./pages/repuestos/Repuestos'))
const TecnicoDetalle = lazy(() => import('./pages/tecnicos/TecnicoDetalle'))
const TecnicoFormulario = lazy(() => import('./pages/tecnicos/TecnicoFormulario'))
const Usuarios = lazy(() => import('./pages/usuarios/Usuarios'))
const VehiculoDetalle = lazy(() => import('./pages/vehiculos/VehiculoDetalle'))
const Vehiculos = lazy(() => import('./pages/vehiculos/Vehiculos'))
const Tecnicos = lazy(() => import('./pages/tecnicos/Tecnicos'))

const { administrador: A, coordinador: C, solicitante: S } = ROLES

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
          <Route path="cuenta/clave" element={<CambiarClave />} />
          <Route path="ordenes" element={<Ordenes />} />
          <Route path="ordenes/:id" element={<OrdenDetalle />} />
          <Route path="vehiculos" element={<Vehiculos />} />
          <Route path="vehiculos/:id" element={<VehiculoDetalle />} />

          {/* Registrar solicitudes: Administrador y Solicitante/Consulta */}
          <Route element={<RutaProtegida roles={[A, S]} />}>
            <Route path="ordenes/nueva" element={<OrdenNueva />} />
          </Route>

          {/* Consulta: Administrador y Coordinador/Técnico */}
          <Route element={<RutaProtegida roles={[A, C]} />}>
            <Route path="ordenes/:id/costos" element={<ReporteCostos />} />
            <Route path="preventivo" element={<Preventivo />} />
            <Route path="categorias" element={<Categorias />} />
            <Route path="especialidades" element={<Especialidades />} />
            <Route path="proveedores" element={<Proveedores />} />
            <Route path="tecnicos" element={<Tecnicos />} />
            <Route path="tecnicos/:id" element={<TecnicoDetalle />} />
            <Route path="repuestos" element={<Repuestos />} />
            <Route path="repuestos/:id" element={<RepuestoDetalle />} />
            {Muestrario && <Route path="muestrario" element={<Muestrario />} />}
          </Route>

          {/* Escritura de catálogos: solo Administrador */}
          <Route element={<RutaProtegida roles={[A]} />}>
            <Route path="categorias/nueva" element={<CategoriaFormulario />} />
            <Route path="categorias/:id/editar" element={<CategoriaFormulario />} />
            <Route path="tecnicos/nuevo" element={<TecnicoFormulario />} />
            <Route path="tecnicos/:id/editar" element={<TecnicoFormulario />} />
            <Route path="repuestos/nuevo" element={<RepuestoFormulario />} />
            <Route path="repuestos/:id/editar" element={<RepuestoFormulario />} />
            <Route path="usuarios" element={<Usuarios />} />
          </Route>

          <Route path="*" element={<PaginaError codigo={404} />} />
        </Route>
      </Route>
    </Routes>
  )
}

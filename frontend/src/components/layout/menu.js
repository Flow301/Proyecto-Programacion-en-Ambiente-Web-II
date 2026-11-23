import { ClipboardList, Gauge, House, Layers, Package, Store, Truck, UserCog, Users, Wrench } from 'lucide-react'

// Nombres exactos de los roles del token (SGMA.Shared/Constants/Roles.cs)
export const ROLES = {
  administrador: 'Administrador',
  coordinador: 'Coordinador/Técnico',
  solicitante: 'Solicitante/Consulta',
}

const { administrador: A, coordinador: C, solicitante: S } = ROLES

// Menú agrupado por propósito (enunciado Avance 2, 4.2). Cada rol ve solo lo suyo, según la sección 11
// del enunciado general. Reportes no lleva grupo propio: el Dashboard es el Inicio del Administrador y
// Costos por orden se abre desde el detalle de la orden.
export const menu = [
  { grupo: null, items: [{ to: '/', texto: 'Inicio', Icono: House, roles: [A], fin: true }] },
  {
    grupo: 'Operación',
    items: [
      { to: '/ordenes', texto: 'Órdenes', Icono: ClipboardList, roles: [A, C] },
      { to: '/ordenes', texto: 'Mis solicitudes', Icono: ClipboardList, roles: [S] },
      { to: '/preventivo', texto: 'Preventivo', Icono: Gauge, roles: [A, C] },
    ],
  },
  {
    grupo: 'Inventario',
    items: [
      { to: '/vehiculos', texto: 'Vehículos', Icono: Truck, roles: [A, C, S] },
      { to: '/repuestos', texto: 'Repuestos', Icono: Package, roles: [A, C] },
    ],
  },
  {
    grupo: 'Catálogos',
    items: [
      { to: '/categorias', texto: 'Categorías', Icono: Layers, roles: [A, C] },
      { to: '/tecnicos', texto: 'Técnicos', Icono: UserCog, roles: [A, C] },
      { to: '/especialidades', texto: 'Especialidades', Icono: Wrench, roles: [A, C] },
      { to: '/proveedores', texto: 'Proveedores', Icono: Store, roles: [A, C] },
    ],
  },
  { grupo: 'Administración', items: [{ to: '/usuarios', texto: 'Usuarios', Icono: Users, roles: [A] }] },
]

export const menuDelRol = (rol) =>
  menu
    .map(({ grupo, items }) => ({ grupo, items: items.filter((i) => i.roles.includes(rol)) }))
    .filter(({ items }) => items.length > 0)

// Pantalla de inicio de cada rol (enunciado Avance 2, 4.2)
export const inicioDelRol = (rol) => (rol === A ? '/' : '/ordenes')

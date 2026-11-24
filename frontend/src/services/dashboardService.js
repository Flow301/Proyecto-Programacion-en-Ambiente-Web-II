import { api } from '@/api/api'

// Dashboard general (reporte A1): solo Administrador
export const obtenerDashboard = () => api('Dashboard')

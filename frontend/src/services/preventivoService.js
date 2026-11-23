import { api } from '@/api/api'

// Mantenimiento preventivo: alertas (Vencidos y Próximos), visibles para Administrador y Coordinador/Técnico
export const alertasPreventivo = () => api('Preventivo/alertas')

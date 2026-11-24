import { api } from '@/api/api'

// Mantenimiento preventivo (requerimientos 7.9). Se calcula en la API en cada consulta.
// Evaluación de toda la flota y generación de la orden: solo Administrador
export const evaluarPreventivo = () => api('Preventivo')
export const generarOrdenPreventiva = (idActivo, datos) => api(`Preventivo/${idActivo}/orden`, { method: 'POST', body: datos })
// Alertas (Vencidos y Próximos): Administrador y Coordinador/Técnico
export const alertasPreventivo = () => api('Preventivo/alertas')

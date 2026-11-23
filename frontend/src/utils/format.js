// Formatos únicos de presentación (enunciado Avance 2, 4.3). Toda pantalla usa estas funciones:
// fechas dd/mm/aaaa, montos en colones con separador de miles y unidades visibles.
// Solo dan formato: los cálculos (costos, porcentajes, días) los hace la API.

const SIN_DATO = '—'

const numero = new Intl.NumberFormat('es-CR', { maximumFractionDigits: 0 })
const unDecimal = new Intl.NumberFormat('es-CR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })
const colones = new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', maximumFractionDigits: 0 })
const dosDigitos = (n) => String(n).padStart(2, '0')

const vacio = (valor) => valor === null || valor === undefined || valor === ''

// La API manda las fechas en ISO sin zona ("2026-11-16T08:00:00"): se leen como hora local de Costa Rica
function aFecha(valor) {
  const fecha = valor instanceof Date ? valor : new Date(valor)
  return Number.isNaN(fecha.getTime()) ? null : fecha
}

// 16/11/2026
export function formatFecha(valor) {
  const f = !vacio(valor) && aFecha(valor)
  if (!f) return SIN_DATO
  return `${dosDigitos(f.getDate())}/${dosDigitos(f.getMonth() + 1)}/${f.getFullYear()}`
}

// 16/11, para espacios angostos donde el año se sobreentiende (pasos ya recorridos de la ruta)
export function formatFechaCorta(valor) {
  const f = !vacio(valor) && aFecha(valor)
  if (!f) return SIN_DATO
  return `${dosDigitos(f.getDate())}/${dosDigitos(f.getMonth() + 1)}`
}

// 16/11/2026 08:00, para el historial
export function formatFechaHora(valor) {
  const f = !vacio(valor) && aFecha(valor)
  if (!f) return SIN_DATO
  return `${formatFecha(f)} ${dosDigitos(f.getHours())}:${dosDigitos(f.getMinutes())}`
}

// ₡171 500
export function formatColones(valor) {
  return vacio(valor) ? SIN_DATO : colones.format(Number(valor))
}

// 412 800
export function formatNumero(valor) {
  return vacio(valor) ? SIN_DATO : numero.format(Number(valor))
}

// 412 800 km (la unidad puede venir de la API, como la de la variable de control)
export function formatKm(valor, unidad = 'km') {
  return vacio(valor) ? SIN_DATO : `${numero.format(Number(valor))} ${unidad}`
}

// 6,0 h
export function formatHoras(valor) {
  return vacio(valor) ? SIN_DATO : `${unDecimal.format(Number(valor))} h`
}

// 94,0 %
export function formatPorcentaje(valor) {
  return vacio(valor) ? SIN_DATO : `${unDecimal.format(Number(valor))} %`
}

// La API devuelve la imagen relativa a su origen ("/imagenes/activos/PKP-0001.jpg"):
// el navegador la buscaría en el puerto de Vite, así que se completa con el origen de la API
const ORIGEN_API = new URL(import.meta.env?.VITE_API_URL ?? 'http://localhost').origin

export function urlImagen(ruta) {
  if (!ruta) return null
  if (/^https?:\/\//i.test(ruta)) return ruta
  return `${ORIGEN_API}/${ruta.replace(/^\/+/, '')}`
}

import { z } from 'zod'

// Reglas que repiten las validaciones de los DTO de la API, para avisar antes de enviar.
// La API valida de todos modos (doble validación, convenciones §3).

const SIN_ANGULOS = /^[^<>]*$/

// Texto obligatorio, sin < ni > (la API los rechaza contra XSS)
export const texto = (etiqueta, max) =>
  z.string().trim()
    .min(1, `Ingrese ${etiqueta}.`)
    .max(max, `Máximo ${max} caracteres.`)
    .regex(SIN_ANGULOS, 'No use los signos < ni >.')

// Texto opcional: vacío se envía como null
export const textoOpcional = (max) =>
  z.string().trim()
    .max(max, `Máximo ${max} caracteres.`)
    .regex(SIN_ANGULOS, 'No use los signos < ni >.')
    .transform((v) => v || null)

// Número escrito en un campo: llega como texto y sale como número
export const numero = (etiqueta, { min, max, entero = true } = {}) =>
  z.string().trim()
    .min(1, `Ingrese ${etiqueta}.`)
    .refine((v) => (entero ? /^\d+$/ : /^\d+([.,]\d{1,2})?$/).test(v), entero ? 'Use solo números enteros.' : 'Use números con hasta dos decimales.')
    .transform((v) => Number(v.replace(',', '.')))
    .refine((v) => min === undefined || v >= min, `El valor mínimo es ${min}.`)
    .refine((v) => max === undefined || v <= max, `El valor máximo es ${max}.`)

// Elección de una lista: llega como texto ('' sin elegir) y sale como id numérico
export const eleccion = (etiqueta) =>
  z.string().min(1, `Elija ${etiqueta}.`).transform(Number)

export const telefono = z.string().trim()
  .min(1, 'Ingrese el teléfono.')
  .regex(/^\d{4}-?\d{4}$/, 'Use el formato 8888-8888.')

export const correo = z.string().trim()
  .min(1, 'Ingrese el correo.')
  .max(100, 'Máximo 100 caracteres.')
  .email('Escriba un correo válido, por ejemplo nombre@empresa.cr.')

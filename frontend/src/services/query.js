// Agrega los filtros a la ruta como query string: { idEstadoOrden: [1, 3], idActivo: 6 }
// → ruta?idEstadoOrden=1&idEstadoOrden=3&idActivo=6. Los valores vacíos no se envían.
export function conFiltros(ruta, filtros = {}) {
  const query = new URLSearchParams()
  for (const [clave, valor] of Object.entries(filtros)) {
    for (const v of [].concat(valor ?? [])) if (v !== '' && v != null) query.append(clave, v)
  }
  const texto = query.toString()
  return texto ? `${ruta}?${texto}` : ruta
}

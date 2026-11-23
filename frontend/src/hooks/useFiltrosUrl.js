import { useCallback } from 'react'
import { useSearchParams } from 'react-router'

// Filtros y página en la URL (?idEstadoOrden=1&idEstadoOrden=3&pagina=2): recargar, compartir el enlace
// o volver desde un detalle conserva lo que el usuario estaba viendo.
// Cambiar un filtro vuelve a la página 1; los valores vacíos se quitan de la URL.
export default function useFiltrosUrl() {
  const [params, setParams] = useSearchParams()

  const pagina = Math.max(Number(params.get('pagina')) || 1, 1)
  const valor = useCallback((clave) => params.get(clave) ?? '', [params])
  const valores = useCallback((clave) => params.getAll(clave), [params])

  const cambiar = useCallback((clave, nuevo) => {
    setParams((actual) => {
      const siguiente = new URLSearchParams(actual)
      siguiente.delete(clave)
      for (const v of [].concat(nuevo ?? [])) if (v !== '' && v != null) siguiente.append(clave, v)
      siguiente.delete('pagina')
      return siguiente
    }, { replace: true })
  }, [setParams])

  const irAPagina = useCallback((n) => {
    setParams((actual) => {
      const siguiente = new URLSearchParams(actual)
      if (n > 1) siguiente.set('pagina', n)
      else siguiente.delete('pagina')
      return siguiente
    })
  }, [setParams])

  const limpiar = useCallback(() => setParams({}, { replace: true }), [setParams])

  return { params, pagina, valor, valores, cambiar, irAPagina, limpiar, hayFiltros: [...params.keys()].some((k) => k !== 'pagina') }
}

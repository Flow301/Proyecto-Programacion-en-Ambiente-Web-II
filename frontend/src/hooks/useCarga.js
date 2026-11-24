import { useCallback, useEffect, useMemo, useState } from 'react'

// Carga datos de la API con su estado: cargando, error y datos. recargar() repite la petición
// (botón "Reintentar"). cargar debe ser estable (useCallback o una función de módulo).
// "cargando" se deriva: la respuesta guardada no corresponde todavía a la petición actual.
export default function useCarga(cargar) {
  const [intento, setIntento] = useState(0)
  const peticion = useMemo(() => ({ cargar, intento }), [cargar, intento])
  const [respuesta, setRespuesta] = useState({ peticion: null, datos: null, error: null })

  useEffect(() => {
    let vigente = true
    peticion.cargar()
      .then((datos) => vigente && setRespuesta({ peticion, datos, error: null }))
      .catch((error) => vigente && setRespuesta({ peticion, datos: null, error }))
    return () => { vigente = false }
  }, [peticion])

  const recargar = useCallback(() => setIntento((n) => n + 1), [])
  const cargando = respuesta.peticion !== peticion
  return { datos: respuesta.datos, error: cargando ? null : respuesta.error, cargando, recargar }
}

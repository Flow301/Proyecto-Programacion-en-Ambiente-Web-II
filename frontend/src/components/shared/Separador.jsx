// Punto medio entre dos datos ("CMP-0001 · Camión de carga pesada"). En Overpass el glifo "·" queda pegado
// a la palabra siguiente, así que va centrado en su propia caja. Se escribe entre espacios para que el
// lector de pantalla no junte las palabras: {codigo} <Separador /> {nombre}
export default function Separador() {
  return <span aria-hidden="true" className="inline-block w-[0.5em] text-center">·</span>
}

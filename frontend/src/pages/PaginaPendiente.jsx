import EncabezadoPagina from '@/components/shared/EncabezadoPagina'
import EstadoVacio from '@/components/shared/EstadoVacio'

// shortcut: marca la ruta de una pantalla de negocio que todavía no existe (plan de trabajo, semanas 10 a 12);
// cada pantalla la reemplaza en App.jsx cuando se construye
export default function PaginaPendiente({ titulo }) {
  return (
    <>
      <EncabezadoPagina titulo={titulo} />
      <EstadoVacio titulo="Esta pantalla todavía no está construida" mensaje="La ruta, el menú y los permisos ya funcionan; el contenido se agrega en su semana del plan de trabajo." />
    </>
  )
}

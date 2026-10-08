# Modelo de datos

Este documento describe las tablas de SGMA, sus campos y tipos, sus relaciones, las restricciones de unicidad y de validación, y los datos que cargan los seeders. Las reglas de negocio que usan estos datos están en [requerimientos](requerimientos.md) y las transiciones de estado en la [matriz de estados](matriz-estados.md).

Las entidades se definen en código (`SGMA.Domain/Entities`) y su configuración en `AppDbContext` (`SGMA.Infrastructure/Data`); la base se genera con migraciones.

## Convenciones del modelo

- **Nombres.** Tablas y entidades en español y en singular, sin tildes ni ñ: `Activo`, `OrdenMantenimiento`, `TecnicoEspecialidad`. Las llaves se llaman `Id` más el nombre de la entidad: `IdActivo`, `IdOrden`.
- **Restricciones con nombre**, como en el material de clase: `PK_Tabla`, `FK_Tabla_Referencia`, `UQ_Tabla_Campo` y `CK_Tabla_Regla`.
- **Llaves primarias.** Usamos IDENTITY en todas las tablas. El material asigna los ids a mano con `ValueGeneratedNever()` y calcula el siguiente con `Max + 1`, lo que falla si dos usuarios crean registros a la vez. Los catálogos sembrados con `HasData` mantienen ids fijos y el resto del código los referencia con constantes en `SeedIds`.
- **Sin borrado en cascada.** Todas las relaciones usan `DeleteBehavior.Restrict`. Nada se borra en cadena: los borrados físicos que permitimos (categoría y especialidad sin relaciones) se validan antes en el servicio.
- **Estado lógico.** `Tecnico`, `Repuesto`, `Proveedor` y `Usuario` tienen un campo `Estado` de tipo `bit` (1 = activo, 0 = inactivo), igual que el `Status` del material. Eliminar en esas entidades es poner `Estado = 0`.
- **Tipos SQL y su equivalente en C#.**
  - `int` → `int`
  - `smallint` → `short`
  - `bit` → `bool`
  - `decimal` → `decimal`
  - `nvarchar` y `varchar` → `string`
  - `date` y `datetime2` → `DateTime`
- **Montos** en colones, con `decimal(18,2)`.
- **Fechas.** `date` para fechas de calendario (adquisición, certificación, fecha programada) y `datetime2` para momentos exactos (solicitud, cambios de estado, cierre). Guardamos la hora local del servidor, como el `sysdatetime()` del material.
- **Entidades** como `record class` con propiedades de navegación `virtual`, siguiendo el material.

---

## Catálogos

Son tablas pequeñas con valores fijos. Se siembran con `HasData` y viajan dentro de las migraciones.

### Rol

- `IdRol` (int): clave primaria.
- `Nombre` (nvarchar(30)): obligatorio y único.

Valores: 1 Administrador, 2 Coordinador/Técnico, 3 Solicitante/Consulta.

### EstadoActivo

- `IdEstadoActivo` (int): clave primaria.
- `Nombre` (nvarchar(30)): obligatorio y único.

Valores: 1 Operativo, 2 En Mantenimiento, 3 Fuera de Servicio, 4 Dado de Baja.

### EstadoOrden

- `IdEstadoOrden` (int): clave primaria.
- `Nombre` (nvarchar(30)): obligatorio y único.
- `EsFinal` (bit): indica si el estado no admite más transiciones.

Valores:

1. Solicitada
2. Diagnosticada
3. Aprobada
4. Rechazada (final)
5. En Ejecución
6. En Espera de Repuesto
7. Completada (final)
8. Cancelada (final)

### Prioridad

- `IdPrioridad` (int): clave primaria.
- `Nombre` (nvarchar(20)): obligatorio y único.
- `Nivel` (smallint): orden de importancia, de 1 (Baja) a 4 (Crítica).

Valores: 1 Baja, 2 Media, 3 Alta, 4 Crítica.

### NaturalezaMantenimiento

- `IdNaturaleza` (int): clave primaria.
- `Nombre` (nvarchar(20)): obligatorio y único.

Valores: 1 Preventivo, 2 Correctivo, 3 Emergencia.

### NivelCertificacion

- `IdNivelCertificacion` (int): clave primaria.
- `Nombre` (nvarchar(20)): obligatorio y único.

Valores: 1 Básico, 2 Intermedio, 3 Avanzado, 4 Experto.

### TipoMantenimiento

- `IdTipoMantenimiento` (int): clave primaria.
- `Nombre` (nvarchar(80)): obligatorio y único.
- `Descripcion` (nvarchar(250)): opcional.
- `IdNaturaleza` (int): obligatorio, FK a `NaturalezaMantenimiento`.
- `IdEspecialidadRequerida` (int): obligatorio, FK a `Especialidad`.

Los valores están en la sección de seeders.

---

## Entidades principales

### CategoriaActivo

- `IdCategoria` (int): clave primaria.
- `Nombre` (nvarchar(60)): obligatorio y único.
- `Descripcion` (nvarchar(250)): opcional.
- `Prefijo` (char(3)): obligatorio y único; se usa en el código de los vehículos.
- `IntervaloKm` (int): obligatorio; cada cuántos km corresponde el mantenimiento preventivo.
- `MargenAvisoKm` (int): obligatorio; cuántos km antes del intervalo el vehículo pasa a "próximo a vencer".

Se siembra con `HasData`, como la categoría del material.

### Activo

- `IdActivo` (int): clave primaria.
- `Codigo` (varchar(10)): obligatorio y único, con el patrón `PREFIJO-0001`.
- `Nombre` (nvarchar(100)): obligatorio.
- `Descripcion` (nvarchar(250)): opcional.
- `IdCategoria` (int): obligatorio, FK a `CategoriaActivo`.
- `IdEstadoActivo` (int): obligatorio, FK a `EstadoActivo`.
- `FechaAdquisicion` (date): obligatorio.
- `ValorReferencia` (decimal(18,2)): obligatorio.
- `Imagen` (nvarchar(200)): opcional; ruta relativa del archivo, por ejemplo `imagenes/activos/PKP-0001.jpg`.
- `Placa` (varchar(10)): obligatorio y único.
- `Marca` (nvarchar(40)): obligatorio.
- `Modelo` (nvarchar(40)): obligatorio.
- `Anio` (smallint): obligatorio; año de fabricación.
- `KilometrajeActual` (int): obligatorio.
- `KilometrajeUltimoPreventivo` (int): obligatorio.
- `IntervaloKmPropio` (int): opcional; si existe, reemplaza al de la categoría.
- `FechaRegistro` (datetime2): obligatorio, con valor por defecto `sysdatetime()`.

### Especialidad

- `IdEspecialidad` (int): clave primaria.
- `Nombre` (nvarchar(60)): obligatorio y único.
- `Descripcion` (nvarchar(250)): opcional.

Se siembra con `HasData`, porque los tipos de mantenimiento la referencian.

### Tecnico

- `IdTecnico` (int): clave primaria.
- `NombreCompleto` (nvarchar(100)): obligatorio.
- `Identificacion` (varchar(20)): obligatorio y único; cédula.
- `Telefono` (varchar(20)): obligatorio.
- `Correo` (nvarchar(100)): obligatorio.
- `Disponible` (bit): obligatorio.
- `TarifaHora` (decimal(10,2)): obligatorio.
- `Estado` (bit): obligatorio; 1 = activo.

### Repuesto

- `IdRepuesto` (int): clave primaria.
- `Codigo` (varchar(20)): obligatorio y único.
- `Nombre` (nvarchar(100)): obligatorio.
- `UnidadMedida` (nvarchar(20)): obligatorio; por ejemplo unidad, juego o galón.
- `StockActual` (int): obligatorio.
- `StockMinimo` (int): obligatorio.
- `CostoUnitario` (decimal(18,2)): obligatorio; es el costo que se copia al detalle de la orden al usar el repuesto.
- `Estado` (bit): obligatorio; 1 = activo.

### Proveedor

- `IdProveedor` (int): clave primaria.
- `Nombre` (nvarchar(100)): obligatorio.
- `CedulaJuridica` (varchar(20)): obligatorio y único.
- `PersonaContacto` (nvarchar(100)): obligatorio.
- `Telefono` (varchar(20)): obligatorio.
- `Correo` (nvarchar(100)): obligatorio.
- `Direccion` (nvarchar(200)): obligatorio.
- `CondicionesGenerales` (nvarchar(300)): opcional.
- `Estado` (bit): obligatorio; 1 = activo.

### Usuario

- `IdUsuario` (int): clave primaria.
- `NombreCompleto` (nvarchar(100)): obligatorio.
- `Login` (varchar(50)): obligatorio y único.
- `Correo` (nvarchar(100)): obligatorio y único.
- `ClaveHash` (nvarchar(255)): obligatorio; hash generado con `PasswordHasher`, nunca la contraseña.
- `IdRol` (int): obligatorio, FK a `Rol`.
- `Estado` (bit): obligatorio; 1 = activo.
- `FechaCreacion` (datetime2): obligatorio, con valor por defecto `sysdatetime()`.
- `UltimoAcceso` (datetime2): opcional; se actualiza en cada inicio de sesión.

### OrdenMantenimiento

- `IdOrden` (int): clave primaria.
- `Codigo` (varchar(15)): obligatorio y único, con el patrón `OM-AAAA-00001`.
- `IdActivo` (int): obligatorio, FK a `Activo`.
- `IdTipoMantenimiento` (int): obligatorio, FK a `TipoMantenimiento`.
- `IdPrioridad` (int): opcional hasta el diagnóstico, FK a `Prioridad`.
- `IdEstadoOrden` (int): obligatorio, FK a `EstadoOrden`.
- `IdUsuarioSolicitante` (int): obligatorio, FK a `Usuario`.
- `Motivo` (nvarchar(500)): obligatorio.
- `Diagnostico` (nvarchar(1000)): opcional hasta el diagnóstico.
- `KilometrajeDiagnostico` (int): opcional hasta el diagnóstico.
- `FechaSolicitud` (datetime2): obligatorio, con valor por defecto `sysdatetime()`.
- `FechaProgramada` (date): opcional hasta la aprobación.
- `FechaCierre` (datetime2): opcional; se llena en cualquier estado final.
- `CostoRepuestos` (decimal(18,2)): opcional; se congela al cerrar.
- `CostoManoObra` (decimal(18,2)): opcional; se congela al cerrar.
- `CostoTotal` (decimal(18,2)): opcional; se congela al cerrar.

Mientras la orden está abierta, los tres costos quedan vacíos y la API los calcula en vivo con LINQ.

---

## Tablas puente (relaciones N:N con atributos propios)

### TecnicoEspecialidad (Técnico N:N Especialidad)

- `IdTecnico` (int): FK a `Tecnico`.
- `IdEspecialidad` (int): FK a `Especialidad`.
- `IdNivelCertificacion` (int): obligatorio, FK a `NivelCertificacion`.
- `FechaCertificacion` (date): obligatorio; desde cuándo posee la especialidad.

La clave primaria es compuesta, (`IdTecnico`, `IdEspecialidad`), así que la base impide asociar la misma especialidad dos veces al mismo técnico.

### RepuestoProveedor (Repuesto N:N Proveedor)

- `IdRepuesto` (int): FK a `Repuesto`.
- `IdProveedor` (int): FK a `Proveedor`.
- `PrecioOfrecido` (decimal(18,2)): obligatorio.
- `TiempoEntregaDias` (int): obligatorio.

La clave primaria es compuesta, (`IdRepuesto`, `IdProveedor`), lo que impide duplicados. De esta tabla salen los datos del proveedor recomendado.

### AsignacionTecnico (Orden N:N Técnico)

- `IdOrden` (int): FK a `OrdenMantenimiento`.
- `IdTecnico` (int): FK a `Tecnico`.
- `FechaAsignacion` (datetime2): obligatorio.
- `HorasTrabajadas` (decimal(6,2)): obligatorio, empieza en 0; cada registro de horas se suma.
- `TarifaAplicada` (decimal(10,2)): opcional; copia de la tarifa del técnico al cerrar la orden.

La clave primaria es compuesta, (`IdOrden`, `IdTecnico`), así que un técnico no se asigna dos veces a la misma orden.

### DetalleRepuesto (Orden N:N Repuesto)

- `IdOrden` (int): FK a `OrdenMantenimiento`.
- `NumeroLinea` (int): consecutivo dentro de la orden.
- `IdRepuesto` (int): obligatorio, FK a `Repuesto`.
- `Cantidad` (int): obligatorio.
- `CostoUnitario` (decimal(18,2)): obligatorio; costo del repuesto al momento del uso.
- `Subtotal` (decimal(18,2)): obligatorio; cantidad por costo unitario.
- `FechaRegistro` (datetime2): obligatorio.

La clave primaria es (`IdOrden`, `NumeroLinea`), igual que el detalle de factura del material. Así, un mismo repuesto puede aparecer en dos líneas de la misma orden con costos distintos si se registró en momentos diferentes.

### RepuestoPendiente (Orden N:N Repuesto)

- `IdOrden` (int): FK a `OrdenMantenimiento`.
- `IdRepuesto` (int): FK a `Repuesto`.
- `Cantidad` (int): obligatorio.
- `FechaRegistro` (datetime2): obligatorio.

La clave primaria es compuesta, (`IdOrden`, `IdRepuesto`). Guarda lo que no se pudo surtir por falta de stock. Al reanudar la orden, cada pendiente se convierte en una línea de `DetalleRepuesto` y se elimina de esta tabla.

---

## Tablas de historial

### HistorialEstadoActivo

- `IdHistorial` (int): clave primaria.
- `IdActivo` (int): obligatorio, FK a `Activo`.
- `IdEstadoAnterior` (int): opcional, FK a `EstadoActivo`; vacío en el registro inicial.
- `IdEstadoNuevo` (int): obligatorio, FK a `EstadoActivo`.
- `FechaCambio` (datetime2): obligatorio.
- `IdUsuario` (int): obligatorio, FK a `Usuario`; quien ejecutó la acción que provocó el cambio.
- `IdOrden` (int): opcional, FK a `OrdenMantenimiento`; la orden que provocó el cambio, cuando el cambio fue automático.
- `Observacion` (nvarchar(500)): opcional.

### HistorialEstadoOrden

- `IdHistorial` (int): clave primaria.
- `IdOrden` (int): obligatorio, FK a `OrdenMantenimiento`.
- `IdEstadoAnterior` (int): opcional, FK a `EstadoOrden`; vacío en el registro inicial.
- `IdEstadoNuevo` (int): obligatorio, FK a `EstadoOrden`.
- `FechaCambio` (datetime2): obligatorio.
- `IdUsuario` (int): obligatorio, FK a `Usuario`.
- `Observacion` (nvarchar(500)): opcional.

Ninguna de las dos tablas tiene endpoints de escritura: solo las llenan los servicios al cambiar un estado, y los seeders.

---

## Relaciones

**Relaciones 1:N**

- Un `Rol` tiene muchos `Usuario`.
- Una `CategoriaActivo` tiene muchos `Activo`.
- Un `EstadoActivo` está en muchos `Activo`.
- Un `Activo` tiene muchas `OrdenMantenimiento` y muchos `HistorialEstadoActivo`.
- Una `NaturalezaMantenimiento` agrupa muchos `TipoMantenimiento`.
- Una `Especialidad` es requerida por muchos `TipoMantenimiento`.
- Un `TipoMantenimiento` está en muchas `OrdenMantenimiento`.
- Una `Prioridad` está en muchas `OrdenMantenimiento`.
- Un `EstadoOrden` está en muchas `OrdenMantenimiento`.
- Un `Usuario` solicita muchas `OrdenMantenimiento` y es responsable de muchos registros de historial.
- Un `NivelCertificacion` está en muchas filas de `TecnicoEspecialidad`.
- Una `OrdenMantenimiento` tiene muchos `HistorialEstadoOrden` y puede haber provocado muchos `HistorialEstadoActivo`.

**Relaciones N:N, cada una con su tabla puente y atributos propios**

- `Tecnico` N:N `Especialidad`, por `TecnicoEspecialidad`: nivel de certificación y fecha.
- `Repuesto` N:N `Proveedor`, por `RepuestoProveedor`: precio y tiempo de entrega.
- `OrdenMantenimiento` N:N `Tecnico`, por `AsignacionTecnico`: fecha, horas y tarifa aplicada.
- `OrdenMantenimiento` N:N `Repuesto`, por `DetalleRepuesto`: cantidad, costo unitario y subtotal.
- `OrdenMantenimiento` N:N `Repuesto`, por `RepuestoPendiente`: cantidad que falta.

## Restricciones de unicidad

- **Catálogos:** el `Nombre` de cada uno (`Rol`, `EstadoActivo`, `EstadoOrden`, `Prioridad`, `NaturalezaMantenimiento`, `NivelCertificacion` y `TipoMantenimiento`).
- **CategoriaActivo:** `Nombre` y `Prefijo`.
- **Activo:** `Codigo` y `Placa`.
- **Especialidad:** `Nombre`.
- **Tecnico:** `Identificacion`.
- **Repuesto:** `Codigo`.
- **Proveedor:** `CedulaJuridica`.
- **Usuario:** `Login` y `Correo`.
- **OrdenMantenimiento:** `Codigo`.
- **Tablas puente:** sus claves compuestas (`TecnicoEspecialidad`, `RepuestoProveedor`, `AsignacionTecnico` y `RepuestoPendiente`).

Todos los índices únicos se configuran con `.IsUnique()`. El material de clase tiene un índice de categoría sin esa llamada, y por eso ese índice no garantiza unicidad.

## Restricciones CHECK

Son la última defensa en la base; las mismas reglas se validan antes en los servicios.

- **CategoriaActivo:** `IntervaloKm > 0`, y `MargenAvisoKm >= 0 AND MargenAvisoKm < IntervaloKm`.
- **Activo:**
  - `KilometrajeActual >= 0`;
  - `KilometrajeUltimoPreventivo BETWEEN 0 AND KilometrajeActual`;
  - `ValorReferencia >= 0`;
  - `IntervaloKmPropio IS NULL OR IntervaloKmPropio > 0`.
- **Tecnico:** `TarifaHora > 0`.
- **Repuesto:** `StockActual >= 0`, `StockMinimo >= 0` y `CostoUnitario > 0`. La primera es la que garantiza que el stock nunca sea negativo.
- **RepuestoProveedor:** `PrecioOfrecido > 0` y `TiempoEntregaDias >= 1`.
- **AsignacionTecnico:** `HorasTrabajadas >= 0`.
- **DetalleRepuesto:** `Cantidad > 0` y `CostoUnitario > 0`.
- **RepuestoPendiente:** `Cantidad > 0`.

---

## Seeders

Seguimos los tres niveles del material:

1. **`CatalogSeeder` (`HasData`):** catálogos, categorías, especialidades y tipos de mantenimiento. Llegan a todos los ambientes dentro de las migraciones.
2. **`MasterSeeder` (`UseSeeding`):** el usuario `admin`, con la contraseña tomada de `Seed:AdminPassword` en User Secrets. Llega a todos los ambientes.
3. **`DevelopmentSeeder` (`UseAsyncSeeding`, solo en Development):** usuarios de demostración, vehículos, técnicos, proveedores, repuestos, asociaciones y órdenes.
   - Corre dentro de una transacción.
   - No hace nada si ya existen vehículos.
   - Como las llaves son IDENTITY, resuelve las relaciones por clave natural: código, placa, identificación o login.

Todos los nombres de personas y empresas son ficticios. Las marcas y modelos de vehículos son reales, las placas siguen el formato costarricense y los correos usan el dominio reservado `.test`.

### Categorías (HasData, 10)

Cada categoría indica su prefijo, su intervalo y su margen de aviso. La justificación de estos valores está en [decisiones técnicas](decisiones-tecnicas.md).

1. Pickup — `PKP` — cada 10 000 km — aviso a 1 000 km.
2. Camión liviano — `CML` — 10 000 km — 1 000 km.
3. Camión pesado — `CMP` — 15 000 km — 1 500 km.
4. Tractocamión — `TRC` — 20 000 km — 2 000 km.
5. Bus urbano — `BUS` — 10 000 km — 1 000 km.
6. Bus interurbano — `BUI` — 15 000 km — 1 500 km.
7. Microbús — `MCB` — 10 000 km — 1 000 km.
8. Van de carga — `VAN` — 10 000 km — 1 000 km.
9. Vehículo administrativo — `ADM` — 10 000 km — 1 000 km.
10. Motocicleta — `MOT` — 5 000 km — 500 km.

Microbús y Motocicleta quedan sin vehículos a propósito, para poder demostrar que una categoría sin vehículos sí se puede eliminar.

### Especialidades (HasData, 10)

1. Mecánica general
2. Motores diésel
3. Motores de gasolina
4. Frenos
5. Suspensión y dirección
6. Sistema eléctrico y electrónica
7. Transmisión y embrague
8. Aire acondicionado
9. Llantas y alineamiento
10. Carrocería y pintura

### Tipos de mantenimiento (HasData, 13)

Cada tipo indica su naturaleza y la especialidad que requiere.

1. Servicio preventivo por kilometraje — Preventivo — Mecánica general.
2. Servicio preventivo de motor diésel — Preventivo — Motores diésel.
3. Reparación de frenos — Correctivo — Frenos.
4. Reparación de suspensión y dirección — Correctivo — Suspensión y dirección.
5. Reparación del sistema eléctrico — Correctivo — Sistema eléctrico y electrónica.
6. Reparación de motor diésel — Correctivo — Motores diésel.
7. Reparación de motor de gasolina — Correctivo — Motores de gasolina.
8. Reparación de transmisión y embrague — Correctivo — Transmisión y embrague.
9. Reparación de aire acondicionado — Correctivo — Aire acondicionado.
10. Cambio y alineamiento de llantas — Correctivo — Llantas y alineamiento.
11. Reparación de carrocería — Correctivo — Carrocería y pintura.
12. Auxilio por avería en ruta — Emergencia — Mecánica general.
13. Atención de emergencia en taller — Emergencia — Mecánica general.

### Usuarios (8)

`admin` lo crea el `MasterSeeder`; los demás, el `DevelopmentSeeder`, con una contraseña común de desarrollo definida en `DemoData`, como en el material.

1. `admin` — Administrador General — Administrador.
2. `lvargas` — Luis Vargas Mora, jefe de taller — Coordinador/Técnico.
3. `asolis` — Andrea Solís Rojas, coordinadora de taller — Coordinador/Técnico.
4. `cjimenez` — Carlos Jiménez Araya, supervisor de rutas urbanas — Solicitante/Consulta.
5. `mcastro` — María Fernanda Castro Quesada, encargada de logística — Solicitante/Consulta.
6. `jchaves` — José Pablo Chaves Vega, supervisor de carga — Solicitante/Consulta.
7. `rmendez` — Roberto Méndez Solano, supervisor de distribución — Solicitante/Consulta.
8. `drojas` — Daniela Rojas Hernández — Solicitante/Consulta, **inactiva**. Sirve para probar que un usuario inactivo no puede iniciar sesión.

### Vehículos (12)

Formato de cada línea: código — unidad — placa — año — km actual / km del último preventivo — estado.

1. `PKP-0001` — Toyota Hilux 2.4 4x4 — CL-284731 — 2021 — 86 450 / 80 000 — Operativo.
2. `PKP-0002` — Nissan Frontier 4x4 — CL-301562 — 2019 — 128 900 / 119 500 — Operativo.
3. `PKP-0003` — Isuzu D-Max 4x4 — CL-317845 — 2022 — 54 200 / 40 000 — Operativo.
4. `CML-0001` — Isuzu NPR — CL-275903 — 2018 — 187 300 / 180 000 — En Mantenimiento.
5. `CML-0002` — Hino 300 Serie 716 — CL-289114 — 2020 — 142 650 / 133 000 — Operativo.
6. `CMP-0001` — Freightliner M2 106 — C-158327 — 2017 — 412 800 / 400 000 — En Mantenimiento.
7. `TRC-0001` — Volvo FH 460 — C-163950 — 2019 — 538 600 / 520 000 — Operativo.
8. `BUS-0001` — Mercedes-Benz OF-1721 — AB-6214 — 2018 — 356 900 / 350 000 — Operativo.
9. `BUS-0002` — Volvo B290R — AB-6587 — 2020 — 241 300 / 230 000 — Fuera de Servicio.
10. `BUI-0001` — Scania K360 — AB-7012 — 2019 — 498 200 / 485 000 — Operativo.
11. `VAN-0001` — Hyundai H100 — CL-296470 — 2017 — 214 900 / 210 000 — Operativo.
12. `ADM-0001` — Toyota RAV4 — BKL-482 — 2012 — 241 000 / 236 000 — Dado de Baja (unidad vendida).

Cada vehículo lleva también su descripción, su fecha de adquisición y su valor de referencia en colones. Las imágenes se guardan como `imagenes/activos/<código>.jpg` dentro de `wwwroot`.

**Pendiente:** conseguir las 12 fotos, propias o con licencia libre.

Con estos datos, la evaluación preventiva muestra todos los casos:

- **Vencidos:** `PKP-0003` (ya tiene una orden preventiva abierta, así que aparece como "orden en curso") y `BUS-0002`.
- **Próximos a vencer:** `PKP-0002` (faltan 600 km), `CML-0002` (350 km) y `TRC-0001` (1 400 km).
- **Al día:** el resto.
- **Excluido:** `ADM-0001`, por estar dado de baja.

### Técnicos (10)

Cada técnico tiene su cédula, su tarifa por hora y sus especialidades, con el nivel y el año de certificación.

1. Jorge Alberto Mora Solano — 1-0845-0231 — ₡5 200/h — Mecánica general (Experto, 2012), Motores de gasolina (Avanzado, 2015).
2. Ricardo Ulate Campos — 2-0611-0874 — ₡4 800/h — Frenos (Avanzado, 2016), Suspensión y dirección (Intermedio, 2019).
3. Esteban Quirós Fallas — 1-1203-0456 — ₡6 000/h — Motores diésel (Experto, 2010), Transmisión y embrague (Avanzado, 2014).
4. Mauricio Salas Brenes — 4-0198-0723 — ₡4 500/h — Motores diésel (Intermedio, 2018), Mecánica general (Avanzado, 2015).
5. Kevin Arias Méndez — 1-1532-0098 — ₡3 800/h — Mecánica general (Básico, 2023), Motores diésel (Básico, 2024), Llantas y alineamiento (Intermedio, 2022).
6. Silvia Navarro Chacón — 3-0457-0612 — ₡5 500/h — Suspensión y dirección (Avanzado, 2017), Frenos (Intermedio, 2020).
7. Óscar Herrera Murillo — 2-0789-0345 — ₡4 200/h — Sistema eléctrico y electrónica (Experto, 2011), Aire acondicionado (Avanzado, 2016). **No disponible** (vacaciones).
8. Fabián Rodríguez Alfaro — 1-0976-0811 — ₡5 000/h — Sistema eléctrico y electrónica (Avanzado, 2018), Aire acondicionado (Intermedio, 2021).
9. Gabriela Castro Zúñiga — 5-0321-0554 — ₡4 600/h — Carrocería y pintura (Avanzado, 2016), Llantas y alineamiento (Básico, 2023).
10. Randall Porras Sibaja — 6-0244-0198 — ₡3 900/h — Mecánica general (Intermedio, 2013). **Inactivo.**

Son 21 asociaciones técnico–especialidad. Cada especialidad tiene al menos un técnico activo y disponible.

### Proveedores (10)

1. Repuestos Diésel del Valle S.A. — 3-101-456123 — La Uruca, San José.
2. Frenos y Embragues Centroamericanos S.A. — 3-101-552310 — El Coyol, Alajuela.
3. Llantas y Servicios del Norte S.A. — 3-101-631874 — Barva, Heredia.
4. Distribuidora Automotriz Pacífico S.A. — 3-101-702456 — Barranca, Puntarenas.
5. Lubricantes y Filtros Central S.R.L. — 3-102-518907 — Cartago centro.
6. Electroauto Repuestos Eléctricos S.A. — 3-101-689201 — Pavas, San José.
7. Importadora de Partes Pesadas S.A. — 3-101-744318 — Grecia, Alajuela.
8. Climatización Vehicular CR S.A. — 3-101-590662 — Belén, Heredia.
9. Suspensiones Técnicas del Este S.A. — 3-101-615027 — Paraíso, Cartago.
10. Repuestos Rápidos Guanacaste S.R.L. — 3-102-611245 — Liberia, Guanacaste. **Inactivo.**

Cada proveedor lleva su persona de contacto, teléfono, correo y condiciones generales, por ejemplo "Crédito a 30 días, garantía de 6 meses".

### Repuestos (12) y sus proveedores (24 asociaciones)

Formato de cada repuesto: código — nombre — unidad — stock / mínimo — costo unitario. Debajo van sus proveedores, con el precio ofrecido y el tiempo de entrega.

1. `FIL-ACE-001` — Filtro de aceite para motor diésel — unidad — 25 / 10 — ₡9 500.
   - Lubricantes y Filtros Central: ₡9 200 en 1 día.
   - Repuestos Diésel del Valle: ₡8 600 en 4 días.
   - Distribuidora Automotriz Pacífico: ₡9 900 en 2 días.
2. `FIL-AIR-001` — Filtro de aire para servicio pesado — unidad — 12 / 6 — ₡24 000.
   - Lubricantes y Filtros Central: ₡23 500 en 2 días.
   - Importadora de Partes Pesadas: ₡21 800 en 6 días.
3. `FIL-COM-001` — Filtro de combustible diésel — unidad — 18 / 8 — ₡14 500.
   - Repuestos Diésel del Valle: ₡13 900 en 3 días.
   - Lubricantes y Filtros Central: ₡14 200 en 1 día.
4. `ACE-1540-GAL` — Aceite 15W-40 para motor diésel — galón — 40 / 20 — ₡16 800.
   - Lubricantes y Filtros Central: ₡16 300 en 1 día.
   - Distribuidora Automotriz Pacífico: ₡15 900 en 3 días.
5. `PAS-FRE-001` — Juego de pastillas de freno delanteras para pickup — juego — 6 / 4 — ₡32 000.
   - Frenos y Embragues Centroamericanos: ₡30 500 en 2 días.
   - Repuestos Rápidos Guanacaste (inactivo): ₡27 000 en 1 día.
6. `ZAP-FRE-001` — Juego de zapatas de freno para camión y bus — juego — **3 / 4** — ₡68 000.
   - Frenos y Embragues Centroamericanos: ₡66 000 en 3 días.
   - Importadora de Partes Pesadas: ₡61 500 en 8 días.
7. `DIS-FRE-001` — Disco de freno delantero — unidad — 4 / 2 — ₡45 000.
   - Frenos y Embragues Centroamericanos: ₡44 000 en 2 días.
   - Distribuidora Automotriz Pacífico: ₡42 500 en 5 días.
8. `BAT-12V-001` — Batería 12 V 100 Ah — unidad — **2 / 3** — ₡95 000.
   - Electroauto Repuestos Eléctricos: ₡92 000 en 1 día.
   - Distribuidora Automotriz Pacífico: ₡89 500 en 4 días.
9. `LLA-225-001` — Llanta 11R22.5 para camión y bus — unidad — 8 / 6 — ₡285 000.
   - Llantas y Servicios del Norte: ₡279 000 en 3 días.
   - Importadora de Partes Pesadas: ₡268 000 en 10 días.
10. `LLA-245-001` — Llanta 245/70R16 para pickup — unidad — 10 / 8 — ₡98 000.
    - Llantas y Servicios del Norte: ₡96 500 en 2 días.
11. `AMO-DEL-001` — Amortiguador delantero — unidad — **0 / 2** — ₡52 000.
    - Suspensiones Técnicas del Este: ₡50 500 en 4 días.
    - Distribuidora Automotriz Pacífico: ₡48 900 en 7 días.
12. `REF-GAL-001` — Refrigerante para motor — galón — 15 / 8 — ₡11 500.
    - Lubricantes y Filtros Central: ₡11 200 en 1 día.
    - Distribuidora Automotriz Pacífico: ₡10 800 en 3 días.

Los stocks listados son los finales, ya descontado lo que consumen las órdenes sembradas. Los marcados en negrita quedan bajo el mínimo y aparecen como alerta en el dashboard. Además:

- El amortiguador sin stock es el que tiene a la orden 10 en espera.
- Las pastillas de freno permiten demostrar que un proveedor inactivo no entra en la recomendación.
- La llanta 245/70R16 tiene un solo proveedor.

### Órdenes de mantenimiento (12)

Formato de cada orden: código — vehículo — tipo — solicitante — estado, con sus datos principales.

1. `OM-2026-00001` — `PKP-0003` — Servicio preventivo por kilometraje — `admin` — **Solicitada**. Se generó desde la evaluación preventiva porque el vehículo superó el intervalo.
2. `OM-2026-00002` — `BUS-0001` — Reparación de aire acondicionado — `cjimenez` — **Solicitada**. Los pasajeros reportan que el aire no enfría.
3. `OM-2026-00003` — `CML-0002` — Reparación de frenos — `jchaves` — **Diagnosticada**. Prioridad Alta, 142 650 km; ruido metálico al frenar por desgaste de las zapatas traseras.
4. `OM-2026-00004` — `TRC-0001` — Cambio y alineamiento de llantas — `jchaves` — **Aprobada**. Prioridad Media, 538 600 km, programada para el 20 de octubre de 2026; desgaste irregular en el eje delantero.
5. `OM-2026-00005` — `PKP-0002` — Reparación del sistema eléctrico — `mcastro` — **Rechazada**. Prioridad Baja; la falla la causa un accesorio instalado sin autorización y se retira sin orden.
6. `OM-2026-00006` — `CMP-0001` — Reparación de motor diésel — `jchaves` — **En Ejecución**.
   - Prioridad Alta, 412 800 km.
   - Técnicos: Esteban Quirós (6 h) y Mauricio Salas (5 h).
   - Detalle: 2 filtros de combustible y 5 galones de aceite, ₡113 000.
   - Sus costos se calculan en vivo.
7. `OM-2026-00007` — `BUS-0002` — Reparación de suspensión y dirección — `cjimenez` — **Solicitada**. El bus cayó en un hueco y quedó fuera de servicio. Muestra que un vehículo Fuera de Servicio sí admite órdenes.
8. `OM-2026-00008` — `BUI-0001` — Servicio preventivo de motor diésel — `admin` — **Completada**.
   - Prioridad Media, 485 000 km.
   - Técnicos: Esteban Quirós (3,5 h) y Kevin Arias (3,5 h).
   - Detalle: 2 filtros de aceite, 1 filtro de aire, 2 filtros de combustible y 6 galones de aceite.
   - Repuestos ₡172 800, mano de obra ₡34 300, total ₡207 100.
9. `OM-2026-00009` — `PKP-0001` — Servicio preventivo por kilometraje — `admin` — **Completada**.
   - Prioridad Baja, 80 000 km.
   - Técnico: Jorge Mora (2 h).
   - Detalle: 1 filtro de aceite, 2 galones de aceite y 1 galón de refrigerante.
   - Repuestos ₡54 600, mano de obra ₡10 400, total ₡65 000.
10. `OM-2026-00010` — `CML-0001` — Reparación de suspensión y dirección — `mcastro` — **En Espera de Repuesto**.
    - Prioridad Alta, 187 300 km.
    - Técnica: Silvia Navarro (1,5 h).
    - Pendiente: 2 amortiguadores delanteros.
11. `OM-2026-00011` — `VAN-0001` — Reparación de transmisión y embrague — `rmendez` — **Cancelada** desde Aprobada. La unidad se envía a la agencia por garantía.
12. `OM-2026-00012` — `CMP-0001` — Atención de emergencia en taller — `jchaves` — **Solicitada**. Fuga de combustible detectada durante la reparación del motor. Muestra la excepción de emergencia: el vehículo ya estaba En Mantenimiento por la orden 6.

Si en el Avance 1 no alcanza el tiempo, priorizamos sembrar las órdenes 1 a 7 y 12, incluidas las asignaciones de la orden 6. Las órdenes 8 a 11 se agregan al inicio del Avance 2, recreando la base con `dotnet ef database drop` y `dotnet ef database update`.

### Coherencia de los datos sembrados

- Cada orden tiene en `HistorialEstadoOrden` todos los pasos de su recorrido, con el usuario del rol que corresponde a cada transición. Por ejemplo, la orden 8 pasa por Solicitada, Diagnosticada (`lvargas`), Aprobada (`admin`), En Ejecución (`lvargas`) y Completada (`lvargas`).
- Cada vehículo tiene en `HistorialEstadoActivo` su registro inicial como Operativo y, según su caso:
  - el paso a En Mantenimiento de `CMP-0001` y `CML-0001`;
  - la ida y vuelta a En Mantenimiento de `BUI-0001` y `PKP-0001`;
  - el paso de `BUS-0002` a Fuera de Servicio (`admin`, "Impacto en la suspensión delantera");
  - el de `ADM-0001` a Dado de Baja (`admin`, "Unidad vendida").
- Los técnicos asignados tienen la especialidad que exige el tipo de cada orden. El kilometraje del último preventivo de `BUI-0001` y `PKP-0001` coincide con el de su orden preventiva completada.
- Las órdenes completadas tienen fecha de cierre, tarifa aplicada en cada asignación y los tres costos congelados. La cancelada y la rechazada tienen fecha de cierre y no tienen costos.

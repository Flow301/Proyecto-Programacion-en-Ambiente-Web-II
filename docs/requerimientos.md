# Requerimientos funcionales

Este documento traduce el enunciado oficial del proyecto al contexto de flota vehicular. El enunciado es la fuente de verdad: si algo aquí lo contradice, manda el enunciado y corregimos este archivo.

Cada módulo de la sección 7 del enunciado tiene el mismo orden: objetivo, campos, acciones, validaciones y reglas de negocio, permisos por rol y avance en que se entrega. Donde el enunciado es ambiguo, dejamos escrita nuestra interpretación con la marca *Decisión del equipo*, para poder justificarla en la defensa.

Las fórmulas y criterios que debemos sustentar (mantenimiento preventivo, selección de proveedor, codificación y paleta) están en [decisiones técnicas](decisiones-tecnicas.md). Los estados y sus transiciones están en la [matriz de estados](matriz-estados.md), y las tablas y tipos en el [modelo de datos](modelo-datos.md).

## Roles

Usamos exactamente los nombres del enunciado, sin adaptarlos al contexto:

- **Administrador:** gestión total del sistema: flota, catálogos, usuarios, técnicos, repuestos, proveedores y órdenes. Acceso completo a reportes e historial.
- **Coordinador/Técnico:** gestión operativa de las órdenes en el taller: diagnóstico, ejecución, repuestos usados, horas y cambios de estado. No administra catálogos ni usuarios.
- **Solicitante/Consulta:** reporta necesidades de mantenimiento (por ejemplo, un supervisor de ruta), crea solicitudes y consulta el estado de sus órdenes y de los vehículos. No edita catálogos, no aprueba órdenes ni modifica información técnica.

Los permisos no son solo visuales: la API valida el rol en cada endpoint protegido.

---

## 7.1 Gestión de Activos (vehículos)

**Objetivo.** Mantener un inventario centralizado y actualizado de los vehículos de la flota: camiones, buses, pickups y similares.

**Campos.**

- Código único, generado por el sistema con el patrón `PREFIJO-0001`: prefijo de la categoría más un consecutivo por prefijo. No cambia nunca.
- Nombre descriptivo de la unidad.
- Descripción.
- Fecha de adquisición.
- Valor de referencia, en colones.
- Estado actual: Operativo, En Mantenimiento, Fuera de Servicio o Dado de Baja.
- Imagen representativa.
- Categoría.
- Atributos propios del contexto: placa, marca, modelo, año, kilometraje actual y kilometraje del último mantenimiento preventivo.
- Intervalo de mantenimiento preventivo propio, en km. Es opcional y, si existe, reemplaza al de la categoría.

**Acciones.**

- Registrar, consultar (listado paginado con imagen y detalle), actualizar y dar de baja un vehículo.
- Cargar y actualizar la imagen representativa.
- Cambiar manualmente el estado: a Fuera de Servicio, de Fuera de Servicio a Operativo, y a Dado de Baja.
- Consultar el historial de estados del vehículo.

**Validaciones y reglas de negocio.**

- El código y la placa son únicos en el sistema.
- La categoría debe existir.
- Un vehículo nunca se elimina físicamente. "Dar de baja" lo pasa al estado Dado de Baja; así se conservan sus órdenes y su historial, que no se puede borrar.
- Todo vehículo nace en estado Operativo.
- El paso a En Mantenimiento y el regreso a Operativo los hace el sistema según las órdenes; ningún usuario los ejecuta directamente. Ver la [matriz de estados](matriz-estados.md).
- Los cambios a Fuera de Servicio y Dado de Baja son acciones exclusivas del Administrador y no requieren una orden.
- *Decisión del equipo:* el Administrador puede devolver un vehículo de Fuera de Servicio a Operativo. Dado de Baja es un estado final.
- *Decisión del equipo:* el Administrador no puede cambiar el estado a mano mientras el vehículo tenga una orden En Ejecución o En Espera de Repuesto. Si pudiera, el sistema pisaría su cambio al cerrarse la orden.
- *Decisión del equipo:* no se puede dar de baja un vehículo con órdenes abiertas; primero se completan o se cancelan.
- Un vehículo En Mantenimiento no admite una nueva orden abierta, salvo que sea de un tipo de naturaleza Emergencia. Se valida al crear la orden (ver 7.5).
- El kilometraje nunca disminuye, y el kilometraje del último preventivo no puede ser mayor que el actual.
- La imagen debe ser JPG, PNG o WebP y pesar como máximo 2 MB.
- Todo cambio de estado, incluido el registro inicial, genera una entrada en el historial (ver 7.8).

**Permisos.**

- Administrador: CRUD completo y cambios manuales de estado.
- Coordinador/Técnico: consulta.
- Solicitante/Consulta: consulta.

**Avance.** El CRUD, la imagen, el código, la baja lógica, los cambios manuales con su historial y todas las validaciones van en el Avance 1. Los cambios automáticos provocados por las órdenes van en el Avance 2.

---

## 7.2 Categoría de Activo

**Objetivo.** Clasificar los vehículos (por ejemplo: pickup, camión liviano, camión pesado, bus urbano, microbús) y definir los parámetros de mantenimiento preventivo de cada tipo.

**Campos.**

- Nombre.
- Descripción.
- Prefijo para el código de los vehículos, de tres letras.
- Intervalo de mantenimiento preventivo, en km.
- Margen de aviso, en km: cuánto antes de cumplir el intervalo el vehículo se considera "próximo a vencer".

**Acciones.** Registrar, consultar, actualizar y eliminar una categoría.

**Validaciones y reglas de negocio.**

- El nombre y el prefijo son únicos.
- El intervalo es mayor que cero; el margen es mayor o igual que cero y menor que el intervalo.
- No se puede eliminar una categoría con vehículos asociados, aunque estén dados de baja. Si no tiene vehículos, se elimina físicamente.
- *Decisión del equipo:* el prefijo no se puede cambiar si la categoría ya tiene vehículos, porque sus códigos ya se generaron con ese prefijo.

**Permisos.**

- Administrador: CRUD completo.
- Coordinador/Técnico: consulta.
- Solicitante/Consulta: sin acceso.

**Avance.** Avance 1.

---

## 7.3 Técnicos y Especialidades

**Objetivo.** Administrar el personal técnico del taller y sus competencias. Un técnico puede tener varias especialidades y una especialidad puede tenerla varios técnicos.

**Campos.**

- Técnico: nombre completo, identificación (cédula), teléfono, correo, disponibilidad (disponible o no disponible), tarifa por hora en colones y estado activo o inactivo.
- Especialidad: nombre y descripción (por ejemplo: motores diésel, frenos, sistema eléctrico, suspensión y dirección).
- Por cada especialidad que posee un técnico: nivel de certificación y fecha desde la que la posee.

**Acciones.**

- Registrar, consultar, actualizar y eliminar técnicos y especialidades.
- Asociar y desasociar especialidades a un técnico.
- Consultar técnicos disponibles filtrando por especialidad.

**Validaciones y reglas de negocio.**

- La identificación del técnico es única, y el nombre de la especialidad también.
- No se puede asociar la misma especialidad dos veces al mismo técnico.
- La fecha de certificación no puede ser futura y la tarifa por hora es mayor que cero.
- *Decisión del equipo:* el enunciado pide CRUD completo (secciones 3 y 11), pero en 7.3 solo lista registrar, consultar y actualizar. Para técnicos, eliminar es inactivar, porque sus horas y asignaciones deben conservarse. Una especialidad se elimina físicamente solo si no tiene técnicos ni tipos de mantenimiento asociados; si los tiene, la operación se rechaza.
- *Decisión del equipo:* el técnico y el usuario del sistema son entidades separadas. Los técnicos no inician sesión; el Coordinador registra sus horas.
- *Decisión del equipo:* la disponibilidad la marca el Administrador (vacaciones, incapacidad). Solo se asignan técnicos activos y disponibles.
- Un técnico solo puede asignarse a una orden si posee la especialidad que requiere el tipo de mantenimiento (ver 7.7). El nivel de certificación se muestra para ayudar a elegir, pero no bloquea la asignación: el enunciado solo exige poseer la especialidad.

**Permisos.**

- Administrador: CRUD completo y asociación de especialidades.
- Coordinador/Técnico: consulta.
- Solicitante/Consulta: sin acceso.

**Avance.** Avance 1. La validación de especialidad al asignar técnicos va en el Avance 2, junto con 7.7.

---

## 7.4 Repuestos y Proveedores

**Objetivo.** Controlar el inventario de repuestos e insumos de la flota, considerando que un repuesto puede venir de varios proveedores con precios y tiempos de entrega distintos.

**Campos.**

- Repuesto: código, nombre, unidad de medida, stock actual, stock mínimo, costo unitario en colones y estado activo o inactivo.
- Proveedor: nombre, cédula jurídica, persona de contacto, teléfono, correo, dirección, condiciones generales y estado activo o inactivo.
- Por cada proveedor que suministra un repuesto: precio ofrecido y tiempo de entrega estimado en días.

**Acciones.**

- Registrar, consultar, actualizar y eliminar repuestos y proveedores.
- Asociar un repuesto a uno o varios proveedores con sus condiciones, modificarlas y desasociarlos.
- Registrar entradas de stock.
- Consultar los repuestos con stock por debajo del mínimo.
- Consultar el proveedor recomendado para un repuesto.

**Validaciones y reglas de negocio.**

- El código del repuesto y la cédula jurídica del proveedor son únicos.
- El stock no puede ser negativo bajo ninguna circunstancia. Lo validamos en el servicio y además con una restricción `CHECK` en la tabla.
- *Decisión del equipo:* el enunciado no define el costo del repuesto, pero el detalle de la orden exige "costo unitario al momento del uso". Por eso el repuesto tiene un costo unitario que mantiene el Administrador y que se copia al detalle cuando se usa.
- *Decisión del equipo:* no hay módulo de compras. El stock inicial se indica al crear el repuesto; después solo sube con "registrar entrada" (cantidad mayor que cero) y solo baja por el consumo en órdenes. No se edita el número a mano, para no pisar un descuento que esté haciendo una orden al mismo tiempo.
- El stock mínimo es mayor o igual que cero; el costo y el precio son mayores que cero; el tiempo de entrega es de al menos 1 día.
- No se puede asociar el mismo proveedor dos veces al mismo repuesto.
- "Por debajo del mínimo" significa stock actual menor que el stock mínimo.
- Para elegir proveedor, el sistema calcula un puntaje ponderado de tiempo de entrega y precio. La fórmula y su justificación están en [decisiones técnicas](decisiones-tecnicas.md).
  - Solo participan proveedores activos.
  - En empate gana el más barato.
  - Si hay un solo proveedor, ese es el recomendado; si no hay ninguno, la respuesta lo indica.
  - La recomendación es de apoyo, no genera compras. También se muestra cuando una orden queda esperando ese repuesto.
- *Decisión del equipo:* eliminar repuestos y proveedores es inactivar, porque sus datos aparecen en órdenes y asociaciones.

**Permisos.**

- Administrador: CRUD completo, asociaciones y entradas de stock.
- Coordinador/Técnico: consulta (incluye stock bajo el mínimo y proveedor recomendado) y registro de uso en órdenes (ver 7.6).
- Solicitante/Consulta: sin acceso.

**Avance.** Avance 1, incluido el proveedor recomendado. El consumo de stock en órdenes va en el Avance 2.

---

## 7.5 Orden de Mantenimiento (proceso principal)

**Objetivo.** Gestionar el ciclo de vida completo de una solicitud de mantenimiento sobre un vehículo.

**Campos.**

- Código de orden, generado con el patrón `OM-AAAA-00001` (año de la solicitud y consecutivo).
- Vehículo relacionado.
- Tipo de mantenimiento.
- Motivo de la solicitud.
- Usuario solicitante.
- Fecha de solicitud.
- Prioridad: Baja, Media, Alta o Crítica.
- Diagnóstico y kilometraje registrado en el diagnóstico.
- Fecha programada y fecha de cierre.
- Costo de repuestos, costo de mano de obra y costo total.
- Estado actual.

**Catálogo de tipos de mantenimiento.** *Decisión del equipo:* el enunciado usa "tipo" para dos cosas: la emergencia ("orden de tipo emergencia") y la especialidad ("la especialidad requerida por el tipo de mantenimiento"). Por eso cada tipo tiene un nombre, una naturaleza (Preventivo, Correctivo o Emergencia) y la especialidad que requiere. Por ejemplo: "Servicio preventivo por kilometraje" es Preventivo y requiere mecánica general; "Reparación de frenos" es Correctivo y requiere frenos; "Auxilio por avería en ruta" es Emergencia y requiere mecánica general. El catálogo se carga con los seeders y todos los roles que crean o atienden órdenes lo pueden consultar.

**Acciones.**

- Crear una solicitud.
- Consultar órdenes: listado paginado y detalle.
- Editar una solicitud mientras está Solicitada.
- Diagnosticar.
- Aprobar o rechazar.
- Iniciar la ejecución asignando técnicos.
- Registrar repuestos y horas.
- Reanudar una orden en espera.
- Completar.
- Cancelar, con observación opcional.
- Consultar el historial de estados de la orden.

**Validaciones y reglas de negocio.**

- Toda orden queda vinculada a un único vehículo existente. *Decisión del equipo:* no se crean órdenes para vehículos Dado de Baja; para vehículos Fuera de Servicio sí, porque así vuelve a operar una unidad dañada.
- Si el vehículo está En Mantenimiento, solo se puede crear una orden cuyo tipo sea de naturaleza Emergencia.
  - *Decisión del equipo:* el enunciado habla de una "nueva" orden, así que se valida al crear; las órdenes que ya estaban abiertas siguen su curso.
- Toda orden nace en estado Solicitada.
- Cada cambio de estado se valida contra la [matriz de transiciones](matriz-estados.md). Una transición no permitida se rechaza con una excepción de negocio propia, no con un error genérico.
- Cada cambio de estado genera automáticamente su registro en el historial, con observación opcional.
- En el diagnóstico, el Coordinador define la prioridad y registra el kilometraje del vehículo.
  - Ese kilometraje no puede ser menor que el actual del vehículo, y al registrarlo actualiza el del vehículo.
  - *Decisión del equipo:* en el diagnóstico el Coordinador también puede corregir el tipo de mantenimiento.
- Al aprobar, el Administrador fija la fecha programada. Es obligatoria y no puede estar en el pasado.
- Iniciar la ejecución exige asignar al menos un técnico (ver 7.7).
- Solo se completa una orden sin repuestos pendientes y con al menos un técnico con horas registradas.
- El costo total combina el detalle de repuestos y la mano de obra:
  - Costo de repuestos: suma de los subtotales del detalle.
  - Costo de mano de obra: horas por tarifa de cada técnico.
  - Mientras la orden está abierta, los costos se calculan en vivo.
  - Al completar, o al cancelar una orden que tuvo ejecución, se congela la tarifa en cada asignación y se guardan los tres costos.
- La fecha de cierre se registra en cualquier estado final: Completada, Cancelada o Rechazada.
- Al completar una orden de naturaleza Preventivo, el kilometraje del último preventivo del vehículo pasa a ser el registrado en esa orden.
- Una orden nunca se elimina físicamente; para eso existe Cancelar, disponible desde cualquier estado no final.
- *Decisión del equipo:* en el Avance 1, sin JWT, el usuario responsable viaja en el cuerpo de la petición. Desde el Avance 2 se toma del token, y solo cambia el controlador.

**Permisos.**

- Administrador: crear, consultar todas, editar mientras está Solicitada, aprobar o rechazar, y cancelar.
- Coordinador/Técnico: consultar todas, diagnosticar, iniciar ejecución, registrar repuestos y horas, reanudar, completar y cancelar. No crea órdenes.
- Solicitante/Consulta: crear y consultar solo las suyas. Ve el estado y el historial, pero no repuestos, horas ni costos.

**Avance.** El modelo completo, crear y consultar van en el Avance 1, con la validación del vehículo, del bloqueo por En Mantenimiento y del código. El resto del ciclo de vida va en el Avance 2.

---

## 7.6 Detalle de Repuestos Utilizados

**Objetivo.** Registrar qué repuestos e insumos se usaron en una orden.

**Campos.**

- Detalle: repuesto, cantidad, costo unitario al momento del uso, subtotal y fecha de registro.
- Pendientes: repuesto y cantidad que no se pudo surtir.

**Acciones.**

- Registrar uno o varios repuestos usados en una orden.
- Consultar el detalle de repuestos de una orden.

**Validaciones y reglas de negocio.**

- Solo se registran repuestos con la orden En Ejecución. El repuesto debe estar activo y la cantidad debe ser mayor que cero.
- Antes de registrar se valida el stock disponible.
- Si alcanza, se crea la línea con el costo unitario vigente y se descuenta el stock en la misma operación.
- Si no alcanza:
  - *Decisión del equipo:* la línea completa queda como pendiente, sin surtidos parciales.
  - La orden pasa automáticamente a En Espera de Repuesto.
  - La API responde con el nuevo estado y un mensaje que indica qué falta. No hay stock negativo ni fallo silencioso.
- Al reanudar la orden, todos los pendientes deben tener stock. Se descuentan en una sola operación y la orden vuelve a En Ejecución; si falta algo, la reanudación se rechaza indicando qué repuesto falta.
- *Decisión del equipo:* cancelar una orden no devuelve el stock automáticamente y sus costos quedan registrados. Si las piezas no se llegaron a instalar, el Administrador registra una entrada.

**Permisos.**

- Coordinador/Técnico: registrar y consultar.
- Administrador: consultar.
- Solicitante/Consulta: sin acceso.

**Avance.** Las tablas van en el Avance 1 y las operaciones en el Avance 2.

---

## 7.7 Asignación de Técnicos a la Orden

**Objetivo.** Registrar qué técnicos participaron en una orden y cuántas horas dedicó cada uno, como base del costo de mano de obra y de la carga técnica.

**Campos.** Técnico, fecha de asignación, horas trabajadas y tarifa aplicada (se congela al cerrar la orden).

**Acciones.**

- Asignar uno o varios técnicos a una orden.
- Registrar las horas de cada técnico asignado.
- Consultar técnicos y horas de una orden.

**Validaciones y reglas de negocio.**

- *Decisión del equipo:* la sección 8 dice que los técnicos se asignan antes de iniciar y la 7.7 que se asignan a una orden En Ejecución. Iniciar la ejecución recibe al menos un técnico y hace las dos cosas en una sola operación; mientras la orden siga En Ejecución se pueden agregar más.
- Solo se asigna un técnico que posea la especialidad requerida por el tipo de mantenimiento de la orden, y que esté activo y disponible.
- Un técnico no se asigna dos veces a la misma orden.
- Solo se registran horas en una orden En Ejecución. Cada registro se suma a las horas del técnico en esa orden y debe ser mayor que cero y de como máximo 24 horas.
- Las horas por la tarifa de cada técnico determinan el costo de mano de obra (ver 7.5).

**Permisos.**

- Coordinador/Técnico: asignar técnicos y registrar horas.
- Administrador: consulta.
- Solicitante/Consulta: sin acceso.

**Avance.** Las tablas van en el Avance 1 y las operaciones en el Avance 2.

---

## 7.8 Historial de Estados

**Objetivo.** Mantener la trazabilidad completa de los cambios de estado de los vehículos y de las órdenes.

**Campos.** Estado anterior, estado nuevo, fecha y hora del cambio, usuario responsable y observación opcional. Llevamos dos historiales separados: uno de vehículos y otro de órdenes.

**Acciones.**

- Consultar el historial de estados de un vehículo.
- Consultar el historial de estados de una orden.

**Validaciones y reglas de negocio.**

- El registro se genera automáticamente en cada cambio de estado real. No existe ningún endpoint para insertarlo, editarlo ni eliminarlo.
- El historial es de solo lectura para todos los roles.
- *Decisión del equipo:* al crear un vehículo o una orden se registra su estado inicial, con estado anterior vacío, porque la línea de tiempo del vehículo debe ir desde su registro.
- Si el estado no cambia de verdad, no se registra nada. Por ejemplo, un vehículo que ya estaba En Mantenimiento por otra orden.
- El usuario responsable es quien ejecutó la acción que provocó el cambio, también cuando el cambio del vehículo es automático.
- Los registros que cargan los seeders son coherentes con el estado de cada orden sembrada.

**Permisos.**

- Administrador: consulta completa.
- Coordinador/Técnico: consulta completa.
- Solicitante/Consulta: solo el historial de sus órdenes y de los vehículos relacionados con ellas.

**Avance.** El historial de vehículos (registro inicial y cambios manuales) y su consulta van en el Avance 1. El historial de órdenes, los cambios automáticos y su vista en el frontend van en el Avance 2.

---

## 7.9 Mantenimiento Preventivo

**Objetivo.** Anticipar el mantenimiento de cada vehículo según su uso real, medido en kilometraje.

**Campos.**

- En la categoría: intervalo y margen de aviso, en km.
- En el vehículo: kilometraje actual, kilometraje del último preventivo e intervalo propio opcional.

**Acciones.**

- Configurar la regla por categoría o por vehículo.
- Evaluar bajo demanda qué vehículos cumplen o están próximos a cumplir su umbral.
- Generar una orden preventiva a partir de una sugerencia.

**Validaciones y reglas de negocio.**

- Cada vehículo queda Vencido, Próximo a vencer o Al día según la fórmula de [decisiones técnicas](decisiones-tecnicas.md). Si el vehículo tiene intervalo propio, ese reemplaza al de la categoría.
- *Decisión del equipo:* la variable es solo el kilometraje, sin combinarla con tiempo.
- *Decisión del equipo:* "bajo demanda" significa que se calcula en cada consulta, sin procesos programados.
- Se excluyen los vehículos Dado de Baja.
- Si el vehículo ya tiene una orden preventiva abierta, aparece como "orden en curso" y no se sugiere otra.
- *Decisión del equipo:* el sistema sugiere y el Administrador confirma. La orden generada nace Solicitada, con un tipo de naturaleza Preventivo y con el Administrador como solicitante.
- Al completarse una orden preventiva, el contador del vehículo se reinicia (ver 7.5).

**Permisos.**

- Administrador: configurar reglas, ejecutar la evaluación y generar órdenes.
- Coordinador/Técnico: consulta de alertas.
- Solicitante/Consulta: sin acceso.

**Avance.** Los parámetros en categoría y vehículo van en el Avance 1. La evaluación, las alertas y la generación de órdenes van en el Avance 2.

---

## 7.10 Gestión de Usuarios y Roles

**Objetivo.** Administrar las cuentas de acceso y sus roles, para que cada usuario autenticado tenga solo los permisos de su rol.

**Campos.** Nombre completo, nombre de usuario (login), correo, contraseña (guardada como hash), rol, estado activo o inactivo, fecha de creación y fecha del último acceso.

**Acciones.**

- Registrar, consultar, actualizar, desactivar y reactivar usuarios.
- Asignar o cambiar el rol.
- Iniciar sesión con credenciales válidas y obtener un token JWT.
- Cerrar sesión.
- Mostrar solo las pantallas y opciones de menú del rol autenticado.

**Validaciones y reglas de negocio.**

- El nombre de usuario y el correo son únicos.
- La contraseña nunca se guarda en texto plano: usamos `PasswordHasher` (PBKDF2 con *salt*).
- Los usuarios nunca se eliminan físicamente, solo se desactivan, porque tienen actividad registrada (órdenes, cambios de estado).
- Un usuario inactivo no puede iniciar sesión aunque sus credenciales sean correctas.
  - Caso borde: un token emitido antes de la desactivación sigue siendo válido hasta que expira.
- Todo endpoint protegido valida el rol del token: responde 401 si no hay token o es inválido, y 403 si el rol no tiene permiso.
- El seeder crea al menos un Administrador (`admin`).
- *Decisión del equipo:* no se puede desactivar ni quitarle el rol al último Administrador activo, para que el sistema nunca quede sin acceso administrativo.
- El último acceso se actualiza en cada inicio de sesión. El cierre de sesión se hace del lado del cliente, descartando el token.

**Permisos.**

- Administrador: CRUD completo y asignación de roles.
- Coordinador/Técnico: sin acceso, salvo iniciar y cerrar su propia sesión.
- Solicitante/Consulta: sin acceso, salvo iniciar y cerrar su propia sesión.

**Avance.** Las entidades Usuario y Rol y el seed (administrador y usuarios de demostración de los tres roles) van en el Avance 1, porque órdenes e historial los referencian. El CRUD, el login, el JWT y la autorización por rol van en el Avance 2.

---

## Proceso principal de la orden (sección 8)

La orden recorre este camino:

1. **Solicitada:** la crea un Solicitante o el Administrador.
2. **Diagnosticada:** el Coordinador la revisa y define alcance, prioridad y kilometraje.
3. **Aprobada** con fecha programada, o **Rechazada:** lo decide el Administrador.
4. **En Ejecución:** el Coordinador asigna técnicos e inicia el trabajo, y el vehículo pasa a En Mantenimiento.
5. **En Espera de Repuesto:** pasa sola si falta stock; vuelve a En Ejecución cuando el Coordinador la reanuda con stock suficiente.
6. **Completada:** el Coordinador la cierra, se congelan los costos y el vehículo vuelve a Operativo.

Además, la orden puede **cancelarse** desde cualquier estado no final.

*Decisión del equipo:* la matriz de la sección 9 no incluye cancelar desde En Espera de Repuesto, pero la sección 8 permite cancelar "en cualquier punto previo al cierre". Como la matriz es un mínimo, agregamos esa transición.

*Decisión del equipo:* el vehículo vuelve a Operativo solo cuando ninguna otra de sus órdenes está En Ejecución o En Espera de Repuesto. Si una orden se cancela antes de ejecutarse, el vehículo conserva su estado.

El detalle de cada transición, su rol y sus efectos está en la [matriz de estados](matriz-estados.md).

## Reportes y dashboard (sección 10)

Todos los totales, conteos y promedios se calculan en el backend con consultas LINQ que agregan varias tablas. El frontend solo los muestra.

- **Dashboard** (Administrador): es la vista principal al ingresar. Muestra:
  - la cantidad de órdenes por estado;
  - los vehículos con preventivo vencido o próximo a vencer;
  - los repuestos con stock bajo el mínimo.
- **Reporte de costos por orden** (Administrador y Coordinador/Técnico): una orden o un rango por fecha de cierre, filtrable por tipo y prioridad.
  - Muestra el subtotal de repuestos, el costo de mano de obra y el total.
  - *Decisión del equipo:* como el enunciado no define "lo esperado", junto a cada orden mostramos el costo promedio de las órdenes completadas del mismo tipo y prioridad.
- **Reporte de historial de un vehículo** (Administrador): línea de tiempo con sus cambios de estado y las órdenes de cada período, desde su registro hasta la fecha de consulta.
- **Reporte de carga técnica** (Administrador y Coordinador/Técnico): para un período, la cantidad de órdenes distintas por técnico con asignaciones en ese período y el total de horas.

**Avance.** Avance 2.

## Interfaz y experiencia de usuario (sección 12)

Todo esto corresponde al Avance 2.

- **Navegación y sesión.**
  - El menú muestra solo lo que corresponde al rol e indica la sección activa.
  - En todas las pantallas se ven el usuario autenticado, su rol y la opción de cerrar sesión.
  - Si el token expira o es inválido, la aplicación vuelve al login con un mensaje explicativo.
- **Estados.**
  - El flujo de la orden se ve con un indicador de progreso o etiquetas de estado.
  - Los estados de orden y de vehículo usan el mismo lenguaje visual y siempre combinan color con ícono y texto.
- **Formularios.**
  - Validan en el cliente antes de enviar, sin reemplazar la validación del servidor.
  - El botón de envío se deshabilita mientras la operación está en curso.
  - Dar de baja un vehículo, rechazar o cancelar una orden y desactivar un usuario piden confirmación explícita.
- **Retroalimentación.**
  - Hay un indicador de carga en cada operación contra la API.
  - Cada operación muestra un mensaje de éxito o un error comprensible, nunca un código HTTP crudo.
- **Listados.**
  - El listado de vehículos muestra la imagen.
  - Las alertas de stock bajo y de preventivo se ven en el dashboard sin navegar.
  - Hay una vista de historial desde el detalle de cada vehículo y de cada orden.
  - Las listas vacías o los filtros sin resultados muestran un mensaje claro.
  - Los listados extensos se paginan igual que en el backend.
- **Diseño.** La interfaz es usable en escritorio y tablet, y aplica de forma consistente la paleta definida en [decisiones técnicas](decisiones-tecnicas.md).

## Restricciones transversales

- El frontend no muestra datos simulados ni fijos: toda la información viene de nuestra API, a través de EF Core, desde SQL Server. Los datos de prueba solo entran por seeders o migraciones.
- React consume exclusivamente nuestra API REST.
- Totales, conteos y promedios se calculan en el backend con LINQ.
- No usamos paquetes NuGet ni npm deprecados, sin mantenimiento o con vulnerabilidades conocidas.
- Toda transición de estado no permitida se rechaza con una excepción de negocio propia.

## Fuera de alcance

Salvo que el profesor lo pida, no implementamos:

- órdenes de compra;
- devoluciones ni ajustes de inventario distintos de las entradas;
- edición o eliminación del detalle de repuestos de una orden;
- lecturas de odómetro como entidad propia;
- refresh tokens ni revocación de tokens;
- notificaciones;
- procesos programados.

---

## Checklist de cobertura por avance

Un módulo cuenta como cubierto solo si funciona de punta a punta y sin errores: CRUD completo, relaciones operativas, validaciones y reglas de negocio de ese avance. Un módulo a medias no cuenta. Si un avance queda por debajo del 50 % de cobertura real, se califica con 0 y no tiene defensa.

### Avance 1 — semana 7

**Base técnica**

- [ ] Solución con las capas de clase: Domain, Infrastructure, Application, Shared y WebAPI.
- [ ] Migraciones aplicadas en SQL Server con todas las tablas del modelo, incluidas las de la orden.
- [ ] Seeders: catálogos con `HasData`, administrador con `MasterSeeder` y datos de flota con `DevelopmentSeeder`, con 8 a 12 registros por tabla principal.
- [ ] Manejo global de excepciones con respuestas claras en español.
- [ ] Validaciones de entrada en todos los DTO de creación y edición.
- [ ] Listados paginados con el total de registros.
- [ ] API documentada y probada en Scalar.

**Activos**

- [ ] CRUD completo, con baja lógica (Dado de Baja).
- [ ] Código `PREFIJO-0001` generado, único e inmutable; placa única.
- [ ] Carga y actualización de la imagen, con validación de tipo y tamaño.
- [ ] Cambios manuales del Administrador con sus restricciones.
- [ ] Historial del vehículo (registro inicial y cambios manuales) y su consulta.
- [ ] Kilometraje que nunca disminuye.

**Categorías**

- [ ] CRUD completo, con nombre y prefijo únicos y parámetros de preventivo validados.
- [ ] No se elimina una categoría con vehículos ni se cambia su prefijo si tiene vehículos.

**Técnicos y especialidades**

- [ ] CRUD de técnicos con inactivación e identificación única.
- [ ] CRUD de especialidades, sin eliminar las que tienen relaciones.
- [ ] Asociar y desasociar especialidades con nivel y fecha, sin duplicados.
- [ ] Consulta de técnicos disponibles por especialidad.

**Repuestos y proveedores**

- [ ] CRUD de repuestos con inactivación, código único y stock nunca negativo.
- [ ] CRUD de proveedores con inactivación y cédula jurídica única.
- [ ] Asociación repuesto–proveedor con precio y tiempo de entrega, sin duplicados.
- [ ] Registro de entradas de stock.
- [ ] Consulta de repuestos bajo el mínimo.
- [ ] Proveedor recomendado por puntaje ponderado.

**Orden de mantenimiento**

- [ ] Modelo completo: orden, tipos, prioridades, detalle, pendientes, asignaciones e historial.
- [ ] Crear solicitud en estado Solicitada con código `OM-AAAA-00001`.
- [ ] Validación de vehículo existente y no dado de baja, tipo existente y solicitante activo.
- [ ] Bloqueo por En Mantenimiento, salvo naturaleza Emergencia.
- [ ] Consulta de órdenes: listado paginado y detalle.

**Usuarios**

- [ ] Entidades Usuario y Rol con seed del administrador y de usuarios de los tres roles.

### Avance 2 — semana 13

**Ciclo de vida de la orden**

- [ ] Diagnosticar, aprobar, rechazar, iniciar, reanudar, completar y cancelar según la matriz y los roles.
- [ ] Excepción de negocio propia en toda transición no permitida.
- [ ] Edición solo en estado Solicitada.
- [ ] Cambios automáticos del vehículo a En Mantenimiento y de vuelta a Operativo, con sus casos borde.
- [ ] Kilometraje en el diagnóstico y reinicio del contador al completar una preventiva.
- [ ] Costos en vivo y congelados al cierre.

**Detalle de repuestos**

- [ ] Registro con validación de stock y descuento en la misma operación.
- [ ] Paso automático a En Espera de Repuesto con pendientes y mensaje claro.
- [ ] Reanudación que consume los pendientes o se rechaza indicando qué falta.

**Asignación de técnicos**

- [ ] Asignación con validación de especialidad, actividad y disponibilidad.
- [ ] Registro de horas solo En Ejecución.
- [ ] Costo de mano de obra con la tarifa congelada al cierre.

**Historial**

- [ ] Historial automático de órdenes y de los cambios automáticos del vehículo.
- [ ] Solo lectura para todos los roles y visible desde el detalle en el frontend.

**Mantenimiento preventivo**

- [ ] Evaluación bajo demanda con estados Vencido, Próximo a vencer y Al día.
- [ ] Exclusión de vehículos dados de baja y detección de órdenes preventivas en curso.
- [ ] Generación de la orden preventiva confirmada por el Administrador.

**Usuarios y seguridad**

- [ ] CRUD de usuarios con desactivación, reactivación y asignación de rol.
- [ ] Login con JWT, logout y actualización del último acceso.
- [ ] Usuario inactivo sin acceso; protección del último Administrador activo.
- [ ] Autorización por rol en todos los endpoints, con 401 y 403.

**Reportes y dashboard**

- [ ] Dashboard del Administrador.
- [ ] Reporte de costos por orden.
- [ ] Reporte de historial de un vehículo.
- [ ] Reporte de carga técnica.

**Frontend**

- [ ] React consumiendo la API en todos los módulos, sin datos fijos.
- [ ] Menú y pantallas según el rol, y manejo de token expirado.
- [ ] Requerimientos de interfaz de la sección 12.
- [ ] Paleta de colores aplicada de forma consistente.

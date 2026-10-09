# Plan de trabajo

Plan semanal desde la semana 4 hasta la entrega final en la semana 13. Las tareas están repartidas entre Integrante A e Integrante B para que los dos tengamos commits propios todas las semanas, y están ordenadas para terminar cada módulo de punta a punta antes de empezar el siguiente.

**Pendiente:** definir quién es Integrante A y quién Integrante B, y anotarlo en el README.

## Cómo usamos este plan

- **Una viñeta, al menos un commit.** Cada viñeta es una tarea que termina en uno o más commits del responsable, siguiendo las [convenciones](convenciones.md).
- **Módulos completos.** Cada uno lleva sus módulos completos: entidad, configuración, migración, seeder, repositorio, servicio, controlador y validaciones. Un módulo se marca en la [checklist](requerimientos.md#checklist-de-cobertura-por-avance) solo cuando cumple la [definición de terminado](convenciones.md#7-cuándo-un-módulo-está-terminado).
- **Repaso cruzado semanal.** Una vez por semana, cada uno le explica al otro lo que hizo, durante unos 30 minutos. En la defensa cualquiera de los dos puede tener que explicar o modificar código que escribió el otro.
- **Integración semanal.** Al cerrar cada semana, uno de los dos integra `develop` en `main`, por turnos, y los dos revisan Insights → Contributors en GitHub para confirmar que ambos tienen actividad.
- **Si una semana se atrasa,** primero se terminan los módulos que exige el avance y después lo adelantado de avances posteriores. Nunca dejamos un módulo a medias para empezar otro.

---

## Avance 1: semanas 4 a 7

Objetivo: la API completa del Avance 1, probada en Scalar, con más del 50 % de cobertura real y margen.

### Semana 4 (actual): base del proyecto

**Integrante A**

- Crear la solución `SGMA.slnx` con los cinco proyectos, sus referencias y sus paquetes, según las [convenciones](convenciones.md). Subirla lo antes posible, porque B depende de ella.
- Configurar `Program.cs`: `AppDbContext` con SQL Server, User Secrets (`ConnectionStrings:DefaultConnection` y `Seed:AdminPassword`), Mapster, OpenAPI y Scalar.
- Crear las entidades de catálogo (`Rol`, `EstadoActivo`, `EstadoOrden`, `Prioridad`, `NaturalezaMantenimiento`, `NivelCertificacion`) con su configuración y su `HasData`, `SeedIds` y la primera migración.

**Integrante B**

- Subir `docs/requerimientos.md`.
- Crear las excepciones de negocio en `SGMA.Shared/Exceptions` y el manejador global con `IExceptionHandler` y ProblemDetails en español.
- Crear `PagedResult<T>` en `Domain/DTO` y probar el manejador con un endpoint de prueba que luego se elimina.

**Juntos**

- Compartir el enlace del repositorio con el profesor.
- Integrar `develop` en `main` con los commits de los dos.

### Semana 5: categorías, especialidades y técnicos

**Integrante A**

- **Módulo Categorías completo:**
  - entidad y configuración, con nombre y prefijo únicos y los `CHECK` de intervalo y margen;
  - `HasData` con las 10 categorías;
  - repositorio, servicio y controlador con CRUD y paginación;
  - no eliminar con vehículos y no cambiar el prefijo si tiene vehículos.
- **Inicio del módulo Activos:**
  - entidad y configuración, con código y placa únicos y los `CHECK` de kilometraje;
  - CRUD básico con el código `PREFIJO-0001` generado.

**Integrante B**

- **Módulo Especialidades completo:**
  - entidad, `HasData` con las 10 especialidades y CRUD;
  - no eliminar si tiene relaciones.
- **Módulo Técnicos completo:**
  - entidad, CRUD con inactivación e identificación única;
  - tabla `TecnicoEspecialidad` para asociar y desasociar, con nivel y fecha y sin duplicados;
  - consulta de técnicos disponibles por especialidad;
  - los 10 técnicos y sus 21 asociaciones en el `DevelopmentSeeder`.

### Semana 6: vehículos, repuestos y proveedores

**Integrante A**

- **Terminar Activos:**
  - baja lógica (`DELETE` como Dado de Baja);
  - cambios manuales de estado con sus restricciones;
  - `HistorialEstadoActivo` con su registro inicial y su consulta;
  - validación del kilometraje.
- Subida y actualización de la imagen, con validación de tipo y tamaño. Conseguir las 12 fotos.
- `Usuario` y `Rol` con `MasterSeeder` (`admin`) y los usuarios de demostración en el `DevelopmentSeeder`.
- Los 12 vehículos en el `DevelopmentSeeder`, con sus historiales.

**Integrante B**

- **Módulo Proveedores completo:** CRUD con inactivación y cédula jurídica única, más los 10 proveedores en el seeder.
- **Módulo Repuestos completo:**
  - CRUD con inactivación y código único;
  - `CHECK` de stock;
  - registro de entradas de stock;
  - consulta de repuestos bajo el mínimo.
- Tabla `RepuestoProveedor` para asociar, modificar y desasociar, con las 24 asociaciones en el seeder.
- Proveedor recomendado con el puntaje ponderado de [decisiones técnicas](decisiones-tecnicas.md).

### Semana 7: orden de mantenimiento y entrega del Avance 1

**Integrante A**

- **Modelo completo de la orden:**
  - `TipoMantenimiento` con `HasData`;
  - `OrdenMantenimiento`, `DetalleRepuesto`, `RepuestoPendiente`, `AsignacionTecnico` y `HistorialEstadoOrden`, con su migración.
- **Crear y consultar órdenes:**
  - código `OM-AAAA-00001`;
  - validación del vehículo (que exista y no esté dado de baja), del tipo y del solicitante;
  - bloqueo por En Mantenimiento salvo emergencia;
  - registro inicial en el historial;
  - listado paginado y detalle.

**Integrante B**

- Las órdenes en el `DevelopmentSeeder`, coherentes con sus historiales, asignaciones y detalle. Como mínimo las órdenes 1 a 7 y 12; el resto, si alcanza el tiempo.
- Revisar que todos los listados paginen con total y que todos los errores respondan con ProblemDetails en español.
- Recorrer la checklist del Avance 1 en Scalar sobre una base recreada desde cero, y anotar lo que falle.

**Juntos**

- Corregir lo que salga en el recorrido.
- Repaso cruzado completo, en el que cada uno explica los módulos del otro.
- Integrar en `main` y crear la etiqueta `avance-1`.
- Pedir al profesor el material de JWT y de React para el Avance 2.

---

## Avance 2: semanas 8 a 13

Objetivo: el ciclo de vida completo de la orden, la seguridad con JWT, el frontend en React, el historial visible, los reportes y el dashboard.

### Semana 8: seguridad y primera parte del ciclo de vida

**Integrante A**

- Login con JWT: servicio de tokens, clave en User Secrets, actualización del último acceso y rechazo de usuarios inactivos.
- CRUD de usuarios con desactivación, reactivación y cambio de rol, protegiendo al último Administrador activo.
- Autorización por rol en todos los endpoints existentes (401 y 403). Los controladores pasan a tomar el usuario del token en vez del cuerpo de la petición.

**Integrante B**

- `MatrizTransicionesOrden` con roles.
- Diagnosticar (prioridad, kilometraje y corrección del tipo), aprobar (fecha programada), rechazar y cancelar antes de la ejecución, cada una con su historial.
- Completar el seeder de órdenes si quedó pendiente, y recrear la base.

### Semana 9: frontend base y segunda parte del ciclo de vida

**Integrante A**

- Crear el proyecto React con Vite en `frontend/`, con la estructura que indique el profesor.
- Módulo cliente de la API con el token, login, cierre de sesión y redirección al login cuando el token expira.
- Plantilla general: menú según el rol con la sección activa, usuario y rol visibles, y la paleta como variables CSS.

**Integrante B**

- Iniciar la ejecución con la asignación de técnicos (especialidad, actividad y disponibilidad) y el paso automático del vehículo a En Mantenimiento.
- Registrar repuestos con descuento de stock y pendientes, pasando a En Espera de Repuesto, y reanudar.
- Registrar horas, completar (costos y tarifa congelados, reinicio del preventivo, vehículo de vuelta a Operativo) y cancelar desde la ejecución.

### Semana 10: pantallas de catálogos y mantenimiento preventivo

**Integrante A**

- Pantallas de Categorías, Especialidades y Técnicos, incluida la asociación de especialidades.
- Pantallas de Proveedores y Repuestos, con asociaciones, entradas de stock y proveedor recomendado.

**Integrante B**

- Evaluación del mantenimiento preventivo en el backend y generación de la orden preventiva confirmada por el Administrador.
- Endpoint del dashboard: órdenes por estado, alertas de preventivo y stock bajo el mínimo.
- Pantallas de Vehículos: listado con imagen, detalle, historial y cambios de estado.

### Semana 11: pantallas de órdenes y reportes

**Integrante A**

- Pantallas de órdenes: listado, solicitud, detalle con indicador de estado e historial, y las acciones de cada rol (diagnosticar, aprobar, iniciar, registrar repuestos y horas, reanudar, completar, cancelar).

**Integrante B**

- Reportes en el backend: costos por orden con el promedio de referencia, historial de un vehículo y carga técnica, todos con LINQ.
- Pantalla del dashboard.

### Semana 12: pantallas restantes y experiencia de usuario

**Integrante A**

- Pantallas de usuarios y de mantenimiento preventivo.
- Repaso de los requerimientos de interfaz:
  - confirmaciones;
  - indicadores de carga;
  - botones deshabilitados durante el envío;
  - mensajes de éxito y de error;
  - listas vacías y paginación.

**Integrante B**

- Pantallas de los tres reportes.
- Prueba en tablet y escritorio, y revisión de contraste de la paleta.
- Recorrido de la checklist del Avance 2 sobre una base recreada desde cero, anotando lo que falle.

### Semana 13: cierre y entrega final

**Integrante A**

- Corregir lo que salga en el recorrido, en sus módulos.
- Completar las secciones pendientes del README: frontend, login y usuarios de prueba.

**Integrante B**

- Corregir lo que salga en el recorrido, en sus módulos.
- Revisión final de dependencias (`dotnet list package --vulnerable` y `npm audit`) y de que no queden datos fijos en React.

**Juntos**

- Repaso cruzado completo de todo el sistema y ensayo de la defensa.
- Integrar en `main` y crear la etiqueta `avance-2`.

---

## Pendientes que no son código

- Conseguir las 12 fotos de los vehículos (semana 6, Integrante A).
- Pedir al profesor el material de escritura en el CRUD, validaciones, manejo de excepciones, JWT y React (lo antes posible).
- Definir quién es Integrante A y quién Integrante B (semana 4).

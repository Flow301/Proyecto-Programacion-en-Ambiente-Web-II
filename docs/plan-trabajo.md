# Plan de trabajo

Plan semanal desde la semana 4 hasta la entrega final en la semana 13. Las tareas están repartidas entre Integrante A e Integrante B para que los dos tengamos commits propios todas las semanas, y están ordenadas para terminar cada módulo de punta a punta antes de empezar el siguiente.

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

- Copiar del material `Result<T>` y `BaseResult` en `Domain/DTO/PatternResult`.
- Crear las excepciones de negocio en `SGMA.Shared/Exceptions` y el manejador global con `IExceptionHandler`, que responde con `Result<T>.Failure` en español (ver [convenciones](convenciones.md#respuestas-y-errores)).
- Probar el manejador con un endpoint de prueba que luego se elimina.

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
  - los 10 técnicos y sus 20 asociaciones en el `DevelopmentSeeder`.

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
- Revisar que todos los listados paginen con `Result<T>.Paged` y que todos los errores respondan con `Result<T>` en español.
- Recorrer la checklist del Avance 1 en Scalar sobre una base recreada desde cero, y anotar lo que falle.

### Semana 7 (continuación): usuarios, login y JWT

El profesor pidió el módulo de usuarios y la seguridad para el Avance 1, así que se adelantan de la semana 8. Lo que ya existe y lo que falta está en [requerimientos, 7.10](requerimientos.md#710-gestión-de-usuarios-y-roles); las reglas técnicas, en [convenciones](convenciones.md#autenticación-y-autorización).

**Integrante A (hecho)**

- Paquete JwtBearer, sección `Jwt` en `appsettings.json` y secreto `Jwt:Key` (README, paso 2).
- `Program.cs`: autenticación JWT, política por defecto que exige token, 401 y 403 con `Result<T>`, y esquema Bearer en Scalar.
- `Roles.cs`, `ITokenService` y `TokenService`, y `ClaimsPrincipalExtensions` (`User.GetIdUsuario()`).
- `AuthService` y `AuthController`: `POST api/Auth/login`, `GET api/Auth/perfil` y `PUT api/Auth/clave` (cambio de la contraseña propia).
- En `IUsuarioRepository`: `GetByIdAsync` (ahora con el rol), `GetByLoginForUpdateAsync`, `GetForUpdateAsync` y `SaveChangesAsync`. Además, `UsuarioDto` y `CambioClaveDto`.
- El usuario responsable sale del token: se quitó `IdUsuario` de `ActivoCreateDto`, `ActivoEstadoDto`, `ActivoBajaDto` y `OrdenMantenimientoCreateDto`, y el campo `idUsuario` de `PUT api/Activo/{id}/imagen`.
- Roles en `ActivoController`, `CategoriaActivoController`, `EspecialidadController` y `TecnicoController`.

**Integrante B**

1. **CRUD de usuarios.** Ya existen `UsuarioDto` (salida) y los métodos de `IUsuarioRepository` de la lista anterior; se reutilizan, no se duplican.
   - **DTO nuevos** en `SGMA.Domain/DTO`, con DataAnnotations del mismo largo que las columnas (`NombreCompleto` 100, `Login` 50, `Correo` 100) y `<summary>` y `<example>` en cada propiedad:
     - `UsuarioCreateDto`: `NombreCompleto`, `Login`, `Correo` (`[EmailAddress]`), `Clave` (8 a 100 caracteres) e `IdRol`.
     - `UsuarioUpdateDto`: `NombreCompleto`, `Login` y `Correo`. No cambia el rol, la clave ni el estado, porque cada uno tiene su endpoint.
     - `UsuarioRolDto`: `IdRol`.
     - `UsuarioClaveDto`: `Clave` (8 a 100 caracteres).
     - `RolDto`: `IdRol` y `Nombre`.
   - **Repositorio** (`IUsuarioRepository` y `UsuarioRepository`):
     - Lectura: `GetAllAsync` y `GetPaginationAsync` (con el rol, incluidos los inactivos, ordenados por `IdUsuario`), `CountAsync` y `GetRolesAsync`.
     - Verificaciones: `ExistsByLoginAsync(login, excludeId)`, `ExistsByCorreoAsync(correo, excludeId)`, `RolExistsAsync(idRol)` y `CountAdministradoresActivosAsync()`.
     - Escritura: `AddAsync`.
   - **Mapster:** `UsuarioCreateDto → Usuario` ignorando `IdUsuario`, `ClaveHash`, `Estado`, `FechaCreacion`, `UltimoAcceso` y `Rol`, y `UsuarioUpdateDto → Usuario` ignorando también `IdRol`. `Usuario → UsuarioDto` no necesita configuración (`RolNombre ← Rol.Nombre`).
   - **`IUsuarioService` y `UsuarioService`**, con `ILogger` como los demás servicios. Para el hash, `PasswordHasher<Usuario>`, igual que `AuthService`.
     - `GetAllAsync`, `GetPaginationAsync` (con la misma validación de página que `TecnicoService`), `GetByIdAsync` y `GetRolesAsync`.
     - `AddAsync`: login y correo únicos (409); el rol existe (400); guarda la clave con hash; el usuario nace activo.
     - `UpdateAsync`: existe (404); login y correo únicos excluyendo al propio usuario (409).
     - `CambiarRolAsync`: existe (404); el rol existe (400); si deja de ser Administrador y es el último Administrador activo, responde 409.
     - `CambiarClaveAsync` (reseteo): existe (404); guarda el hash nuevo; funciona también con inactivos. El log registra quién y a quién, nunca la clave.
     - `DeleteAsync` (desactivar): existe y está activo (404); si es el último Administrador activo, responde 409; `Estado = false`.
     - `ReactivarAsync`: existe (404); si ya está activo, responde 409; `Estado = true`.
   - **`UsuarioController`** con `[Authorize(Roles = Roles.Administrador)]` en la clase, documentado como los demás, con 401 y 403 declarados en la clase:

     | Método | Ruta | Respuestas |
     |---|---|---|
     | GET | `api/Usuario` | 200 |
     | GET | `api/Usuario/{pageNumber}/{pageSize}` | 200, 400 |
     | GET | `api/Usuario/{id}` (`Name = "GetUsuarioById"`) | 200, 404 |
     | GET | `api/Usuario/roles` | 200 |
     | POST | `api/Usuario` | 201 con `CreatedAtRoute`, 400, 409 |
     | PUT | `api/Usuario/{id}` | 200, 400, 404, 409 |
     | PUT | `api/Usuario/{id}/rol` | 200, 400, 404, 409 |
     | PUT | `api/Usuario/{id}/clave` | 200, 400, 404 |
     | DELETE | `api/Usuario/{id}` | 200, 404, 409 |
     | POST | `api/Usuario/{id}/reactivar` | 200, 404, 409 |

   - Registrar `IUsuarioService` y `UsuarioService` en `Program.cs`. `IUsuarioRepository` ya está registrado.
2. **Roles en los controladores que faltan,** siguiendo el ejemplo de `TecnicoController` (ver [convenciones](convenciones.md#autenticación-y-autorización)):
   - `ProveedorController` y `RepuestoController`: Administrador y Coordinador/Técnico consultan (incluidos stock bajo el mínimo y proveedor recomendado); solo el Administrador escribe, asocia proveedores y registra entradas.
   - `OrdenMantenimientoController`: los tres roles consultan; crean el Administrador y el Solicitante/Consulta. El usuario ya sale del token.
   - Declarar el 401 en la clase y el 403 donde corresponda.
3. **El Solicitante ve solo lo suyo.**
   - En `GET api/OrdenMantenimiento/{pageNumber}/{pageSize}` y `GET api/OrdenMantenimiento/{id}`, el controlador pasa `User.GetIdUsuario()` y si el usuario es Solicitante (`User.IsInRole(Roles.SolicitanteConsulta)`). El servicio filtra por `IdUsuarioSolicitante`. Una orden ajena responde 404, igual que si no existiera.
   - En `GET api/Activo/{id}/historial`, un Solicitante solo ve el historial de vehículos con alguna orden suya; si no tiene ninguna, 404.
4. **Prueba en Scalar** con `admin`, `lvargas` (Coordinador), `cjimenez` (Solicitante) y `drojas` (inactiva). Revisar que cada endpoint responda 401 sin token y 403 con un rol sin permiso, y que no se pueda desactivar ni quitar el rol a `admin` mientras sea el único Administrador activo.
5. Marcar en la [checklist](requerimientos.md#avance-1--semana-7) los ítems de "Usuarios y seguridad" que queden terminados.

**Juntos**

- Corregir lo que salga en el recorrido.
- Repaso cruzado completo, en el que cada uno explica los módulos del otro, incluido el flujo del token: login, `TokenService`, validación en `Program.cs`, `[Authorize]` y `User.GetIdUsuario()`.
- Integrar en `main` y crear la etiqueta `avance-1`.
- Pedir al profesor el material de React para el Avance 2, y confirmar que la forma en que resolvimos el JWT le parece bien.

---

## Avance 2: semanas 8 a 13

Objetivo: el ciclo de vida completo de la orden, el frontend en React, el historial visible, los reportes y el dashboard. La seguridad con JWT se adelantó al Avance 1.

### Semana 8: frontend base y primera parte del ciclo de vida

**Integrante A**

La seguridad se adelantó a la semana 7, así que A adelanta el frontend base de la semana 9:

- Crear el proyecto React con Vite en `frontend/`, con la estructura que indique el profesor.
- Módulo cliente de la API con el token, login, cierre de sesión y redirección al login cuando el token expira.

**Integrante B**

- `MatrizTransicionesOrden` con roles.
- Diagnosticar (prioridad, kilometraje y corrección del tipo), aprobar (fecha programada), rechazar y cancelar antes de la ejecución, cada una con su historial.
- Completar el seeder de órdenes si quedó pendiente, y recrear la base.

### Semana 9: plantilla del frontend y segunda parte del ciclo de vida

**Integrante A**

- Pantalla de cambio de contraseña propia, disponible para los tres roles.
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

- Pantallas de usuarios (incluidos el cambio de rol y el reseteo de contraseña) y de mantenimiento preventivo.
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
- Completar las secciones pendientes del README: frontend y cómo iniciar sesión desde React.

**Integrante B**

- Corregir lo que salga en el recorrido, en sus módulos.
- Revisión final de dependencias (`dotnet list package --vulnerable` y `npm audit`) y de que no queden datos fijos en React.

**Juntos**

- Repaso cruzado completo de todo el sistema y ensayo de la defensa.
- Integrar en `main` y crear la etiqueta `avance-2`.

---

## Pendientes que no son código

- Conseguir las 12 fotos de los vehículos (semana 6, Integrante A).
- Pedir al profesor el material de manejo global de excepciones y React (lo antes posible), y confirmar con él la forma en que resolvimos el JWT. El CRUD, las validaciones y los seeders ya están en el proyecto ERP de clase.
# Convenciones del equipo

Estos son los acuerdos con los que trabajamos los dos. Si algo de aquí deja de servirnos, lo cambiamos en este archivo antes de cambiarlo en el código.

## 1. Stack y versiones

Versiones verificadas en NuGet el 8 de octubre de 2026. Todas son estables, tienen mantenimiento activo y no muestran avisos de deprecación ni de vulnerabilidades.

### Herramientas

- SDK de .NET 10.
- Visual Studio 2026, o VS Code con C# Dev Kit. Visual Studio 2022 no soporta oficialmente proyectos `net10.0`.
- SQL Server Developer, instancia local, con autenticación de Windows.
- `dotnet-ef` 10.0.x, instalado como herramienta global.
- Git para Windows.

### Paquetes NuGet por proyecto

- **`SGMA.Domain`:** ninguno.
- **`SGMA.Shared`:** ninguno.
- **`SGMA.Infrastructure`:**
  - `Microsoft.EntityFrameworkCore` 10.0.12
  - `Microsoft.EntityFrameworkCore.SqlServer` 10.0.12
  - `Microsoft.EntityFrameworkCore.Design` 10.0.12, con `PrivateAssets` en `all`
  - `Microsoft.Extensions.Identity.Core` 10.0.12, para `PasswordHasher`
- **`SGMA.Application`:**
  - `Mapster` 10.0.13
- **`SGMA.WebAPI`:**
  - `Microsoft.AspNetCore.OpenApi` 10.0.12
  - `Microsoft.EntityFrameworkCore.Design` 10.0.12, con `PrivateAssets` en `all`
  - `Mapster.DependencyInjection` 10.0.13
  - `Scalar.AspNetCore` 2.17.14
  - `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12. **Pendiente (Avance 2).**

Es la misma distribución de paquetes del material de clase, con dos diferencias:

- Scalar: el material usa la 2.17.4 y nosotros la 2.17.14, el último parche de la misma versión.
- `SGMA.WebAPI` no referencia `Microsoft.Extensions.Identity.Core`: ASP.NET Core ya lo incluye y el SDK lo marca como sobrante (advertencia `NU1510`).

### Frontend

**Pendiente (Avance 2).** React con JavaScript, sin TypeScript, creado con Vite, porque Create React App está deprecado. Las versiones de Node.js, React y cada dependencia se fijan y se verifican al crear el proyecto.

### Reglas sobre dependencias

- Todos los paquetes `Microsoft.*` van en la misma versión de parche. EF Core exige que todos sus paquetes coincidan.
- Nunca usamos versiones preliminares (`preview`, `rc`, `pre`).
- Antes de cada integración a `main` revisamos las dependencias:

  ```powershell
  dotnet list package --vulnerable --include-transitive
  dotnet list package --deprecated
  dotnet list package --outdated
  ```

  Los dos primeros comandos deben salir vacíos. Si el tercero muestra un parche nuevo de 10.0.x, actualizamos todos los paquetes `Microsoft.*` juntos. En el frontend, el equivalente es `npm audit`.
- Ningún paquete entra al proyecto sin que los dos estemos de acuerdo y sin revisar antes en NuGet o npm que esté mantenido.

---

## 2. Estructura de capas

Seguimos las capas y patrones de los materiales de clase.

### Proyectos y dependencias

```
backend/
├── SGMA.slnx
├── SGMA.Domain/
│   ├── Entities/          Entidades (record class)
│   └── DTO/               DTO de lista, detalle, creación, edición y reportes, y PatternResult/ (Result y BaseResult)
├── SGMA.Infrastructure/
│   ├── Data/              AppDbContext y configuración de tablas
│   ├── Data/Seed/         CatalogSeeder, MasterSeeder, DevelopmentSeeder, DemoData, SeedIds
│   ├── Interfaces/        Interfaces de repositorios
│   ├── Implementation/    Repositorios
│   └── Migrations/        Generadas por EF
├── SGMA.Application/
│   ├── Interfaces/        Interfaces de servicios
│   ├── Implementation/    Servicios y matriz de transiciones
│   └── Mapper/            MapsterConfig
├── SGMA.Shared/
│   ├── Exceptions/        Excepciones de negocio
│   └── Constants/         Constantes compartidas, como los nombres de rol
└── SGMA.WebAPI/
    ├── Controllers/       Controladores
    ├── Handlers/          Manejador global de excepciones
    ├── wwwroot/imagenes/  Imágenes de los vehículos
    ├── Program.cs
    └── appsettings.json
```

Referencias entre proyectos, igual que en el material:

- `SGMA.Infrastructure` referencia a Domain.
- `SGMA.Application` referencia a Domain, Infrastructure y Shared.
- `SGMA.WebAPI` referencia a Application, Domain, Infrastructure y Shared.
- `SGMA.Domain` y `SGMA.Shared` no referencian a nadie.

En la Clean Architecture de libro, Infrastructure depende de Application y no al revés. Seguimos la organización de clase porque el enunciado la exige, y los dos debemos poder explicar esa diferencia si nos la preguntan.

### Recorrido de una petición

1. **Controlador** (`WebAPI/Controllers`). Recibe la petición, llama al servicio y responde con `StatusCode(result.Status, result)`. Al crear responde 201 con `CreatedAtRoute`, que agrega el encabezado `Location`. Documenta cada código posible con `[ProducesResponseType]` y `[EndpointSummary]` para que se vea en Scalar. No tiene lógica de negocio.
2. **Servicio** (`Application/Implementation`). Aplica las reglas de negocio y convierte entidades en DTO con Mapster. Devuelve siempre un `Result<T>`: `Success`, `Paged` o `Failure` con 400, 404 o 409 para los errores esperados del CRUD. Recibe el usuario responsable como parámetro: en el Avance 1 viene del cuerpo de la petición y desde el Avance 2, del token.
3. **Repositorio** (`Infrastructure/Implementation`). Consulta y guarda con EF Core. Devuelve entidades, salvo en agregaciones (`GroupBy`, reportes, dashboard), que devuelven el DTO de resultado directamente. Cada método de escritura termina con un solo `SaveChangesAsync`, y los borrados físicos usan `ExecuteDeleteAsync`, igual que en el material.
4. **`AppDbContext`** (`Infrastructure/Data`). Configura tablas, relaciones, índices y restricciones en `OnModelCreating`, y aplica los seeders.

Cada entidad principal tiene su par de interfaz e implementación: `IActivoRepository` con `ActivoRepository`, `IActivoService` con `ActivoService` y su `ActivoController`. Repositorios y servicios se registran en `Program.cs` con `AddScoped`.

**Operaciones que tocan varias entidades.** Por ejemplo, iniciar una orden cambia la orden, crea asignaciones, cambia el vehículo y escribe dos historiales. Los pasos intermedios solo modifican entidades rastreadas por EF, sin guardar, y el último paso guarda una sola vez. Como todos los repositorios de una petición comparten el mismo `AppDbContext` (`AddScoped`), ese único `SaveChangesAsync` aplica todo en una sola transacción: se guarda todo o nada.

### Respuestas y errores

- **Forma única de respuesta:** `Result<T>` y `BaseResult`, en `SGMA.Domain/DTO/PatternResult`, copiados del material. El código HTTP se repite en `status` dentro del body.
- **Errores esperados del CRUD** (no existe, nombre repetido, tiene relaciones): el servicio devuelve `Result<T>.Failure`, como en el material. El orden de las verificaciones es 404 y después 409.
- **Reglas del flujo de estados:** el servicio lanza una excepción de negocio propia, porque el enunciado lo exige (sección 9). El manejador global la convierte en la misma forma `Result<T>`, así el cliente nunca ve dos formatos.
- **Validación de formato:** la hace `[ApiController]` con las DataAnnotations del DTO y responde 400 con `ValidationProblemDetails`. Es la única respuesta con otra forma, igual que en el material.
- **Errores inesperados:** el manejador global responde 500 con un mensaje genérico en español, sin detalles internos.

### Lugares fijos

- **Excepciones de negocio:** `SGMA.Shared/Exceptions`.
  - `RecursoNoEncontradoException` → 404.
  - `TransicionNoPermitidaException` → 409.
  - `ReglaNegocioException` → 409.
- **Respuesta estándar:** `SGMA.Domain/DTO/PatternResult/Result.cs` y `BaseResult.cs`.
- **Manejador global:** `SGMA.WebAPI/Handlers`. Implementa `IExceptionHandler` y responde con `Result<T>.Failure` y mensajes en español.
- **Matriz de transiciones:** `SGMA.Application/Implementation/MatrizTransicionesOrden.cs`.
- **Ids de catálogos sembrados:** constantes en `SGMA.Infrastructure/Data/Seed/SeedIds.cs`. Nunca escribimos números sueltos en el código.

---

## 3. Reglas obligatorias

Estas reglas no se discuten en cada cambio: si un cambio las rompe, no entra a `main`.

### Datos y cálculos

- React nunca muestra datos fijos ni simulados. Todo viene de nuestra API, que lee con EF Core desde SQL Server. Los datos de prueba solo entran por seeders.
- Totales, conteos, promedios y reportes se calculan en el backend con LINQ. El frontend solo los muestra.
- La API nunca devuelve entidades, siempre DTO.
- Las lecturas usan `AsNoTracking()` y todo acceso a datos es asíncrono, con el sufijo `Async`.
- Los listados devuelven 200 con una lista vacía si no hay datos. El 404 solo se usa cuando no existe un recurso pedido por id.
- Los listados extensos se paginan con `Result<T>.Paged`, que devuelve también el total de registros y de páginas. La paginación ordena siempre por la llave y acepta como máximo 50 registros por página (`MaxPageSize`); fuera de ese rango responde 400, como en el material.

### Reglas de negocio

- Toda transición de estado no permitida se rechaza con una excepción de negocio propia, nunca con un error genérico.
- El historial de estados se genera solo, en cada cambio real y dentro de la misma transacción. Es de solo lectura: no hay endpoints para escribirlo.
- El stock nunca es negativo. Si no alcanza, la orden pasa sola a En Espera de Repuesto.
- El vehículo pasa a En Mantenimiento y vuelve a Operativo automáticamente según sus órdenes. Fuera de Servicio y Dado de Baja son acciones exclusivas del Administrador.
- El estado del vehículo cambia en un solo lugar: un método de `ActivoService` que recibe el estado nuevo, el usuario y, si la provocó una orden, la orden. Lo usan tanto las transiciones manuales como las automáticas del servicio de órdenes, y es lo único que escribe `HistorialEstadoActivo`. Si el estado no cambia, no registra nada.
- Un vehículo En Mantenimiento no admite una orden nueva, salvo de emergencia.
- Vehículos, usuarios, técnicos, repuestos, proveedores y órdenes nunca se borran físicamente.
- Un técnico solo se asigna si tiene la especialidad requerida, y las horas solo se registran con la orden En Ejecución.

### Validación

- Doble validación:
  - **Formato:** DataAnnotations en los DTO de entrada, que ASP.NET responde con 400.
  - **Negocio:** en el servicio, con excepciones propias.
  - La base tiene además restricciones `UNIQUE` y `CHECK` como última defensa.
- En el Avance 2 se suma la validación en el cliente, que no reemplaza la del servidor.

### Seguridad

- Las contraseñas se guardan como hash con `PasswordHasher`, nunca en texto plano.
- Un usuario inactivo no inicia sesión.
- El seeder siempre crea al menos un Administrador.
- Ningún secreto va al repositorio. La cadena de conexión, la contraseña del administrador inicial y la clave del JWT viven en User Secrets. La contraseña común de los usuarios de demostración no cuenta como secreto: es pública y solo existe en Development (ver [modelo de datos](modelo-datos.md)).

---

## 4. Convenciones de nombres

### Código C#

- **Dominio en español, sufijos técnicos en inglés:**
  - entidades: `Activo`, `OrdenMantenimiento`, `Repuesto`;
  - interfaces: `IActivoRepository`, `IActivoService`;
  - implementaciones: `ActivoRepository`, `ActivoService`;
  - controladores: `ActivoController`.
- **DTO:** `ActivoListDto` (listas), `ActivoDetailDto` (detalle), `ActivoCreateDto` y `ActivoUpdateDto` (entrada), y nombres descriptivos para reportes, como `CostoOrdenDto` o `CargaTecnicaDto`. Si la entidad es pequeña, basta un solo DTO de salida (`EspecialidadDto`), como `WarehouseDto` en el material.
  - Los DTO de entrada no llevan la llave: al crear la asigna la base y al editar viene de la ruta.
  - Sus DataAnnotations usan los mismos largos que las columnas, para que el error sea un 400 y no una excepción de SQL Server.
- **Métodos:**
  - las operaciones genéricas del CRUD usan los nombres del material: `GetAllAsync`, `GetByIdAsync`, `GetPaginationAsync`, `CountAsync`, `AddAsync`, `UpdateAsync` y `DeleteAsync`, más las verificaciones `ExistsAsync` y `ExistsByNameAsync`;
  - las acciones del negocio van en español: `DiagnosticarAsync`, `AprobarAsync`, `RegistrarEntradaAsync`, `EvaluarPreventivoAsync`.
- Sin tildes ni ñ en los identificadores: `Categoria`, `Anio`, `Tecnico`, `ClaveHash`.
- **Mayúsculas y prefijos:**
  - `PascalCase` para tipos, métodos y propiedades;
  - `camelCase` para variables locales y parámetros;
  - `_camelCase` para campos privados, como el `_repository` del material.
- Una clase por archivo, con el mismo nombre del archivo.
- **Comentarios:** pocos, breves y solo donde aportan algo que el código no dice. No comentamos cada línea ni escribimos frases como "Este método se encarga de…".

### Base de datos

- Tablas en singular y con el mismo nombre de la entidad.
- Llaves con `Id` más el nombre de la entidad: `IdActivo`, `IdOrden`. Las llaves foráneas llevan el mismo nombre que la llave a la que apuntan.
- Restricciones con nombre: `PK_Tabla`, `FK_Tabla_Referencia`, `UQ_Tabla_Campo` y `CK_Tabla_Regla`.
- Migraciones con nombres descriptivos en `PascalCase`, por ejemplo `CreaModeloInicial` o `AgregaRepuestoPendiente`.

### API

- Rutas `api/[controller]` con el controlador en singular: `api/Activo`, `api/OrdenMantenimiento`.
- Acciones del negocio como subrutas en minúscula y con guiones: `api/OrdenMantenimiento/{id}/diagnosticar`, `api/Repuesto/bajo-minimo`, `api/Repuesto/{id}/proveedor-recomendado`.
- Paginación con la misma forma de ruta del material: `api/Activo/{pageNumber}/{pageSize}`.
- JSON en `camelCase`, el formato por defecto de ASP.NET Core.

### React

**Pendiente (Avance 2).** Lo definimos con la estructura que indique el profesor: componentes en `PascalCase` con extensión `.jsx`, hooks con prefijo `use` y un único módulo para las llamadas a la API.

---

## 5. Flujo de ramas

### Ramas

- **`develop`:** donde trabajamos los dos todos los días, con commits directos.
- **`main`:** solo versiones estables. Recibe `develop` por pull request.

No usamos ramas por funcionalidad ni revisiones obligatorias: quien abre el pull request lo fusiona.

### El día a día

```powershell
git switch develop
git pull
# trabajar y hacer commits
git pull
git push
```

- Siempre `git pull` antes de empezar y antes de subir. Así se combinan los cambios del otro y los conflictos aparecen pequeños.
- Los dos tenemos configurado `git config --global pull.rebase false`, para que `git pull` combine con merge.
- Si hay un conflicto, lo resuelve quien lo encontró. Si toca código del otro, lo resolvemos juntos.

### Integrar `develop` en `main`

**Cuándo:** al terminar una funcionalidad grande, como mínimo una vez por semana y siempre antes de una entrega. GitHub solo cuenta en Insights → Contributors los commits que están en `main`; si `develop` pasa semanas sin integrarse, el gráfico muestra semanas vacías.

**Antes de integrar, comprobamos:**

1. La solución compila sin errores.
2. Las migraciones se aplican sobre una base vacía (`database drop` y `database update`) y los seeders cargan sin fallar.
3. Los endpoints tocados funcionan en Scalar, tanto en el camino correcto como en los errores esperados.
4. `dotnet list package --vulnerable --include-transitive` y `--deprecated` salen vacíos.

**Cómo:**

1. En GitHub, **Pull requests → New pull request**, con *base* `main` y *compare* `develop`.
2. **Create pull request** y luego **Merge pull request** con merge commit.
3. La rama `develop` nunca se borra.

**Configuración del repositorio en GitHub:**

- Solo está permitido **Allow merge commits**; squash y rebase están desactivados para que cada commit llegue a `main` con su autor y su fecha.
- **Automatically delete head branches** está desactivado para que GitHub no borre `develop`.

**Entregas:** después de integrar la versión entregada, marcamos el commit de `main` con una etiqueta:

```powershell
git switch main
git pull
git tag avance-1
git push origin avance-1
```

Para la entrega final, la etiqueta es `avance-2`.

---

## 6. Commits

### Cómo y cuándo

- Cada uno hace sus propios commits, desde su propia cuenta y con el correo de su cuenta de GitHub. Nunca subimos trabajo del otro bajo nuestro nombre, y los commits no llevan líneas de coautoría ni firmas.
- Los dos hacemos commits todas las semanas, idealmente uno o dos por sesión de trabajo. Un commit por avance pequeño que funciona: una entidad con su configuración, un endpoint, una validación, un seeder.
- Nada de acumular días de trabajo en un solo commit antes de una entrega.
- Un commit no mezcla cambios que no tienen que ver entre sí.
- Nunca subimos secretos, carpetas `bin/`, `obj/` o `.vs/`, ni archivos personales del editor.

### Formato del mensaje

- En español, en presente y en tercera persona: **Agrega**, **Corrige**, **Actualiza**, **Elimina**, **Renombra**, **Refactoriza**, **Documenta**.
- Empieza con mayúscula, no termina en punto y cabe en una línea de unos 72 caracteres.
- Dice qué cambió y dónde, de forma que se entienda sin abrir el código.

Ejemplos que sí:

- `Agrega validación de stock en el registro de repuestos de la orden`
- `Agrega entidad Activo con su configuración en AppDbContext`
- `Corrige el cálculo del costo de mano de obra al completar la orden`
- `Actualiza la checklist de cobertura del Avance 1`

Ejemplos que no:

- `cambios`
- `avance`
- `arreglos varios`
- `Agregué cosas del activo y también arreglé lo de proveedores.`

---

## 7. Cuándo un módulo está terminado

Un módulo solo cuenta como terminado, y se marca en la [checklist de cobertura](requerimientos.md#checklist-de-cobertura-por-avance), cuando cumple todo esto:

1. Tiene su entidad, su configuración en `AppDbContext` y su migración aplicada.
2. Tiene datos en el seeder.
3. Tiene repositorio, servicio y controlador con todas las acciones del avance.
4. Tiene todas las validaciones y reglas de negocio de su sección en los [requerimientos](requerimientos.md).
5. Funciona en Scalar de punta a punta, con el camino correcto y con los errores esperados.
6. Los dos lo entendemos lo suficiente para explicarlo y modificarlo en la defensa.

---

## 8. Pendiente de confirmar con el profesor

El material de clase (proyecto ERP) cubre las consultas, el CRUD completo con `Result<T>`, las DataAnnotations, el mapeo con Mapster y los seeders. Para estos temas aplicamos lo descrito en este documento hasta tener el material correspondiente:

- Manejo global de excepciones con `IExceptionHandler`.
- Transacciones entre varios repositorios. Mientras tanto, un solo `SaveChangesAsync` al final de cada operación (ver [Recorrido de una petición](#recorrido-de-una-petición)).
- Subida de imágenes.
- JWT: dónde se generan los tokens, qué claims llevan y cuánto duran.
- Estructura del proyecto React y forma de consumir la API.

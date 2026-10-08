# SGMA — Sistema de Gestión de Mantenimiento de Activos

**Contexto: flota vehicular.** Proyecto del curso ISW-621 Programación en Ambiente Web I, carrera de Ingeniería del Software, UTN Sede Central.

> Las partes marcadas como **Pendiente** se completan en el avance indicado. El avance real de cada módulo se registra en la [checklist de cobertura](docs/requerimientos.md#checklist-de-cobertura-por-avance).

## Equipo

- **Integrante A:** Sebastian Araya Mejias.
- **Integrante B:** Rachel Oviedo Carvajal.

## Qué hace el sistema

SGMA gestiona el mantenimiento de los vehículos de una flota. Mantiene el inventario de unidades, el personal técnico con sus especialidades, los repuestos con sus proveedores y el ciclo de vida completo de cada orden de mantenimiento, desde la solicitud hasta el cierre.

Además:

- registra automáticamente el historial de estados de cada vehículo y de cada orden;
- controla el stock de repuestos y nunca permite que quede negativo;
- calcula el costo de cada orden con los repuestos usados y las horas de los técnicos;
- anticipa el mantenimiento preventivo según el kilometraje real de cada unidad, no solo por fechas fijas.

## Contexto: flota vehicular

- **Activos:** camiones, buses, pickups y vehículos similares. Cada unidad tiene un código interno, sus datos de identificación y su kilometraje.
- **Variable de uso:** el kilometraje. Cada categoría de vehículo define cada cuántos kilómetros corresponde el mantenimiento preventivo y con cuánta anticipación avisar (ver [decisiones técnicas](docs/decisiones-tecnicas.md)).
- **Roles**, con los nombres del enunciado:
  - **Administrador:** gestiona la flota, los catálogos y los usuarios; aprueba o rechaza órdenes, configura el mantenimiento preventivo y consulta los reportes.
  - **Coordinador/Técnico:** el personal del taller; diagnostica, ejecuta y cierra órdenes, asigna técnicos y registra repuestos y horas.
  - **Solicitante/Consulta:** quien reporta la necesidad de mantenimiento, por ejemplo un supervisor de ruta; crea solicitudes y consulta sus órdenes y los vehículos.

## Proceso principal

Una orden nace **Solicitada**. El taller la **diagnostica** y el Administrador la **aprueba** o la **rechaza**. Al pasar a **En Ejecución**, el vehículo cambia solo a *En Mantenimiento*; cuando la orden se **completa**, vuelve a *Operativo*. Si durante la ejecución falta stock de un repuesto, la orden pasa sola a **En Espera de Repuesto** hasta que haya disponibilidad. Antes del cierre, la orden puede **cancelarse**.

Las transiciones permitidas, quién puede ejecutar cada una y sus efectos automáticos están en la [matriz de estados](docs/matriz-estados.md).

## Entregas

**Avance 1 (semana 7) — en desarrollo.** API sin frontend con el CRUD de activos, categorías, técnicos, especialidades, repuestos y proveedores; relaciones 1:N y N:N; migraciones y seeders; validaciones; manejo global de excepciones; y creación y consulta de órdenes de mantenimiento.

**Avance 2 (semana 13, entrega final) — pendiente.** Ciclo de vida completo de la orden, frontend en React, JWT y roles, historial visible, reportes y dashboard.

## Stack

- **Backend:** ASP.NET Core 10 Web API.
- **ORM:** Entity Framework Core 10 con entidades en código, migraciones y seeders (el enfoque que el curso llama *Model First*).
- **Base de datos:** SQL Server Developer, instancia local.
- **Mapeo entidad–DTO:** Mapster.
- **Documentación de la API:** OpenAPI y Scalar.
- **Contraseñas:** `PasswordHasher` de ASP.NET Core Identity (hash PBKDF2 con *salt*).
- **Autenticación:** JWT con roles. **Pendiente (Avance 2).**
- **Frontend:** React con JavaScript, sin TypeScript. **Pendiente (Avance 2).**
- **Control de versiones:** Git y GitHub.

Las versiones exactas y las reglas sobre dependencias están en [convenciones](docs/convenciones.md).

> El enunciado indica GitLab como repositorio; trabajamos en GitHub con la autorización del profesor.

## Arquitectura y estructura del repositorio

Seguimos la organización de capas y los patrones vistos en clase (Clean Architecture): repositorios y servicios con interfaz e implementación, DTO de lista y de detalle, Mapster para el mapeo y seeders en tres niveles.

```
.
├── README.md
├── .gitignore
├── docs/                         Documentación del proyecto
├── backend/                      En construcción (Avance 1)
│   ├── SGMA.slnx
│   ├── SGMA.Domain/              Entities/ y DTO/
│   ├── SGMA.Infrastructure/      Data/ (AppDbContext y Seed/), Interfaces/, Implementation/ y Migrations/
│   ├── SGMA.Application/         Interfaces/, Implementation/ y Mapper/
│   ├── SGMA.Shared/              Excepciones de negocio y constantes compartidas
│   └── SGMA.WebAPI/              Controllers/, Program.cs, appsettings.json y wwwroot/imagenes/
└── frontend/                     Pendiente (Avance 2)
```

Qué hace cada proyecto y a cuáles referencia:

- **`SGMA.Domain`:** entidades del dominio y DTO. No referencia a ningún otro proyecto.
- **`SGMA.Infrastructure`:** acceso a datos con EF Core: `AppDbContext`, configuración de tablas, seeders, repositorios y migraciones. Referencia a Domain.
- **`SGMA.Application`:** servicios con las reglas de negocio y configuración de Mapster. Referencia a Domain, Infrastructure y Shared.
- **`SGMA.Shared`:** excepciones de negocio y constantes que usan Application y WebAPI. No referencia a ningún otro proyecto.
- **`SGMA.WebAPI`:** controladores, manejo global de excepciones, configuración e imágenes de los activos. Referencia a Application, Domain, Infrastructure y Shared.

## Cómo levantar el proyecto

> Los comandos del backend aplican desde que exista la solución en `backend/` (Avance 1).

### Requisitos

- Windows 10 u 11 y Git.
- SDK de .NET 10.
- Visual Studio 2026 con la carga de trabajo de ASP.NET y desarrollo web, o VS Code con C# Dev Kit. Visual Studio 2022 no soporta oficialmente proyectos `net10.0`.
- SQL Server Developer, instancia por defecto en `localhost`, con autenticación de Windows. SQL Server Management Studio es opcional.
- Herramienta `dotnet-ef` versión 10:

  ```powershell
  dotnet tool install --global dotnet-ef --version 10.0.*
  # Si ya estaba instalada una versión anterior:
  dotnet tool update --global dotnet-ef --version 10.0.*
  ```

- Node.js. **Pendiente (Avance 2):** la versión se fija al crear el frontend.

### 1. Clonar el repositorio

```powershell
git clone <URL-del-repositorio>
cd <carpeta-del-repositorio>
```

### 2. Configurar los secretos locales

Cada uno guarda su cadena de conexión y la contraseña del administrador inicial en *User Secrets*, fuera del repositorio. Así `appsettings.json` es igual para los dos y ningún secreto llega a GitHub.

```powershell
cd backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=SGMA;Trusted_Connection=True;TrustServerCertificate=True" --project SGMA.WebAPI
dotnet user-secrets set "Seed:AdminPassword" "<contraseña-del-admin>" --project SGMA.WebAPI
```

- `TrustServerCertificate=True` es necesario porque la instancia local usa un certificado autofirmado.
- Los secretos quedan en `%APPDATA%\Microsoft\UserSecrets\`, en la carpeta del `UserSecretsId` de `SGMA.WebAPI`. Para revisarlos: `dotnet user-secrets list --project SGMA.WebAPI`.
- **Pendiente (Avance 2):** clave de firma del JWT.

### 3. Crear la base de datos

Desde `backend/`:

```powershell
dotnet ef database update --project SGMA.Infrastructure --startup-project SGMA.WebAPI
```

Este comando crea la base `SGMA`, aplica las migraciones y ejecuta los seeders en este orden:

1. **Catálogos** (`HasData`): estados, roles y demás valores fijos; viajan dentro de las migraciones.
2. **Administrador inicial** (`MasterSeeder`): usuario `admin` con la contraseña del paso 2, guardada como hash.
3. **Datos de demostración** (`DevelopmentSeeder`): vehículos, técnicos, repuestos, proveedores y órdenes de ejemplo. Solo se cargan en ambiente Development, que es el que usan las herramientas de EF en nuestras máquinas.

Los seeders revisan si sus datos ya existen, así que volver a ejecutar el comando no duplica nada. Para empezar desde cero:

```powershell
dotnet ef database drop --force --project SGMA.Infrastructure --startup-project SGMA.WebAPI
dotnet ef database update --project SGMA.Infrastructure --startup-project SGMA.WebAPI
```

### 4. Ejecutar la API

```powershell
dotnet dev-certs https --trust   # solo la primera vez
dotnet run --project SGMA.WebAPI --launch-profile https
```

La documentación interactiva queda en `https://localhost:<puerto>/scalar/v1`. El puerto aparece en la consola y en `SGMA.WebAPI/Properties/launchSettings.json`.

> **Pendiente (Avance 2):** cómo obtener el token con el login y usarlo desde Scalar.

### 5. Ejecutar el frontend

> **Pendiente (Avance 2):** versión de Node.js, instalación de dependencias, URL de la API y comando para levantar la aplicación.

### Usuarios de prueba

El usuario `admin` tiene el rol Administrador y su contraseña es la que cada uno configuró en `Seed:AdminPassword`.

> **Pendiente (Avance 1):** usuarios de demostración de los tres roles, cuando exista el `DevelopmentSeeder`.

### Problemas frecuentes

- **`Falta la configuración Seed:AdminPassword` al ejecutar la API o un comando de EF:** falta el secreto del paso 2.
- **Error de certificado al conectar con SQL Server:** la cadena de conexión debe incluir `TrustServerCertificate=True`.
- **`Unable to create a 'DbContext'` al usar `dotnet ef`:** el comando debe ejecutarse desde `backend/` con `--project` y `--startup-project`, como en el paso 3.
- **Advertencia `NETSDK1233` al compilar:** la solución debe abrirse con Visual Studio 2026.

## Documentación

- [Requerimientos](docs/requerimientos.md): módulos, campos, reglas de negocio, permisos por rol y checklist de cobertura por avance.
- [Modelo de datos](docs/modelo-datos.md): entidades, relaciones, restricciones de unicidad y datos de los seeders.
- [Matriz de estados](docs/matriz-estados.md): estados de la orden y del activo, transiciones, roles y efectos automáticos.
- [Decisiones técnicas](docs/decisiones-tecnicas.md): mantenimiento preventivo, selección de proveedor, codificación de activos y paleta de colores.
- [Convenciones](docs/convenciones.md): versiones, capas, nombres, ramas y mensajes de commit.
- [Plan de trabajo](docs/plan-trabajo.md): tareas por semana e integrante hasta la semana 13.

## Cómo trabajamos

- `main` siempre está estable; cada funcionalidad se desarrolla en su propia rama.
- Cada uno hace sus propios commits desde su cuenta, con mensajes en español y en presente, por ejemplo `Agrega validación de stock en CrearOrden`.
- El detalle está en [convenciones](docs/convenciones.md).

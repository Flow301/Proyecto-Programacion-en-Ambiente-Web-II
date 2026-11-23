# Decisiones técnicas

El enunciado deja varias definiciones en manos de cada equipo y pide que las justifiquemos en la defensa. Este documento explica cada una: qué decidimos, por qué, qué alternativas descartamos y qué casos borde contemplamos.

1. [Mantenimiento preventivo por kilometraje](#1-mantenimiento-preventivo-por-kilometraje)
2. [Selección de proveedor](#2-selección-de-proveedor)
3. [Codificación de vehículos y órdenes](#3-codificación-de-vehículos-y-órdenes)
4. [Estilo visual e interfaz](#4-estilo-visual-e-interfaz)
5. [Reportes escogidos](#5-reportes-escogidos)
6. [Otras decisiones que defendemos](#6-otras-decisiones-que-defendemos)

---

## 1. Mantenimiento preventivo por kilometraje

### Por qué el kilometraje

En una flota, el desgaste de lo que se cambia en un mantenimiento preventivo (aceite, filtros, frenos, llantas) depende sobre todo de la distancia recorrida. Por eso los fabricantes de vehículos expresan sus intervalos de servicio en kilómetros. El propio enunciado asocia el contexto de flota vehicular con "kilometraje y seguridad vial".

Una regla basada solo en fechas falla con vehículos de uso muy distinto. Un bus urbano recorre varios cientos de kilómetros al día y un vehículo administrativo, unas decenas. Con el mismo calendario, el bus llegaría tarde a su mantenimiento y el vehículo administrativo lo recibiría antes de necesitarlo.

### Datos que usa la regla

- **En la categoría:**
  - `IdVariableControl`: la variable de control, del catálogo `VariableControl`. Solo existe Kilometraje (km): es la única medida de uso real de una flota. Va como catálogo para que el formulario la elija de una lista que viene de la API, sin datos fijos.
  - `IntervaloKm`: el umbral, cada cuántos km corresponde el preventivo.
  - `PorcentajeAviso`: desde qué porcentaje del umbral el vehículo queda Próximo (de 1 a 99).
- **En el vehículo:**
  - `KilometrajeActual`: la lectura actual, el valor de la última lectura de uso.
  - `KilometrajeUltimoPreventivo`: la lectura base, el kilometraje con que se completó el último preventivo (o el de registro si nunca tuvo uno).
  - `IntervaloKmPropio`, opcional. Si existe, reemplaza al de la categoría, por ejemplo cuando el manual de una unidad indica un intervalo distinto.
- **Lecturas de uso (`LecturaUso`):** cada lectura guarda el kilometraje, la fecha y el usuario. Las registra el Administrador con la acción "Registrar lectura de uso" (`POST api/Activo/{id}/lecturas`), y el diagnóstico de una orden también deja una, con el código de esa orden. Nunca se editan ni se borran.

### Fórmula

```
umbral      = IntervaloKmPropio del vehículo, o IntervaloKm de su categoría si no tiene
uso         = lectura actual − lectura base        (uso acumulado desde el último preventivo)
porcentaje  = uso ÷ umbral × 100

Sin lecturas  si el vehículo no tiene ninguna lectura de uso registrada
Vencido       si porcentaje ≥ 100
Próximo       si PorcentajeAviso ≤ porcentaje < 100
Al día        si porcentaje < PorcentajeAviso
```

Los bordes se comparan con enteros (`uso × 100 ≥ PorcentajeAviso × umbral`), así que el resultado no depende del redondeo del porcentaje que se muestra, que lleva un decimal.

Ejemplos con los datos sembrados (aviso al 90 % en todas las categorías):

- **`PKP-0002`, pickup con umbral de 10 000 km.** Uso: 128 900 − 119 500 = 9 400 km. Porcentaje: 94 %. Queda **Próximo**.
- **`PKP-0003`, pickup.** Uso: 54 200 − 40 000 = 14 200 km. Porcentaje: 142 %. Queda **Vencido**.
- **`CMP-0001`, camión pesado con umbral de 15 000 km.** Uso: 12 800 km. Porcentaje: 85,3 %. Queda **Al día**.
- **`TRC-0001`, tractocamión con umbral de 20 000 km.** Uso: 18 600 km. Porcentaje: 93 %. Queda **Próximo**.
- **`VAN-0001`, van de carga.** No tiene lecturas registradas. Queda **Sin lecturas** hasta que el Administrador registre la primera.

### Intervalos por categoría

Los valores iniciales son conservadores:

- 10 000 km para pickups, camiones livianos, buses urbanos, microbuses, vans y vehículos administrativos. Es el orden de magnitud habitual de los servicios que indican los fabricantes para vehículos livianos.
- 15 000 km para camiones pesados y buses interurbanos.
- 20 000 km para tractocamiones. Los vehículos pesados trabajan con mayor capacidad de aceite y en recorridos más largos.
- 5 000 km para motocicletas.

Como son parámetros de cada categoría, el Administrador los ajusta sin tocar código. Si una unidad tiene un intervalo distinto en su manual, se usa el intervalo propio del vehículo.

### Criterio de "Próximo": aviso al 90 %

El enunciado del Avance 2 define el aviso como un porcentaje del umbral, y lo deja a cada equipo. Elegimos el 90 % porque deja el 10 % final del intervalo para programar el trabajo: 1 000 km en los intervalos de 10 000. Un bus urbano recorre esa distancia en pocos días, así que el aviso llega con tiempo suficiente sin sacar la unidad de ruta de improviso, y no tan temprano como para que la lista de alertas se llene de vehículos que todavía no lo necesitan.

Al ser un porcentaje, el aviso escala solo con el intervalo: en un tractocamión de 20 000 km avisa a los 2 000 km restantes. Cada categoría puede tener el suyo.

Antes usábamos un margen fijo en km (1 000 km). Lo cambiamos por el porcentaje que pide el enunciado; la migración convierte los márgenes existentes (margen 1 000 de 10 000 = aviso al 90 %), así que los resultados de la evaluación no cambian.

### Cómo se evalúa

- "Bajo demanda" significa que el estado se calcula en cada consulta, con una consulta LINQ que proyecta a un DTO. No hay procesos programados ni columnas guardadas que puedan quedar desactualizadas.
- Se evalúan los vehículos Operativos y En Mantenimiento, del mayor al menor porcentaje; los Sin lecturas van al final.
- El dashboard, el listado de vehículos (columna de estado preventivo) y el detalle del vehículo (bloque "Uso y mantenimiento preventivo") usan la misma consulta.
- La sugerencia se convierte en orden solo cuando el Administrador la confirma. La orden nace Solicitada, con un tipo de naturaleza Preventivo, prioridad Alta si el vehículo está Vencido (Media si está Próximo) y el motivo precargado, por ejemplo: "Preventivo por uso (Próximo): 142 650 km, 96,5 % del umbral de 10 000 km".
- Al completarse una orden preventiva, la lectura actual pasa a ser la nueva lectura base y el vehículo vuelve a Al día (0 %).

### Alternativas descartadas

- **Solo por fecha:** el enunciado exige al menos una variable de uso real, y por las razones de arriba no representa el desgaste.
- **Kilometraje combinado con tiempo** ("cada 10 000 km o 6 meses, lo que ocurra primero"): es realista, pero agrega una segunda regla que no se pide. Queda como posible mejora.
- **Margen fijo en km:** fue nuestra primera versión. Es fácil de leer, pero no escala con el intervalo y el enunciado del Avance 2 pide un porcentaje.
- **Editar el kilometraje desde el formulario del vehículo:** se descartó porque no deja rastro de quién lo cambió ni cuándo. El kilometraje solo cambia con una lectura de uso.

### Casos borde

- **Vehículo nuevo:** al registrarlo, su kilometraje es la lectura inicial y, salvo que se indique otro, también la lectura base. Queda Sin lecturas hasta que se registre la primera lectura de uso.
- **Porcentaje exactamente 100:** el vehículo queda Vencido. Si es exactamente el porcentaje de aviso, queda Próximo.
- **Lectura menor que la anterior:** se rechaza con 400, porque el kilometraje es acumulativo. Una lectura igual a la anterior sí se acepta.
- **Fecha de la lectura futura o anterior a la última lectura:** se rechaza con 400.
- **Dos lecturas al mismo tiempo:** `KilometrajeActual` es un token de concurrencia, así que la segunda se rechaza con 409 y nunca queda guardada la menor.
- **Vehículo Dado de Baja o Fuera de Servicio:** se excluye de la evaluación y generar su orden preventiva se rechaza con 409 (enunciado del Avance 2, sección 5.8). Cuando un vehículo Fuera de Servicio vuelve a Operativo, entra de nuevo en la evaluación. Un vehículo dado de baja no admite lecturas.
- **Vehículo con una orden preventiva abierta:** aparece como "Orden en curso", con el enlace a esa orden, y no se sugiere otra, para no duplicar órdenes.
- **Vehículo En Mantenimiento:** aparece como "En taller" y no se sugiere la orden, porque crearla se rechazaría por el bloqueo de vehículos en mantenimiento. Se sugiere cuando vuelva a Operativo.
- **Kilometraje del último preventivo mayor que el actual:** no puede ocurrir. Lo impiden la validación del servicio y la restricción `CHECK` de la tabla.
- **La sugerencia cambió antes de confirmarla:** al generar la orden el vehículo se vuelve a evaluar, así que si mientras tanto se abrió otra orden preventiva, entró a mantenimiento o se registró otra lectura, se rechaza con el motivo.
- **Dos Administradores generan la orden del mismo vehículo al mismo tiempo:** podrían quedar dos órdenes preventivas abiertas, porque las dos evaluaciones ven el vehículo sin orden. Es un límite conocido: con un solo Administrador generando no ocurre, y la orden sobrante se cancela.

### Dónde está en el código

- `PreventivoRepository`: la consulta LINQ que proyecta al DTO; calcula el umbral, el uso, el porcentaje y la última lectura en la base, y busca la orden preventiva abierta.
- `ClasificadorPreventivo`: la fórmula de arriba, escrita una sola vez. La usan la evaluación, la generación de la orden, el dashboard y los vehículos.
- `ActivoService.RegistrarLecturaAsync`: valida y guarda la lectura junto con el kilometraje del vehículo en una sola transacción.
- `PreventivoService.GenerarOrdenAsync`: vuelve a evaluar el vehículo y crea la orden con el mismo `AddAsync` de una solicitud normal.
- `OrdenMantenimientoService.CompletarAsync`: al completar una orden preventiva, la lectura actual pasa a ser la base.

---

## 2. Selección de proveedor

### Criterios

Usamos los dos criterios que el enunciado da como ejemplo: **tiempo de entrega** y **precio ofrecido**. Los dos están en la tabla `RepuestoProveedor`.

### Fórmula

Solo se comparan los proveedores **activos** del repuesto consultado:

```
puntaje = 0,6 × (tiempo mínimo / tiempo del proveedor)
        + 0,4 × (precio mínimo / precio del proveedor)
```

Cada criterio se normaliza dividiendo el mejor valor entre el del proveedor. Así, el mejor en ese criterio obtiene 1 y los demás, una fracción proporcional. El puntaje máximo posible es 1. Gana el mayor puntaje; en empate, el precio más bajo y después el nombre.

### Por qué 60 % tiempo y 40 % precio

Cada día que se espera un repuesto es una unidad detenida: una ruta que no se cubre o una carga que no se entrega. En una flota, ese costo suele superar la diferencia de precio entre proveedores de un mismo repuesto. Aun así, el precio pesa lo suficiente para que un proveedor mucho más barato pueda ganar si la diferencia de tiempo es pequeña.

Los pesos son constantes en `SGMA.Application`. Si el profesor pide otro criterio, se cambian en un solo lugar.

### Ejemplo con los datos sembrados: filtro de aceite (`FIL-ACE-001`)

Valores mínimos: 1 día y ₡8 600.

- **Lubricantes y Filtros Central (₡9 200, 1 día):** 0,6 × 1/1 + 0,4 × 8 600/9 200 = 0,600 + 0,374 = **0,974**.
- **Distribuidora Automotriz Pacífico (₡9 900, 2 días):** 0,6 × 1/2 + 0,4 × 8 600/9 900 = 0,300 + 0,347 = **0,647**.
- **Repuestos Diésel del Valle (₡8 600, 4 días):** 0,6 × 1/4 + 0,4 × 8 600/8 600 = 0,150 + 0,400 = **0,550**.

Se recomienda Lubricantes y Filtros Central: cuesta ₡600 más que la opción más barata, pero entrega en 1 día en lugar de 4.

Con las zapatas de freno (`ZAP-FRE-001`) pasa lo mismo:

- Frenos y Embragues Centroamericanos (₡66 000, 3 días) obtiene 0,973.
- Importadora de Partes Pesadas (₡61 500, 8 días) obtiene 0,625.

### Alternativas descartadas

- **Solo precio o solo tiempo:** el enunciado exige al menos dos criterios.
- **Normalización mínimo–máximo** (`(máximo − valor) / (máximo − mínimo)`): divide entre cero cuando todos los proveedores tienen el mismo valor, y le da 0 al peor aunque esté muy cerca del mejor.
- **Pesos configurables en cada consulta:** agrega complejidad sin que el enunciado lo pida. Queda como posible mejora.

### Casos borde

- **Un solo proveedor activo:** es el recomendado, con puntaje 1.
- **Ningún proveedor activo:** la respuesta lo indica y no recomienda nada.
- **Proveedor inactivo:** no participa, aunque sea el más barato. Las pastillas de freno sembradas lo demuestran.
- **Precio 0 o tiempo 0:** no pueden existir. Lo impiden la validación (precio > 0, tiempo ≥ 1 día) y las restricciones `CHECK`, así que la fórmula nunca divide entre cero.

---

## 3. Codificación de vehículos y órdenes

### Vehículos: `PREFIJO-0001`

- El **prefijo** es de tres letras y único por categoría: `PKP` pickup, `CML` camión liviano, `CMP` camión pesado, `TRC` tractocamión, `BUS` bus urbano, `BUI` bus interurbano, `MCB` microbús, `VAN` van de carga, `ADM` vehículo administrativo, `MOT` motocicleta.
- El **consecutivo** es de cuatro dígitos, por prefijo: hasta 9 999 vehículos por categoría.
- El **backend genera el código** al registrar el vehículo: busca el mayor consecutivo existente con ese prefijo y le suma uno. El índice único sobre `Codigo` garantiza que no se repita.
  - Si dos registros simultáneos calculan el mismo número, el segundo falla contra el índice. El servicio lo reintenta una vez con el siguiente número.

**Por qué este patrón.** Quien lee `BUS-0002` sabe de inmediato qué tipo de unidad es, y el código es corto para etiquetas y reportes. Además, la unicidad no depende de que el usuario escriba bien.

**El código es inmutable**, aunque se cambie la categoría del vehículo. Es un identificador, no una clasificación: aparece en el historial, en las órdenes y en los reportes, y cambiarlo rompería esa trazabilidad. Por la misma razón:

- el prefijo de una categoría no se puede cambiar si ya tiene vehículos;
- los códigos de vehículos dados de baja no se reutilizan, porque el cálculo del consecutivo incluye todos los vehículos.

**Alternativas descartadas:**

- **La placa como código:** una placa puede cambiar por reemplacamiento y la asigna una entidad externa. La guardamos aparte, también única.
- **El id numérico:** no dice nada a quien lo lee.
- **Prefijo más año de adquisición:** alarga el código sin aportar mucho.

### Órdenes: `OM-AAAA-00001`

- `OM` de orden de mantenimiento, el año de la solicitud y un consecutivo de cinco dígitos que reinicia cada año.
- Se genera igual que el de los vehículos y también es inmutable.
- El año permite ubicar una orden en el tiempo sin abrirla.

---

## 4. Estilo visual e interfaz

### Idea

Tomamos los colores de la señalización vial, que cualquier persona de una flota reconoce sin explicación:

- **Amarillo**, el de las señales preventivas, para lo que requiere atención pronto.
- **Rojo**, el de las señales de alto y de prohibición, para lo que está detenido o vencido.
- **Verde** para lo que terminó bien.
- **Azul**, el de las señales informativas, como color principal de la interfaz.
- **Gris asfalto** para el texto.

La identidad no está en el adorno sino en un gesto que llamamos **ruta con mojones**: el avance se dibuja como los postes de kilómetro de las rutas nacionales. Ese gesto aparece solo en dos piezas, el progreso de la orden y la barra del mantenimiento preventivo. Todo lo demás (menú, tablas, formularios) es una interfaz de trabajo estándar, porque el éxito del sistema es que cada persona termine su tarea rápido y sin errores, no que la pantalla llame la atención.

### Colores base

| Color | Valor | Uso |
|---|---|---|
| Azul vial | `#1F4E79` | Barra superior, botón principal, enlaces, menú activo y anillo de foco. Solo donde se puede hacer algo. |
| Amarillo preventivo | `#F2B705` | Solo "atención pronto": el estado preventivo Próximo y la marca del aviso. Siempre con texto oscuro. |
| Gris asfalto | `#2E3338` | Texto principal y postes ya recorridos de la ruta. |
| Gris claro | `#F4F5F7` | Fondo de la aplicación. |
| Gris texto | `#4B5563` | Texto secundario: fechas, ayudas, subtítulos. |
| Gris de borde | `#6B7280` | Borde de los campos (4,83:1, WCAG pide 3:1 para controles). Nunca como texto sobre el fondo: da 4,43:1. |

En pantalla, los neutros ocupan cerca del 85 %, el azul vial cerca del 10 %, los colores de estado alrededor del 3 % y el amarillo menos del 2 %. Si todo tuviera color, nada destacaría.

### Colores de estado

El mismo significado lleva el mismo color en órdenes, vehículos y preventivo, y el estado **nunca se comunica solo con color**: siempre va con su ícono y su texto. Todos usan un solo componente, `BadgeEstado`.

| Significado | Color pleno | Estados | Ícono (lucide) |
|---|---|---|---|
| Pendiente | gris azulado `#64748B` | Solicitada | `FileText` (documento) |
| En revisión | azul `#2563EB` | Diagnosticada; Orden en curso y En taller en el preventivo | `Search` (lupa) |
| Autorizado | índigo `#4338CA` | Aprobada | `BadgeCheck` (visto bueno) |
| Trabajo en curso | morado ciruela `#86198F` | En Ejecución, En Mantenimiento | `Wrench` (llave) |
| Requiere atención | ámbar oscuro `#B45309` | En Espera de Repuesto, repuesto pendiente | `Clock` (reloj) |
| Terminado bien | verde `#2E7D32` | Completada, Operativo, Al día, Activo, Disponible | `Flag` (bandera), `CircleCheckBig` |
| Detenido | rojo `#C62828` | Rechazada, Fuera de Servicio, Vencido, Stock bajo | `CircleX` (equis), `OctagonX`, `OctagonAlert` |
| Cerrado | gris `#6B7280` | Cancelada, Dado de Baja, Sin lecturas, Inactivo | `Ban` (círculo tachado), `CircleDashed` |
| Atención pronto | amarillo preventivo | Próximo | `TriangleAlert` |

**Por qué En Ejecución y En Mantenimiento son morado ciruela y no amarillo.** En la primera versión de la paleta los pusimos en amarillo. Lo cambiamos por dos razones:

- La guía de colores del enunciado del Avance 2 (sección 4.3) pone "En proceso" en morado o azul intenso y "Requiere atención" en ámbar. Con el amarillo, el mismo color decía "hay trabajo en curso" y "atención pronto" (Próximo).
- Probamos el morado común (`#7E22CE`) y, simulando protanopia, se confunde casi por completo con el índigo de Aprobada (diferencia de color ΔE 2,0), que es el paso anterior en la ruta. El ciruela `#86198F` se distingue de todos los demás estados (ΔE 24 o más en protanopia y deuteranopia).

Así el amarillo queda con un solo significado, el de la señal preventiva.

**Cómo se pinta el badge: tinte y no color pleno.** El badge usa de fondo un tinte muy claro del color del estado, y el texto y el ícono en la variante oscura de ese color (por ejemplo, En Ejecución: fondo `#F7E9F6`, texto `#701A75`). El color pleno se reserva para la ruta de la orden, la barra del preventivo y los conteos del dashboard. Lo decidimos así porque en un listado hay un badge por fila: con color pleno serían diez bloques fuertes compitiendo entre sí, y el tinte deja ver el estado sin dominar la pantalla. Además el contraste mejora, y el amarillo de Próximo, que con texto blanco no cumple, queda resuelto.

Los colores se definen una sola vez como tokens en `frontend/src/index.css` (`--proceso`, `--proceso-tinte`, `--proceso-texto`…) y los componentes usan las clases que generan (`bg-proceso-tinte`, `text-proceso-texto`). Nunca colores sueltos.

### Contraste y daltonismo

Contraste calculado con la fórmula de WCAG 2.x. El mínimo AA es 4,5:1 para texto normal y 3:1 para texto grande, íconos con significado, bordes de controles y foco.

| Combinación | Contraste |
|---|---|
| Gris asfalto sobre gris claro / sobre blanco | 11,69 / 12,75 |
| Gris texto sobre gris claro / sobre blanco | 6,93 / 7,56 |
| Blanco sobre azul vial; azul vial sobre blanco | 8,66 |
| Azul vial sobre el tinte del menú activo (`#E3EBF3`) | 7,20 |
| Gris asfalto sobre amarillo preventivo | 7,01 |
| Rojo de error sobre blanco | 5,62 |
| Borde de campo sobre blanco | 4,83 |
| Foco azul vial sobre blanco; foco amarillo sobre la barra azul | 8,66; 4,77 |

| Estado | Texto sobre su tinte | Color pleno sobre blanco |
|---|---|---|
| Solicitada | 9,14 | 4,76 |
| Diagnosticada | 7,56 | 5,17 |
| Aprobada | 8,43 | 7,90 |
| En Ejecución / En Mantenimiento | 8,57 | 8,24 |
| En Espera de Repuesto | 6,75 | 5,02 |
| Completada / Operativo / Al día | 6,91 | 5,13 |
| Rechazada / Fuera de Servicio / Vencido | 6,72 | 5,62 |
| Cancelada / Dado de Baja / Sin lecturas | 9,02 | 4,83 |
| Próximo | 8,19 | 1,82: por eso el relleno amarillo de la barra lleva contorno `#8A6A00` |

Además del cálculo, medimos en el navegador el contraste real de cada texto visible del muestrario de componentes (267 textos): todos cumplen AA.

En la simulación de daltonismo, los pares que siguen pareciéndose son Solicitada y Cancelada (los dos grises, a propósito) y, en deuteranopia, En Espera y Rechazada. En los dos casos el ícono y el texto los distinguen, que es justamente para lo que están.

### Tipografía

Usamos una sola familia: **Overpass**, autoalojada con el paquete `@fontsource-variable/overpass` (como el proyecto base del curso hace con Geist), así que no depende de ningún servidor externo.

- **Por qué Overpass:** viene de *Highway Gothic*, la letra de las señales de tránsito, así que pertenece al mismo mundo que la paleta. Es legible en tamaños pequeños y tiene cifras tabulares.
- **Cifras tabulares** (`font-variant-numeric: tabular-nums`) en montos, kilómetros, horas, códigos y conteos: todos los dígitos miden lo mismo, así las columnas de números quedan alineadas y se comparan de un vistazo.
- **Escala de cinco tamaños:** 32 px para el código del registro y las cifras del dashboard, 24 px para el título de la pantalla, 18 px para el título de cada sección, 16 px para el cuerpo y 14 px para tablas y texto secundario. Ningún texto baja de 14 px. Pesos 400, 600 y 700.
- **Descartadas:** Public Sans (muy legible, pero neutra y sin relación con el contexto) y Atkinson Hyperlegible Next (la más legible para baja visión, pero de forma menos sobria). También descartamos la letra del sistema operativo, que cambia según la computadora.
- **Un cuidado:** en Overpass la "l" minúscula y la "I" mayúscula se parecen. Lo tenemos presente en logins y códigos.

### Espaciado, radios, elevación e íconos

- **Espaciado** en múltiplos de 4 px (4, 8, 12, 16, 24, 32, 48).
- **Densidad:** filas de tabla de 44 px; botones y campos de 40 px de alto, porque en el taller se usan de pie y con prisa.
- **Dos radios:** 4 px en controles, badges y bloques (como una placa de señal) y 8 px en lo que flota sobre el contenido (ventanas y avisos).
- **Elevación:** el fondo es gris claro y los bloques son blancos con un borde de 1 px, sin sombra. La sombra queda solo para lo que flota. No usamos tarjetas con sombra como contenedor de todo.
- **Íconos:** solo de lucide, con el mismo grosor de trazo, a 16 px en textos, badges y botones, y a 20 px en el menú. Los íconos decorativos van con `aria-hidden`.
- **Sin gráficos:** el enunciado no los exige (sección 2.3) y los conteos y las tablas responden la pregunta del dashboard sin agregar una decisión nueva.

### Componentes compartidos

Las mismas piezas se usan en todo el sistema (enunciado del Avance 2, sección 3.3). Están en `frontend/src/components/`:

| Componente | Para qué |
|---|---|
| `layout/AppLayout` y `layout/menu.js` | Estructura base: barra superior fija con el sistema, el usuario, su rol y "Cerrar sesión"; menú lateral colapsable, agrupado por propósito y filtrado por rol. |
| `RutaProtegida` | Sin sesión manda al login; con un rol sin permiso muestra "Acceso no autorizado", también si la ruta se escribe a mano. |
| `shared/EncabezadoPagina` | El único `h1` de cada pantalla, el `<title>` de la pestaña, el "Volver" y las acciones siempre a la derecha. Al cambiar de pantalla pone el foco en el título. |
| `shared/BadgeEstado` | La etiqueta de todo estado: tinte, ícono y texto. |
| `shared/ProgresoOrden` | La ruta de la orden (ver abajo). |
| `shared/BarraPreventivo` | El tramo de kilómetros del preventivo, con su leyenda. |
| `shared/TablaPaginada` | La tabla única: paginación de la API, columna de código fija, texto largo recortado, estados de carga, vacío y error. |
| `shared/BarraFiltros` y `hooks/useFiltrosUrl` | Filtros siempre visibles y guardados en la URL. |
| `shared/DialogoConfirmacion` | La confirmación de lo irreversible, con la observación opcional. |
| `shared/CampoFormulario` | Etiqueta, `*`, unidad, ayuda y error junto al campo, con `aria-invalid` y `aria-describedby`. |
| `shared/EstadoVacio`, `EstadoError`, `IndicadorCarga`, `PaginaError` | Estados de pantalla: vacío, sin resultados, vacío positivo, sin conexión con "Reintentar", carga con esqueletos, 403 y 404. |
| `shared/TarjetaIndicador` | Los conteos del dashboard por estado, con enlace al listado filtrado. |
| `shared/LineaTiempo` y `BloqueSeccion` | El historial de estados y cada sección del detalle. |

**La ruta de la orden.** Los cinco pasos del enunciado (Solicitada, Diagnosticada, Aprobada, En Ejecución, Completada) se dibujan como postes sobre una línea:

- los postes recorridos van en gris asfalto, con su fecha;
- el poste actual es más grande y va en el color de su estado;
- **En Espera de Repuesto** cuelga como un desvío del poste En Ejecución, con lo que falta ("Falta AMO-DEL-001 × 2");
- **Cancelada** y **Rechazada** cortan la ruta con una señal circular donde se detuvo la orden; los pasos que no ocurrieron siguen visibles, apagados y marcados "no ocurrió";
- cuando la orden se completa, toda la ruta pasa a verde.

Así cumplimos lo que pide el enunciado (sección 4.3): marcar el paso actual y mostrar de forma claramente distinguible En Espera de Repuesto, Rechazada y Cancelada. La ruta no decide nada: dibuja el estado y las fechas del historial que manda la API.

Los formatos son únicos en todo el sistema y están en `frontend/src/utils/format.js`: fechas `dd/mm/aaaa` (y `dd/mm/aaaa hh:mm` en el historial), colones con símbolo y separador de miles, kilómetros, horas con un decimal y porcentajes.

### Decisiones de usabilidad

Cada decisión responde a uno o más principios de la sección 4.1 del enunciado del Avance 2.

1. **Un solo badge con texto, ícono y color para todo estado.** *Principios: consistencia; reconocer antes que recordar.* El mismo significado tiene el mismo color e ícono en órdenes, vehículos y preventivo: quien aprende que la llave ciruela es "trabajo en curso" lo reconoce en cualquier pantalla. Como nunca es solo color, también lo lee quien no distingue colores.

2. **La ruta de la orden en el encabezado del detalle.** *Principios: jerarquía visual; retroalimentación.* Lo primero que se ve de una orden es dónde va en su proceso, antes que los datos. Si la orden queda En Espera de Repuesto por falta de stock, el desvío aparece en la ruta con el repuesto que falta: el usuario se entera del efecto automático en pantalla, no solo en un aviso.

3. **Solo se muestran las acciones que el rol puede ejecutar.** *Principios: prevención de errores; simplicidad.* El menú se filtra por rol y los botones de acción de la orden salen del campo `accionesDisponibles` de la API. El Coordinador no ve "Aprobar" ni el Solicitante ve costos: no se puede equivocar con un botón que no le corresponde.

4. **Confirmación que nombra el registro.** *Principio: prevención de errores.* Antes de cancelar o rechazar una orden, dar de baja un vehículo o desactivar un usuario, la confirmación dice exactamente qué y sobre cuál registro ("¿Cancelar la orden OM-2026-00006 del vehículo CMP-0001?") y permite escribir una observación. El foco vuelve al botón que la abrió.

5. **Menú agrupado por propósito.** *Principios: agrupación; reconocer antes que recordar.* Inicio · Operación (Órdenes, Preventivo) · Inventario (Vehículos, Repuestos) · Catálogos · Administración. Ningún grupo tiene más de cuatro opciones, así se encuentra cada pantalla sin memorizar dónde está. La sección activa se resalta.

6. **Una acción principal por pantalla, siempre en el mismo lugar.** *Principios: jerarquía visual; consistencia.* El botón principal (azul vial) va a la derecha del encabezado y las acciones secundarias van con contorno. Las alertas y los conteos van arriba; el detalle, abajo.

7. **Filtros y página en la URL.** *Principios: reconocer antes que recordar; control del usuario.* Al volver de un detalle o recargar, el listado conserva el filtro y la página. El usuario no tiene que recordar ni rearmar lo que estaba viendo.

8. **Mensajes en el lenguaje del taller.** *Principios: lenguaje del usuario; retroalimentación.* Los textos usan Unidad, Placa, Kilometraje, Orden y Repuesto, nunca "ID", "endpoint" ni "Error 409". El error de un campo dice cómo corregirlo y aparece junto al campo; si la API no responde, se ve "No fue posible conectar con el servidor" con "Reintentar".

### Accesibilidad

Tomamos WCAG 2.2 nivel AA como piso:

- Contraste verificado con números (ver arriba) y texto de al menos 14 px.
- Todo se usa con teclado, con el enlace "Saltar al contenido" al inicio y un anillo de foco de 3 px que la barra fija no tapa.
- El foco va al título al cambiar de pantalla, queda atrapado dentro de un diálogo y vuelve al botón que lo abrió.
- Cada pantalla tiene un solo `h1`, regiones (`header`, `nav`, `main`), tablas con encabezados y título, y un `<title>` propio.
- La interfaz funciona con zoom al 200 % (probado a 640 px de ancho) y respeta `prefers-reduced-motion`.

### Alternativas descartadas

- **Señalización literal:** badges con forma de señal (rombo, octágono) en mayúsculas. Se reconoce como vial, pero a 14 px las formas no se distinguen y las mayúsculas cansan en pantallas de uso diario.
- **Panel de administración estándar:** píldoras de color pleno sin ícono, tarjetas de indicadores con sombra y pestañas. Es lo más familiar, pero no tiene nada propio del producto y las píldoras sin ícono incumplen "nunca solo color".
- **Modo oscuro y vista para teléfono:** excluidos por el enunciado (sección 2.2). La interfaz se diseña para escritorio desde 768 px; en pantallas de menos de 1024 px el menú arranca colapsado para dejar espacio a las tablas.

### Dónde está en el código

- `frontend/src/index.css`: la paleta, los tokens de estado, la tipografía, los radios y el foco.
- `frontend/src/components/shared/`: los componentes de la tabla anterior.
- `frontend/src/utils/format.js`: los formatos únicos.
- `frontend/src/pages/Muestrario.jsx`: una página solo de desarrollo (no entra al build de producción) con todos los componentes en uso y datos reales de la API, para revisarlos.

---

## 5. Reportes escogidos

El enunciado del Avance 2 (sección 5.9) pide dos de los cuatro reportes, uno de cada familia. Escogimos **Dashboard general** (familia A, resumen) y **Costos por orden** (familia B, detalle de un registro).

### Por qué esta combinación

- **Dashboard general:** el Administrador entra a él, así que es la pantalla que más se usa. Reutiliza cálculos que ya existen (la evaluación preventiva y el proveedor recomendado), y con él las alertas de preventivo y de stock bajo quedan visibles sin navegar, como pide el enunciado general (sección 12).
- **Costos por orden:** cierra el proceso principal, que es el centro del sistema. Sus montos salen del mismo detalle de repuestos y de las mismas asignaciones de técnicos que ya validamos, así que el reporte y el detalle de la orden nunca se contradicen. Se abre desde el detalle de la orden con el botón "Reporte de costos".
- **Descartados:** Carga técnica es el otro reporte de resumen y no se puede combinar con el Dashboard. Historial de un vehículo es de detalle como Costos; lo descartamos porque su línea de tiempo repite buena parte del historial del vehículo, que ya es obligatorio (sección 5.7).

### Consultas LINQ

- **Dashboard** (`DashboardRepository`): un `GroupBy` por estado sobre `OrdenMantenimiento` con dos agregaciones (`Count` y `Min` de la fecha de solicitud), unido después al catálogo `EstadoOrden` para que aparezcan los estados en cero. Las alertas usan la evaluación preventiva (`Activo`, `CategoriaActivo`, `LecturaUso`, `OrdenMantenimiento`) y los repuestos, `Repuesto` con `RepuestoProveedor` y `Proveedor`.
- **Costos por orden** (`ReporteRepository`): `DetalleRepuesto` con `Repuesto` para las líneas y su `Sum`; `AsignacionTecnico` con `Tecnico` para la mano de obra, con un `GroupBy` por orden y dos `Sum` (horas y costo); y un `GroupBy` por tipo y prioridad sobre las órdenes Completadas con `Count` y `Average` del costo total congelado. Todo se ejecuta en SQL Server; el servicio solo suma los dos subtotales y calcula los porcentajes.

### Tolerancia de la comparación: ±15 %

La comparación usa el costo total **congelado** de las órdenes Completadas del mismo tipo y prioridad, así que un cambio posterior en tarifas o precios no la altera. Una orden queda:

- **Por debajo** si cuesta más de un 15 % menos que el promedio;
- **Dentro** si está entre −15 % y +15 %;
- **Por encima** si cuesta más de un 15 % más.

Elegimos ±15 % porque un servicio del mismo tipo varía de forma normal por el tamaño del vehículo (un bus usa más aceite que una pickup) y por las horas reales. Un margen menor marcaría como anómalas órdenes normales; uno mayor dejaría pasar sobrecostos reales. Sin órdenes comparables, el indicador dice "Sin órdenes comparables" y no se calcula la diferencia.

Con los datos sembrados, los tres preventivos por kilometraje de prioridad Baja se comparan entre sí: `OM-2026-00009` (₡65 000) queda Por debajo, `OM-2026-00013` (₡99 500) Por encima y `OM-2026-00014` (₡56 100) Por debajo.

---

## 6. Otras decisiones que defendemos

Están explicadas en detalle en los otros documentos; aquí va el resumen para tenerlas juntas.

- **Llaves IDENTITY en lugar de `ValueGeneratedNever()`:** el `Max + 1` del material falla con registros simultáneos. Detalle en el [modelo de datos](modelo-datos.md).
- **Tipo de mantenimiento como catálogo con naturaleza y especialidad:** el enunciado usa "tipo" tanto para la emergencia como para la especialidad requerida. Detalle en los [requerimientos](requerimientos.md), sección 7.5.
- **Tabla aparte para repuestos pendientes:** no mezcla lo consumido con lo que falta, así el costo es siempre la suma del detalle. Detalle en el [modelo de datos](modelo-datos.md).
- **Un endpoint por cada acción de la orden:** cada uno lleva su propio rol y sus propios datos, y la matriz de transiciones vive en código. Detalle en la [matriz de estados](matriz-estados.md).
- **Costos en vivo y congelados al cierre:** el enunciado pide calcular la mano de obra al momento del cierre; congelar la tarifa evita que un cambio posterior altere órdenes cerradas. Detalle en los [requerimientos](requerimientos.md), sección 7.5.
- **Sin borrados físicos donde hay historia:** vehículos, técnicos, repuestos, proveedores, usuarios y órdenes se desactivan, se dan de baja o se cancelan; nunca se eliminan.

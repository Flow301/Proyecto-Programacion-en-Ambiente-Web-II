# Decisiones técnicas

El enunciado deja varias definiciones en manos de cada equipo y pide que las justifiquemos en la defensa. Este documento explica cada una: qué decidimos, por qué, qué alternativas descartamos y qué casos borde contemplamos.

1. [Mantenimiento preventivo por kilometraje](#1-mantenimiento-preventivo-por-kilometraje)
2. [Selección de proveedor](#2-selección-de-proveedor)
3. [Codificación de vehículos y órdenes](#3-codificación-de-vehículos-y-órdenes)
4. [Paleta de colores](#4-paleta-de-colores)
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

## 4. Paleta de colores

### Idea

Tomamos los colores de la señalización vial, que cualquier persona de una flota reconoce sin explicación:

- **Amarillo**, el de las señales preventivas, para lo que requiere atención pronto.
- **Rojo**, el de las señales de alto y de prohibición, para lo que está detenido o vencido.
- **Verde** para lo que está bien.
- **Azul**, el de las señales informativas, como color principal de la interfaz.
- **Gris asfalto** para el texto y los fondos neutros.

### Colores base

- **Azul vial** `#1F4E79`: barra de navegación, botones principales y enlaces.
- **Amarillo preventivo** `#F2B705`: acentos y avisos de "próximo a vencer"; siempre con texto oscuro.
- **Gris asfalto** `#2E3338`: texto principal.
- **Gris claro** `#F4F5F7`: fondo de la aplicación.
- **Verde** `#2E7D32`: estados correctos y mensajes de éxito.
- **Rojo** `#C62828`: estados detenidos o vencidos y mensajes de error.
- **Ámbar oscuro** `#B45309`: estados de espera.
- **Gris** `#6B7280`: estados cerrados sin éxito, bordes e íconos decorativos.
- **Gris texto** `#4B5563`: texto secundario (subtítulos, ayudas de los campos, fechas y datos complementarios).

**Regla de contraste:** todo texto debe cumplir el contraste mínimo AA (4,5:1). Por eso el amarillo lleva texto oscuro y los demás colores de estado llevan texto blanco. Lo verificamos con un comprobador de contraste al aplicar la paleta.

Contraste calculado (fórmula de WCAG 2.1):

| Combinación | Contraste |
|---|---|
| Blanco sobre azul vial, índigo, rojo, Diagnosticada, verde y ámbar oscuro | entre 5,02 y 8,66 |
| Blanco sobre gris (Cancelada, Dado de Baja) y gris azulado (Solicitada) | 4,83 y 4,76 |
| Gris asfalto sobre amarillo preventivo | 7,01 |
| Gris asfalto sobre el fondo gris claro | 11,69 |
| Gris texto sobre el fondo gris claro y sobre blanco | 6,93 y 7,56 |
| Gris `#6B7280` sobre el fondo gris claro | **4,43: no alcanza** |

Por eso el gris `#6B7280` nunca se usa como color de texto sobre el fondo de la aplicación: el texto secundario usa el gris texto. Para bordes e íconos decorativos sí sirve, porque WCAG les pide 3:1.

### Estados de la orden

Cada estado tiene un color y un ícono, y siempre se muestra con su texto.

- **Solicitada:** gris azulado `#64748B`, ícono de documento.
- **Diagnosticada:** azul `#2563EB`, ícono de lupa.
- **Aprobada:** índigo `#4338CA`, ícono de visto bueno.
- **En Ejecución:** amarillo preventivo con texto oscuro, ícono de llave.
- **En Espera de Repuesto:** ámbar oscuro, ícono de reloj.
- **Completada:** verde, ícono de bandera.
- **Rechazada:** rojo, ícono de equis.
- **Cancelada:** gris, ícono de círculo tachado.

### Estados del vehículo

Usamos el mismo lenguaje visual que en las órdenes: el mismo significado lleva el mismo color.

- **Operativo:** verde, como Completada, porque está bien.
- **En Mantenimiento:** amarillo preventivo, como En Ejecución, porque hay trabajo en curso.
- **Fuera de Servicio:** rojo, como una señal de alto, porque está detenido.
- **Dado de Baja:** gris, como Cancelada, porque está cerrado.

### Alertas del dashboard

- **Preventivo:** Vencido en rojo, Próximo en amarillo, Al día en verde, Sin lecturas en gris, y Orden en curso y En taller en el azul de Diagnosticada (`#2563EB`), no en el azul vial: el azul vial es el de la barra y los botones, y una alerta de ese color se confundiría con la interfaz. El `#2563EB` es de la misma familia saturada que los demás colores de estado y da 5,17 con texto blanco.
- **Stock bajo el mínimo:** rojo.

### Reglas de uso

- El estado nunca se comunica solo con color: siempre va con ícono y texto, para que lo lea cualquier persona, también quien no distingue colores.
- Los colores se definen una sola vez como variables CSS en `frontend/src/index.css` y se usan en toda la interfaz: barra de navegación, botones, etiquetas de estado y alertas. Los tokens de shadcn los reutilizan: `primary` es el azul vial, `secondary` el amarillo preventivo, `foreground` el gris asfalto, `muted-foreground` el gris texto, `destructive` el rojo y el fondo el gris claro.
- Los íconos son de `lucide-react`, la misma librería del proyecto base del curso.

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

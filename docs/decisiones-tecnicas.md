# Decisiones técnicas

El enunciado deja varias definiciones en manos de cada equipo y pide que las justifiquemos en la defensa. Este documento explica cada una: qué decidimos, por qué, qué alternativas descartamos y qué casos borde contemplamos.

1. [Mantenimiento preventivo por kilometraje](#1-mantenimiento-preventivo-por-kilometraje)
2. [Selección de proveedor](#2-selección-de-proveedor)
3. [Codificación de vehículos y órdenes](#3-codificación-de-vehículos-y-órdenes)
4. [Paleta de colores](#4-paleta-de-colores)
5. [Otras decisiones que defendemos](#5-otras-decisiones-que-defendemos)

---

## 1. Mantenimiento preventivo por kilometraje

### Por qué el kilometraje

En una flota, el desgaste de lo que se cambia en un mantenimiento preventivo (aceite, filtros, frenos, llantas) depende sobre todo de la distancia recorrida. Por eso los fabricantes de vehículos expresan sus intervalos de servicio en kilómetros. El propio enunciado asocia el contexto de flota vehicular con "kilometraje y seguridad vial".

Una regla basada solo en fechas falla con vehículos de uso muy distinto. Un bus urbano recorre varios cientos de kilómetros al día y un vehículo administrativo, unas decenas. Con el mismo calendario, el bus llegaría tarde a su mantenimiento y el vehículo administrativo lo recibiría antes de necesitarlo.

### Datos que usa la regla

- **En la categoría:**
  - `IntervaloKm`: cada cuántos km corresponde el preventivo.
  - `MargenAvisoKm`: cuántos km antes del intervalo se avisa.
- **En el vehículo:**
  - `KilometrajeActual`.
  - `KilometrajeUltimoPreventivo`.
  - `IntervaloKmPropio`, opcional. Si existe, reemplaza al de la categoría, por ejemplo cuando el manual de una unidad indica un intervalo distinto.

### Fórmula

```
intervalo = IntervaloKmPropio del vehículo, o IntervaloKm de su categoría si no tiene
recorrido = KilometrajeActual − KilometrajeUltimoPreventivo
restante  = intervalo − recorrido

Vencido            si restante ≤ 0
Próximo a vencer   si 0 < restante ≤ MargenAvisoKm
Al día             si restante > MargenAvisoKm
```

Ejemplos con los datos sembrados:

- **`PKP-0002`, pickup con intervalo de 10 000 km y aviso a 1 000 km.** Recorrido: 128 900 − 119 500 = 9 400 km. Restante: 600 km. Queda **Próximo a vencer**.
- **`PKP-0003`, pickup.** Recorrido: 54 200 − 40 000 = 14 200 km. Restante: −4 200 km. Queda **Vencido**.
- **`CMP-0001`, camión pesado con intervalo de 15 000 km y aviso a 1 500 km.** Recorrido: 12 800 km. Restante: 2 200 km. Queda **Al día**.
- **`TRC-0001`, tractocamión con intervalo de 20 000 km y aviso a 2 000 km.** Recorrido: 18 600 km. Restante: 1 400 km. Queda **Próximo a vencer**.

### Intervalos por categoría

Los valores iniciales son conservadores:

- 10 000 km para pickups, camiones livianos, buses urbanos, microbuses, vans y vehículos administrativos. Es el orden de magnitud habitual de los servicios que indican los fabricantes para vehículos livianos.
- 15 000 km para camiones pesados y buses interurbanos.
- 20 000 km para tractocamiones. Los vehículos pesados trabajan con mayor capacidad de aceite y en recorridos más largos.
- 5 000 km para motocicletas.

Como son parámetros de cada categoría, el Administrador los ajusta sin tocar código. Si una unidad tiene un intervalo distinto en su manual, se usa el intervalo propio del vehículo.

### Criterio de "próximo a vencer"

El margen se define en kilómetros, no en porcentaje, porque así razona un jefe de flota: "a esta unidad le faltan 1 000 km para su servicio". Proponemos un margen cercano al 10 % del intervalo: 1 000 km en los intervalos de 10 000. Un bus urbano recorre esa distancia en pocos días, así que el aviso deja tiempo para programar el trabajo sin sacar la unidad de ruta de improviso.

### Cómo se evalúa

- "Bajo demanda" significa que el estado se calcula en cada consulta, con una consulta LINQ que proyecta a un DTO. No hay procesos programados ni columnas guardadas que puedan quedar desactualizadas.
- El dashboard usa la misma consulta.
- La sugerencia se convierte en orden solo cuando el Administrador la confirma. La orden nace Solicitada, con un tipo de naturaleza Preventivo.
- Al completarse una orden preventiva, `KilometrajeUltimoPreventivo` toma el kilometraje registrado en su diagnóstico, y el contador vuelve a empezar.

### Alternativas descartadas

- **Solo por fecha:** el enunciado exige al menos una variable de uso real, y por las razones de arriba no representa el desgaste.
- **Kilometraje combinado con tiempo** ("cada 10 000 km o 6 meses, lo que ocurra primero"): es realista, pero agrega una segunda regla que no se pide. Queda como posible mejora.
- **Margen en porcentaje:** escala solo con el intervalo, pero es menos claro para quien usa el sistema.

### Casos borde

- **Vehículo nuevo sin preventivos previos:** al registrarlo, el kilometraje del último preventivo es igual al actual, salvo que se indique otro.
- **Restante exactamente 0:** el vehículo queda Vencido. Si el restante es exactamente igual al margen, queda Próximo a vencer.
- **Vehículo Dado de Baja:** se excluye de la evaluación.
- **Vehículo Fuera de Servicio:** se evalúa igual, porque su mantenimiento sigue pendiente.
- **Vehículo con una orden preventiva abierta:** aparece como "orden en curso" y no se sugiere otra, para no duplicar órdenes.
- **Vehículo En Mantenimiento:** aparece como "en taller" y no se sugiere la orden, porque crearla se rechazaría por el bloqueo de vehículos en mantenimiento. Se sugiere cuando vuelva a Operativo.
- **Kilometraje del último preventivo mayor que el actual:** no puede ocurrir. Lo impiden la validación del servicio y la restricción `CHECK` de la tabla.

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
- **Gris** `#6B7280`: estados cerrados sin éxito.

**Regla de contraste:** todo texto debe cumplir el contraste mínimo AA (4,5:1). Por eso el amarillo lleva texto oscuro y los demás colores de estado llevan texto blanco. Lo verificamos con un comprobador de contraste al aplicar la paleta.

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

- **Preventivo:** Vencido en rojo, Próximo a vencer en amarillo, Al día en verde, y Orden en curso y En taller en azul.
- **Stock bajo el mínimo:** rojo.

### Reglas de uso

- El estado nunca se comunica solo con color: siempre va con ícono y texto, para que lo lea cualquier persona, también quien no distingue colores.
- Los colores se definen una sola vez como variables CSS y se usan en toda la interfaz: barra de navegación, botones, etiquetas de estado y alertas.
- La librería de íconos se define junto con la estructura del frontend en el Avance 2.

---

## 5. Otras decisiones que defendemos

Están explicadas en detalle en los otros documentos; aquí va el resumen para tenerlas juntas.

- **Llaves IDENTITY en lugar de `ValueGeneratedNever()`:** el `Max + 1` del material falla con registros simultáneos. Detalle en el [modelo de datos](modelo-datos.md).
- **Tipo de mantenimiento como catálogo con naturaleza y especialidad:** el enunciado usa "tipo" tanto para la emergencia como para la especialidad requerida. Detalle en los [requerimientos](requerimientos.md), sección 7.5.
- **Tabla aparte para repuestos pendientes:** no mezcla lo consumido con lo que falta, así el costo es siempre la suma del detalle. Detalle en el [modelo de datos](modelo-datos.md).
- **Un endpoint por cada acción de la orden:** cada uno lleva su propio rol y sus propios datos, y la matriz de transiciones vive en código. Detalle en la [matriz de estados](matriz-estados.md).
- **Costos en vivo y congelados al cierre:** el enunciado pide calcular la mano de obra al momento del cierre; congelar la tarifa evita que un cambio posterior altere órdenes cerradas. Detalle en los [requerimientos](requerimientos.md), sección 7.5.
- **Sin borrados físicos donde hay historia:** vehículos, técnicos, repuestos, proveedores, usuarios y órdenes se desactivan, se dan de baja o se cancelan; nunca se eliminan.

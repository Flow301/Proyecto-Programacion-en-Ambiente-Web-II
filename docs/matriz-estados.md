# Matriz de estados

Este documento define los estados de la orden de mantenimiento y del vehículo, las transiciones permitidas, qué rol puede ejecutar cada una, qué efectos automáticos dispara y cómo se rechaza todo lo que no está permitido. Respeta como mínimo las restricciones de la sección 9 del enunciado; las transiciones que agregamos están marcadas como *Decisión del equipo*.

## Resumen del flujo de la orden

```
Solicitada ─► Diagnosticada ─► Aprobada ─► En Ejecución ─► Completada
                   │                         ▲      │
                   ▼                         │      ▼
               Rechazada              En Espera de Repuesto
```

Además, la orden puede pasar a **Cancelada** desde cualquier estado no final: Solicitada, Diagnosticada, Aprobada, En Ejecución o En Espera de Repuesto.

Los estados finales son **Rechazada**, **Completada** y **Cancelada**: no admiten ninguna transición posterior.

---

## Estados de la orden

1. **Solicitada:** la orden se registró y espera la revisión del taller. Es el estado inicial de toda orden.
2. **Diagnosticada:** el taller revisó el vehículo y definió alcance, prioridad y kilometraje.
3. **Aprobada:** el Administrador autorizó el trabajo y fijó la fecha programada.
4. **Rechazada:** el Administrador no autorizó el trabajo. Es final.
5. **En Ejecución:** el trabajo está en curso, con técnicos asignados. El vehículo está En Mantenimiento.
6. **En Espera de Repuesto:** el trabajo está detenido porque falta stock de al menos un repuesto. El vehículo sigue En Mantenimiento.
7. **Completada:** el trabajo terminó y los costos quedaron congelados. Es final.
8. **Cancelada:** la orden se anuló antes del cierre. Es final.

## Transiciones permitidas de la orden

Cada transición indica el rol que la ejecuta, la acción de la API, las condiciones que deben cumplirse y los efectos automáticos. Todos los efectos de una transición se guardan en una sola transacción: o se aplican todos o ninguno.

### Creación → Solicitada

- **Rol:** Solicitante/Consulta o Administrador.
- **Acción:** `POST api/OrdenMantenimiento`.
- **Condiciones:**
  - el vehículo existe y no está Dado de Baja;
  - el tipo de mantenimiento existe;
  - el solicitante está activo;
  - si el vehículo está En Mantenimiento, el tipo debe ser de naturaleza Emergencia.
- **Efectos:**
  - se genera el código `OM-AAAA-00001`;
  - se registra la fecha de solicitud;
  - se crea el registro inicial en el historial de la orden, con estado anterior vacío.
- **Avance:** 1.

### Solicitada → Diagnosticada

- **Rol:** Coordinador/Técnico.
- **Acción:** `POST api/OrdenMantenimiento/{id}/diagnosticar`.
- **Condiciones:**
  - diagnóstico, prioridad y kilometraje obligatorios;
  - el kilometraje no puede ser menor que el actual del vehículo;
  - si se corrige el tipo y el vehículo está En Mantenimiento, el nuevo tipo debe ser de naturaleza Emergencia.
- **Efectos:**
  - se guardan el diagnóstico, la prioridad y el kilometraje;
  - el kilometraje actual del vehículo se actualiza con el registrado;
  - si el Coordinador corrigió el tipo de mantenimiento, se guarda el nuevo;
  - se registra en el historial de la orden.
- **Avance:** 2.

### Diagnosticada → Aprobada

- **Rol:** Administrador.
- **Acción:** `POST api/OrdenMantenimiento/{id}/aprobar`.
- **Condiciones:** fecha programada obligatoria y no anterior a hoy.
- **Efectos:** se guarda la fecha programada y se registra en el historial de la orden.
- **Avance:** 2.

### Diagnosticada → Rechazada

- **Rol:** Administrador.
- **Acción:** `POST api/OrdenMantenimiento/{id}/rechazar`.
- **Condiciones:** ninguna adicional; la observación con el motivo es opcional.
- **Efectos:** se registra la fecha de cierre y la transición en el historial de la orden. El vehículo no cambia.
- **Avance:** 2.

### Aprobada → En Ejecución

- **Rol:** Coordinador/Técnico.
- **Acción:** `POST api/OrdenMantenimiento/{id}/iniciar`, con la lista de técnicos.
- **Condiciones:**
  - al menos un técnico;
  - cada técnico debe estar activo, disponible y poseer la especialidad que requiere el tipo de mantenimiento;
  - no se repiten técnicos.
- **Efectos:**
  - se crean las asignaciones con su fecha y 0 horas;
  - si el vehículo estaba Operativo o Fuera de Servicio, pasa a En Mantenimiento y se registra en su historial, con la orden que provocó el cambio;
  - si el vehículo ya estaba En Mantenimiento por otra orden, no cambia ni se registra nada en su historial;
  - se registra la transición en el historial de la orden.
- **Avance:** 2.

### En Ejecución → En Espera de Repuesto (automática)

- **Rol:** la dispara el sistema cuando un Coordinador/Técnico registra repuestos y alguno no tiene stock suficiente.
- **Acción:** `POST api/OrdenMantenimiento/{id}/repuestos`.
- **Condiciones:** no existe una acción manual para pasar a este estado.
- **Efectos:**
  - las líneas con stock se registran en el detalle y se descuenta su stock;
  - las líneas sin stock completo quedan como pendientes, sin surtidos parciales;
  - la orden cambia de estado;
  - se registra en el historial con una observación automática que indica qué falta, por ejemplo "Stock insuficiente de AMO-DEL-001: requeridos 2, disponibles 0";
  - la respuesta de la API informa el nuevo estado y lo que falta;
  - el vehículo sigue En Mantenimiento.
- **Avance:** 2.

### En Espera de Repuesto → En Ejecución

- **Rol:** Coordinador/Técnico.
- **Acción:** `POST api/OrdenMantenimiento/{id}/reanudar`.
- **Condiciones:** todos los repuestos pendientes deben tener stock suficiente. Si falta alguno, se rechaza indicando cuál.
- **Efectos:**
  - cada pendiente se convierte en una línea del detalle, con el costo unitario vigente;
  - se descuenta su stock y se eliminan los pendientes;
  - se registra en el historial de la orden.
- **Avance:** 2.

### En Ejecución → Completada

- **Rol:** Coordinador/Técnico.
- **Acción:** `POST api/OrdenMantenimiento/{id}/completar`.
- **Condiciones:**
  - no hay repuestos pendientes;
  - al menos un técnico tiene horas registradas.
- **Efectos:**
  - se copia la tarifa vigente de cada técnico a su asignación;
  - se calculan y guardan el costo de repuestos, el de mano de obra y el total;
  - se registra la fecha de cierre;
  - si el tipo es de naturaleza Preventivo, el kilometraje del último preventivo del vehículo pasa a ser el registrado en el diagnóstico;
  - el vehículo vuelve a Operativo, salvo que tenga otra orden En Ejecución o En Espera de Repuesto;
  - se registra en el historial de la orden y, si el vehículo cambió, también en el suyo.
- **Avance:** 2.

### Solicitada, Diagnosticada o Aprobada → Cancelada

- **Rol:** Administrador o Coordinador/Técnico. *Decisión del equipo:* la sección 11 no lista cancelar para el Coordinador; ver [requerimientos](requerimientos.md), sección 7.5.
- **Acción:** `POST api/OrdenMantenimiento/{id}/cancelar`, con observación opcional.
- **Condiciones:** ninguna adicional.
- **Efectos:** se registran la fecha de cierre y la transición en el historial de la orden. El vehículo conserva su estado, porque todavía no había entrado a mantenimiento por esta orden.
- **Avance:** 2.

### En Ejecución o En Espera de Repuesto → Cancelada

- **Rol:** Administrador o Coordinador/Técnico.
- **Acción:** `POST api/OrdenMantenimiento/{id}/cancelar`, con observación opcional.
- **Condiciones:** ninguna adicional.
- **Efectos:**
  - se congelan la tarifa de cada asignación y los costos de lo que ya se consumió;
  - se eliminan los pendientes;
  - el stock ya descontado no se devuelve: si las piezas no se instalaron, el Administrador registra una entrada;
  - se registra la fecha de cierre;
  - el vehículo vuelve a Operativo, como pide el enunciado, salvo que tenga otra orden En Ejecución o En Espera de Repuesto;
  - se registra en los historiales que correspondan.
- *Decisión del equipo:* la sección 9 no lista la cancelación desde En Espera de Repuesto, pero la sección 8 permite cancelar "en cualquier punto previo al cierre". Como la matriz es un mínimo, la agregamos.
- **Avance:** 2.

### Acciones que no cambian el estado

- **Editar la solicitud:** solo el Administrador y solo en Solicitada. Puede cambiar el motivo y el tipo de mantenimiento; el vehículo de una orden no se cambia. Si el vehículo está En Mantenimiento, el nuevo tipo debe ser de naturaleza Emergencia.
- **Registrar horas:** solo el Coordinador/Técnico y solo En Ejecución.
- **Agregar técnicos:** solo el Coordinador/Técnico y solo En Ejecución.
- **Registrar repuestos con stock suficiente:** solo el Coordinador/Técnico y solo En Ejecución.

---

## Transiciones rechazadas

Toda transición que no aparece arriba se rechaza. Estas son las que el enunciado prohíbe expresamente, más las que se deducen de los estados finales:

- Solicitada no puede pasar directamente a Aprobada, En Ejecución ni Completada.
- Diagnosticada no puede regresar a Solicitada.
- Aprobada no puede pasar directamente a Completada sin ejecución.
- En Ejecución no puede completarse con repuestos pendientes.
- En Espera de Repuesto solo puede reanudarse con stock suficiente; no puede completarse directamente.
- Rechazada, Completada y Cancelada no admiten ninguna transición.
- Ningún estado puede regresar a uno anterior, salvo En Espera de Repuesto → En Ejecución.

## Cómo se rechaza

La matriz vive en código, en `SGMA.Application`, como un diccionario de solo lectura llamado `MatrizTransicionesOrden`. Para cada estado de origen indica los estados destino permitidos y los roles que pueden ejecutar cada transición.

Cada método del servicio que cambia el estado sigue este orden:

1. Busca la orden. Si no existe, lanza `RecursoNoEncontradoException`, que la API responde con **404**.
2. Comprueba en la matriz que la transición esté permitida desde el estado actual y para el rol del usuario. Si no lo está, lanza `TransicionNoPermitidaException`, que se responde con **409**.
3. Verifica las condiciones propias de la transición (técnicos, stock, horas, fechas). Si alguna falla, lanza `ReglaNegocioException`, también con **409**.
4. Aplica todos los efectos y el historial en una sola transacción.

Las excepciones viven en `SGMA.Shared/Exceptions`. El manejador global de excepciones de `SGMA.WebAPI` las traduce a una respuesta ProblemDetails con un mensaje en español, nunca a un error genérico. Por ejemplo:

```json
{
  "title": "Transición no permitida",
  "status": 409,
  "detail": "La orden OM-2026-00003 está Diagnosticada y no puede pasar a Completada.",
  "estadoActual": "Diagnosticada",
  "estadoDestino": "Completada"
}
```

Otras respuestas:

- Los errores de formato en los datos enviados (campos obligatorios, longitudes) responden **400**, por la validación de los DTO.
- Desde el Avance 2, un endpoint llamado sin token válido responde **401**, y con un rol sin permiso, **403**. El rol se valida dos veces: en el atributo `[Authorize]` del controlador y en la matriz.

---

## Estados del vehículo

1. **Operativo:** disponible para circular. Es el estado inicial de todo vehículo.
2. **En Mantenimiento:** en el taller por una orden En Ejecución o En Espera de Repuesto.
3. **Fuera de Servicio:** inmovilizado por decisión administrativa, por ejemplo tras un accidente.
4. **Dado de Baja:** retirado de la flota de forma definitiva. Es final.

## Transiciones del vehículo

### Automáticas, provocadas por las órdenes

Ningún usuario las ejecuta directamente.

- **Operativo o Fuera de Servicio → En Mantenimiento:** cuando una orden del vehículo pasa a En Ejecución.
- **En Mantenimiento → Operativo:** cuando una orden del vehículo se completa, o se cancela desde En Ejecución o En Espera de Repuesto, y ninguna otra orden del vehículo queda En Ejecución o En Espera de Repuesto.

En el historial del vehículo, estos cambios quedan con el usuario que ejecutó la acción sobre la orden y con la orden que los provocó.

### Manuales, exclusivas del Administrador

No requieren una orden.

- **Operativo → Fuera de Servicio.**
- **Fuera de Servicio → Operativo.** *Decisión del equipo:* el enunciado no define cómo sale un vehículo de Fuera de Servicio; permitimos que el Administrador lo reactive.
- **Operativo → Dado de Baja.**
- **Fuera de Servicio → Dado de Baja.**

**Acción:** `POST api/Activo/{id}/estado`, con el estado destino y una observación opcional. "Dar de baja" (`DELETE api/Activo/{id}`) es la misma transición hacia Dado de Baja.

**Condiciones:**

- Ninguna transición manual sale de En Mantenimiento. Así, el Administrador nunca cambia a mano un vehículo que tiene una orden En Ejecución o En Espera de Repuesto; si pudiera, el sistema pisaría su cambio al cerrar esa orden.
- Para dar de baja, el vehículo no puede tener órdenes abiertas.
- Dado de Baja no admite ninguna transición posterior.

Una transición manual no permitida se rechaza con `TransicionNoPermitidaException` (**409**), igual que en las órdenes.

**Avance:** las transiciones manuales y su historial van en el Avance 1; las automáticas, en el Avance 2.

---

## Historial

- Cada cambio de estado real de una orden o de un vehículo crea un registro con estado anterior, estado nuevo, fecha, usuario responsable y observación opcional.
- Al crear una orden o un vehículo se registra su estado inicial con estado anterior vacío.
- Si el estado no cambia, no se registra nada.
- El historial lo escriben solo los servicios, dentro de la misma transacción que el cambio. No hay endpoints para insertarlo, editarlo ni borrarlo.

## Casos borde que debemos poder explicar

- **Dos órdenes en ejecución sobre el mismo vehículo** (una normal y una de emergencia). El vehículo pasa a En Mantenimiento con la primera y solo vuelve a Operativo cuando las dos terminan.
- **Una orden de emergencia creada sobre un vehículo En Mantenimiento.** Se permite. Las órdenes normales se rechazan al crearlas, pero las que ya estaban abiertas antes de que el vehículo entrara al taller siguen su curso.
- **Un vehículo Fuera de Servicio con una orden que se ejecuta y se cancela.** Vuelve a Operativo, como indica el enunciado. Si sigue inmovilizado, el Administrador lo devuelve a Fuera de Servicio manualmente.
- **Dos órdenes esperan el mismo repuesto y llega stock solo para una.** La primera que se reanude lo consume; la otra sigue en espera.
- **Un Coordinador intenta completar una orden En Espera de Repuesto.** Se rechaza con `TransicionNoPermitidaException`: primero debe reanudarse.
- **Un vehículo Dado de Baja.** No admite órdenes nuevas ni cambios de estado, y queda fuera de la evaluación preventiva.

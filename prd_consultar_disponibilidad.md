## PRD: Consultar disponibilidad de una sala

**Problem Statement**

Los usuarios necesitan conocer, antes de intentar reservar, si una sala está disponible dentro de un rango de fecha/hora y cuáles son los "slots" libres para poder seleccionar uno adecuado. Actualmente la API tiene entidades de `Reservation`, `Room` y `User`, y permite crear reservas sin validaciones ni consultas de disponibilidad; no existe un endpoint ni una capa dedicada que calcule y devuelva los huecos libres para una sala en un rango dado.

**Solution**

Agregar un endpoint de lectura `GET /api/rooms/{roomId}/availability` que, dado un rango UTC y parámetros de segmentación, devuelva los slots libres presegmentados (por defecto 30 minutos) dentro del rango solicitado. La implementación será una capa de solo lectura compuesta por un servicio de disponibilidad (`AvailabilityService`) y un calculador puro de slots (`SlotCalculator`) que, a partir de las reservas existentes (consultadas contra la DB), compute los huecos y los segmente. La respuesta incluye metadatos (timestamps en ISO8601 UTC, `isActive` de la sala, parámetros usados) y una lista de slots `{ startUtc, endUtc }` en UTC.

**User Stories**

1. Como usuario anónimo, quiero consultar si una sala está disponible entre dos instantes UTC, para saber si puedo reservar sin autenticarme.
2. Como usuario autenticado, quiero obtener la lista de slots libres en un rango dado, para elegir un horario y hacer una reserva posteriormente.
3. Como usuario en móvil, quiero poder pedir slots en granulado de 30 minutos por defecto, para visualizar una cuadrícula de disponibilidad en la app.
4. Como usuario, quiero poder filtrar los resultados por una `requiredDurationMinutes` mínima, para encontrar sólo slots que me permitan una reunión de mi duración.
5. Como integrador, quiero enviar `startUtc` y `endUtc` en formato ISO8601 UTC, y recibir las mismas marcas en la respuesta, para evitar ambigüedades de zona horaria.
6. Como usuario, quiero que si la sala está marcada como inactiva reciba una respuesta clara que lo indique, para no intentar reservarla.
7. Como usuario, quiero recibir `404 NotFound` si el `roomId` no existe, para corregir mi petición.
8. Como usuario, quiero que se permitan reservas contiguas (cuando `existing.EndUtc == requested.StartUtc`), para poder reservar inmediatamente después de otra reserva sin conflicto.
9. Como UI developer, quiero que los slots estén alineados desde `requestedStartUtc` (no desde bordes de reloj), para que la vista refleje exactamente el rango solicitado.
10. Como consumidor de API, quiero poder pasar `slotMinutes` (opcional) para ajustar la granularidad del resultado.
11. Como consumidor de API, quiero que el endpoint valide `startUtc < endUtc` y devuelva `400 BadRequest` en caso contrario.
12. Como consumidor de API, quiero que exista un límite de rango por consulta (30 días) para evitar consultas demasiado grandes.
13. Como consumidor de API, quiero validación para `slotMinutes` y `requiredDurationMinutes` (ej. min 5, max 1440), con `400` si los valores son inválidos.
14. Como product owner, quiero que el endpoint sea público (sin auth) para facilitar búsquedas previas a la reserva.
15. Como desarrollador, quiero que la lógica de cálculo de slots sea una función pura y testeable (`SlotCalculator`), para probar todos los escenarios sin depender de la base de datos.
16. Como desarrollador, quiero que el servicio de disponibilidad (`AvailabilityService`) realice sólo lecturas y use queries eficientes para recuperar las reservas que intersectan el rango pedido.
17. Como QA, quiero pruebas unitarias que cubran casos límite: solapamiento parcial, solapamiento total, adyacencias exactas y huecos pequeños.
18. Como desarrollador, quiero que el servicio devuelva también los parámetros usados (`slotMinutes`, `requiredDurationMinutes`) en la respuesta para facilitar depuración en el cliente.
19. Como usuario, quiero poder obtener slots presegmentados y listos para mostrar en UI sin que el cliente tenga que subdividir manualmente.
20. Como administrador, quiero que existan datos seed de ejemplo (reservas sobre distintas salas) para pruebas manuales localmente.
21. Como desarrollador, quiero que la solución permita en el futuro añadir caching en capa de lectura sin cambiar la interfaz del servicio.
22. Como equipo, queremos tests de servicio (capa de dominio / aplicación) como prioridad para prevenir regresiones.
23. Como auditor, quiero que el sistema registre la consulta (por debugging) en caso de errores no esperados.
24. Como usuario, quiero recibir timestamps con precisión suficiente (segundos) en formato ISO8601 `Z` para interoperabilidad.
25. Como integrador, quiero que el endpoint responda rápidamente y tenga límites razonables para evitar cargar la base de datos innecesariamente.

**Implementation Decisions**

- **Módulos principales**: construir un `AvailabilityService` (capa de aplicación) y un `SlotCalculator` (módulo puro/funcional). El `AvailabilityService` será la fachada que orquesta:
  - validación de parámetros,
  - consulta de reservas que intersecten el rango solicitado,
  - llamada a `SlotCalculator` para calcular huecos y segmentar,
  - mapeo a DTO de respuesta.

- **`SlotCalculator` (deep module)**: módulo determinista y sin efectos laterales que reciba `requestedStartUtc`, `requestedEndUtc`, lista de reservas (ordenadas), `slotMinutes` y `requiredDurationMinutes?` y devuelva una lista de slots `{ startUtc, endUtc }`. Este módulo será la pieza más testeable y aislada.

- **Consultas a la base**: el servicio pedirá sólo reservas que intersecten el rango mediante la condición `existing.StartUtc < requested.EndUtc && existing.EndUtc > requested.StartUtc` (semántica end-exclusive). Esto minimiza la cantidad de datos leídos.

- **Semántica de solapamiento**: usar regla end-exclusive (toques exactos permitidos): si `existing.EndUtc == candidate.StartUtc` no se considera conflicto.

- **API contract**: `GET /api/rooms/{roomId}/availability?startUtc={}&endUtc={}&slotMinutes={30}&requiredDurationMinutes={}`.
  - Respuesta 200: objeto con `{ roomId, requestedStartUtc, requestedEndUtc, slotMinutes, requiredDurationMinutes?, isActive: bool, slots: [{startUtc,endUtc}], message? }`.
  - `404 NotFound`: si `roomId` no existe.
  - `400 BadRequest`: si parámetros inválidos (`startUtc>=endUtc`, `slotMinutes` fuera de rango, `requiredDurationMinutes` fuera de rango, rango > 30 días).
  - Para salas inactivas: devolver `200 OK` con `isActive: false` y `message: "La sala no está activa"` (decisión tomada para mostrar contexto al cliente en vez de un error duro).

- **Parámetros y límites**:
  - `slotMinutes` por defecto: 30.
  - `requiredDurationMinutes`: opcional; si presente, filtrar slots que no alcancen esa duración.
  - `slotMinutes` y `requiredDurationMinutes` validación: integer entre 5 y 1440.
  - Máximo rango por consulta: 30 días.
  - Timestamps: exigir UTC en requests; responder en ISO8601 con `Z`.

- **Cambios a capas existentes**: no se cambiará el comportamiento de `ReservationService.CreateAsync` en esta iteración (la palabra del equipo fue mantenerlo sin validaciones); en su lugar, se añadirá una consulta de solo-lectura que recupere reservas para el `AvailabilityService`.

- **Datos de desarrollo**: añadir reservas seed de ejemplo en entorno de desarrollo para facilitar pruebas manuales.

**Testing Decisions**

- **Qué hace una buena prueba**: verificar comportamiento externo (entradas/salidas) sin atarse a detalles internos; para `SlotCalculator` se usarán pruebas puras (unitarias) que pasen listas de reservas y verifiquen los slots resultantes.

- **Módulos a testear (prioridad)**:
  - `SlotCalculator`: unit tests exhaustivos (casos límite, solapamientos parciales, adyacencias, huecos pequeños, filtrado por `requiredDurationMinutes`).
  - `AvailabilityService`: pruebas de integración en memoria (usar la configuración de tests existente que usa InMemory DB) para validar la consulta de reservas + integración con `SlotCalculator` y respuestas HTTP mapeadas desde el controller.
  - Tests del controller: opcional (baja prioridad).

- **Casos de prueba específicos**:
  1. Rango sin reservas → un slot que cubre todo el rango (segmentado en `slotMinutes`).
  2. Reservas que cortan el rango en varios huecos → slots en los huecos correctos.
  3. Reserva que toca exactamente el inicio o fin del rango → confirmar regla end-exclusive.
  4. Filtro por `requiredDurationMinutes` → slots menores son descartados.
  5. Parámetros inválidos (`startUtc>=endUtc`, `slotMinutes` fuera de límites) → `400 BadRequest`.
  6. Sala inactiva → respuesta `200` con `isActive:false` y mensaje.

**Out of Scope**

- Forzar validaciones en `ReservationService.CreateAsync` ni reescribir la lógica de creación de reservas (conflicto/validaciones) en esta entrega.
- Implementación de reservas recurrentes.
- Notificaciones enriquecidas de conflicto (sugerencias inteligentes) — puede ser una feature futura.
- Caching avanzado o estrategias distribuidas de cache.
- Políticas de autenticación/roles para consultar disponibilidad (el endpoint será público en primer lanzamiento).

**Further Notes**

- Recomendación futura: centralizar la regla de solapamiento en un helper `HasConflict` reutilizable y aplicar esa regla tanto en creación de reservas como en el `AvailabilityService` para evitar divergencias.
- Posible mejora: ofrecer una opción para alinear los slots a bordes de reloj (00:00/00:30) si el cliente lo necesita.
- Si la carga crece, considerar indexar las columnas de tiempo y añadir caching con TTL corto para consultas frecuentes.
- Entregar el PRD como Issue en GitHub si el equipo lo aprueba; el contenido está listo para pegar en la descripción del Issue.



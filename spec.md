# Spec: Reservaciones (Features A + B + C + D)

## Contexto del entregable

Grupo: estudiantes 227056 y 264272.

Esta spec es la fuente de verdad del feature de reservaciones sobre el proyecto
RoomBookingApi. Se escribio antes del codigo y los tests unitarios se derivan
uno-a-uno de los scenarios listados mas abajo.

## Workflow SDD seguido

Usamos un workflow propio inspirado en el Sequential Handoff definido en
`.github/agents/` del repositorio. La secuencia es:

1. **Feature Planner**: leer el codebase (Rooms, Users, DbContext, patrones existentes)
   y escribir esta spec. Solo lectura, cero codigo de produccion.
2. **Feature Implementer**: implementar Domain -> DTOs -> Service -> Controller ->
   Seeder respetando los patrones de `RoomsController` y `UsersController`. No escribe
   tests. Verifica con `dotnet build`.
3. **Feature Tester**: escribir tests unitarios en MSTest, uno por cada scenario de
   esta spec, con el mismo nombre `Metodo_Scenario_ResultadoEsperado`. Verifica con
   `dotnet test`.
4. **Thorough Reviewer** (opcional): multi-perspective review antes del merge.

Cada fase tiene separacion estricta de responsabilidades: el Planner no codea, el
Implementer no escribe tests, el Tester no toca produccion. Esto hace que cada
scenario de la spec tenga trazabilidad directa a un test (cada test lleva un comentario
`// Scenario Xn` al comienzo).

## Descripcion global

Una API que permite a los usuarios **reservar salas**, **cancelar** sus reservas,
**consultar la disponibilidad** de una sala en un rango horario y **listar las
reservas** asociadas a un usuario.

## Non-goals

- Sin autenticacion ni autorizacion. El `UserId` llega en el body/URL y se confia.
- Sin edicion de reserva. Una reserva solo se puede cancelar, no modificar.
- Sin recurrencia, invitados ni notificaciones.
- Sin paginacion en los endpoints de listado.

## Modelo de datos

`Reservation`:
- `Id`: int (auto)
- `RoomId`: int (FK a Room)
- `UserId`: int (FK a User)
- `StartAt`: DateTime en UTC
- `EndAt`: DateTime en UTC, exclusivo
- `Status`: enum `Active` | `Cancelled`
- `CreatedAt`: DateTime en UTC
- `CancelledAt`: DateTime? en UTC (null si nunca se cancelo)

EF Core InMemory no enforza FKs, por lo que la validacion de existencia de Room y User
se hace explicitamente en el service.

## Convenciones

- Todas las fechas se tratan como UTC. El service normaliza con `.ToUniversalTime()`
  antes de persistir o comparar.
- `EndAt` es exclusivo: dos reservas `[09:00, 10:00)` y `[10:00, 11:00)` NO se
  superponen (back-to-back valido).
- La cancelacion es "soft": se marca el `Status` y se registra el timestamp
  `CancelledAt`, pero la fila no se borra. Las reservas canceladas aparecen en los
  listados pero no bloquean disponibilidad.

---

## Feature A: Crear Reserva

### Endpoint
- Metodo: `POST`
- Ruta: `/api/reservations`
- Body: `CreateReservationDto { roomId, userId, startAt, endAt }`
- 201 Created con `ReservationDto` y header `Location` (via `CreatedAtAction(GetById)`)
- 400 Bad Request (errores de forma o de validacion simple)
- 404 Not Found (room o user inexistente)
- 409 Conflict (overlap con otra reserva activa, o sala inactiva)

### Scenarios

#### Scenario A1: Happy path — crear reserva valida
- GIVEN una sala activa y un usuario existentes, sin reservas previas que solapen
- WHEN se envia `POST /api/reservations` con fechas futuras y `EndAt > StartAt`
- THEN la API responde 201 Created con la reserva; `Status = "Active"`,
  `CancelledAt = null`, y la fila queda persistida

#### Scenario A2: Sala inactiva
- GIVEN una sala existente con `IsActive = false`
- WHEN se intenta crear una reserva para esa sala
- THEN la API responde 409 Conflict con mensaje `"Room is not active."`

#### Scenario A3: Overlap exacto con reserva activa
- GIVEN una reserva activa `[10:00, 11:00)` en la sala X
- WHEN se intenta crear otra reserva `[10:00, 11:00)` en la sala X
- THEN la API responde 409 Conflict con mensaje
  `"Room is already booked for the requested range."`

#### Scenario A4: Overlap parcial con reserva activa
- GIVEN una reserva activa `[10:00, 12:00)` en la sala X
- WHEN se intenta crear otra reserva `[11:00, 13:00)` en la sala X
- THEN la API responde 409 Conflict

#### Scenario A5: Reservas back-to-back permitidas
- GIVEN una reserva activa `[10:00, 11:00)` en la sala X
- WHEN se crea otra reserva `[11:00, 12:00)` en la sala X
- THEN la API responde 201 Created (no hay solape porque `EndAt` es exclusivo)

#### Scenario A6: Fechas invertidas
- GIVEN un payload con `EndAt <= StartAt`
- WHEN se envia `POST /api/reservations`
- THEN la API responde 400 Bad Request con mensaje `"EndAt must be after StartAt."`

#### Scenario A7: StartAt en el pasado
- GIVEN un payload con `StartAt` anterior al momento actual
- WHEN se envia `POST /api/reservations`
- THEN la API responde 400 Bad Request con mensaje `"StartAt cannot be in the past."`

#### Scenario A8: Usuario inexistente
- GIVEN un `UserId` que no existe en la base
- WHEN se envia `POST /api/reservations`
- THEN la API responde 404 Not Found con mensaje `"User not found."`

#### Scenario A9: Sala inexistente
- GIVEN un `RoomId` que no existe en la base
- WHEN se envia `POST /api/reservations`
- THEN la API responde 404 Not Found con mensaje `"Room not found."`

#### Scenario A10: Overlap con reserva cancelada no bloquea
- GIVEN una reserva `[10:00, 11:00)` en la sala X que luego fue cancelada
- WHEN se crea otra reserva `[10:00, 11:00)` en la sala X
- THEN la API responde 201 Created (las reservas canceladas no bloquean disponibilidad)

---

## Feature B: Cancelar Reserva

### Endpoint
- Metodo: `POST`
- Ruta: `/api/reservations/{id}/cancel`
- Body: vacio
- 204 No Content (cancelada exitosamente)
- 404 Not Found (id no existe)
- 409 Conflict (ya estaba cancelada)

Nota: se eligio `POST /{id}/cancel` sobre `DELETE /{id}` porque (1) la cancelacion es
un soft-delete con auditoria (`CancelledAt`) y no una remocion fisica como la que
aplican `RoomsController` y `UsersController`, y (2) queremos devolver 409 si la
reserva ya estaba cancelada, lo que rompe la idempotencia estricta de DELETE.

### Scenarios

#### Scenario B1: Cancelar reserva activa
- GIVEN una reserva existente con `Status = Active`
- WHEN se envia `POST /api/reservations/{id}/cancel`
- THEN la API responde 204; la reserva queda con `Status = Cancelled` y
  `CancelledAt` seteado al momento de cancelacion

#### Scenario B2: Cancelar reserva ya cancelada
- GIVEN una reserva con `Status = Cancelled`
- WHEN se envia `POST /api/reservations/{id}/cancel`
- THEN la API responde 409 Conflict con mensaje `"Reservation already cancelled."`

#### Scenario B3: Cancelar id inexistente
- GIVEN un `id` que no existe
- WHEN se envia `POST /api/reservations/{id}/cancel`
- THEN la API responde 404 Not Found con mensaje `"Reservation not found."`

---

## Feature C: Consultar Disponibilidad

### Endpoint
- Metodo: `GET`
- Ruta: `/api/reservations/availability?roomId=X&startAt=Y&endAt=Z`
- 200 OK con
  `AvailabilityDto { roomId, startAt, endAt, isAvailable, conflictingReservationsCount }`
- 400 Bad Request (parametros invalidos)

### Scenarios

#### Scenario C1: Rango libre
- GIVEN una sala sin reservas activas en el rango consultado
- WHEN se envia `GET /api/reservations/availability?roomId=1&startAt=...&endAt=...`
- THEN la API responde 200 con `IsAvailable = true` y `ConflictingReservationsCount = 0`

#### Scenario C2: Rango bloqueado por reserva activa
- GIVEN una reserva activa en la sala que solapa totalmente con el rango consultado
- WHEN se consulta disponibilidad para ese rango
- THEN la API responde 200 con `IsAvailable = false` y `ConflictingReservationsCount = 1`

#### Scenario C3: Solape parcial
- GIVEN una reserva activa `[10:00, 12:00)` en la sala X
- WHEN se consulta disponibilidad para `[11:00, 13:00)` en la sala X
- THEN la API responde 200 con `IsAvailable = false` y `ConflictingReservationsCount = 1`

#### Scenario C4: Solo reserva cancelada en el rango
- GIVEN una reserva cancelada en la sala que solapa con el rango consultado
- WHEN se consulta disponibilidad para ese rango
- THEN la API responde 200 con `IsAvailable = true` (las canceladas no bloquean)

---

## Feature D: Listar Reservas del Usuario

### Endpoint
- Metodo: `GET`
- Ruta: `/api/reservations/user/{userId}`
- 200 OK con lista de `ReservationDto` (incluye canceladas)
- La lista se devuelve vacia si el usuario no tiene reservas o no existe

### Scenarios

#### Scenario D1: Usuario con varias reservas
- GIVEN un usuario con 3 reservas (una temprana, una mediana, una cancelada)
- WHEN se envia `GET /api/reservations/user/{userId}`
- THEN la API responde 200 con las 3 reservas ordenadas por `StartAt` ascendente,
  incluyendo la cancelada

#### Scenario D2: Usuario sin reservas
- GIVEN un usuario que existe pero no tiene reservas
- WHEN se envia `GET /api/reservations/user/{userId}`
- THEN la API responde 200 con `[]`

#### Scenario D3: Usuario inexistente
- GIVEN un `userId` que no existe
- WHEN se envia `GET /api/reservations/user/{userId}`
- THEN la API responde 200 con `[]` (no 404; el contrato expone una vista
  "reservas de X" que esta vacia si X no existe)

---

## Plan de implementacion

Archivos a crear:
- `RoomBookingApi/DTOs/CreateReservationDto.cs`
- `RoomBookingApi/DTOs/AvailabilityDto.cs`

Archivos a modificar:
- `RoomBookingApi/Domain/Reservation.cs` (enum `ReservationStatus` + campos)
- `RoomBookingApi/DTOs/ReservationDto.cs` (campos de salida)
- `RoomBookingApi/Services/ReservationService.cs` (7 metodos + helper)
- `RoomBookingApi/Controllers/ReservationsController.cs` (6 endpoints + 2 helpers)
- `RoomBookingApi/Data/AppDbSeeder.cs` (reservas seed para demo via Swagger)
- `RoomBookingApi.Tests/ReservationServiceTests.cs` (un test por scenario)

Firmas principales del service:
- `Task<(Reservation? Reservation, string? Error)> CreateAsync(Reservation r)`
- `Task<(bool Success, string? Error)> CancelAsync(int id)`
- `Task<(bool IsAvailable, int ConflictingCount)> IsRoomAvailableAsync(int roomId, DateTime startAt, DateTime endAt)`
- `Task<List<Reservation>> GetByUserAsync(int userId)`
- `Task<List<Reservation>> GetAllAsync()`
- `Task<Reservation?> GetByIdAsync(int id)`

---

## Ajustes que harian a la spec (nota final post-implementacion)

Durante la implementacion surgieron decisiones que conviene dejar explicitas en una
proxima iteracion de la spec:

1. Formalizar los codigos de error como un enum `ReservationError` devuelto por el
   service, en lugar de mapear por keyword en el string del mensaje. El mapeo actual es
   simple pero fragil ante cambios de wording.
2. Agregar scenarios explicitos para `userId <= 0` y `roomId <= 0` a nivel endpoint
   (hoy los atrapa la validacion de forma del controller pero no hay scenario formal).
3. Documentar el contrato de DateTime `Kind`: el service normaliza a UTC cualquier
   valor entrante, pero la spec no obliga al cliente a enviar UTC. Conviene dejarlo
   escrito.
4. Definir explicitamente el comportamiento de reservas que cruzan medianoche o dia
   (hoy funcionan porque solo comparamos DateTime, pero no hay scenario que lo valide).
5. Definir paginacion y filtros (status, rango de fechas) para los endpoints de
   listado cuando crezca el volumen de datos.

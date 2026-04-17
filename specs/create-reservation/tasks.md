# Tasks – Crear Reserva

Lista ordenada. Cada task es atómica y verificable. No pasar a la siguiente hasta que la actual compile (o corra).

## Domain + DTOs

- [ ] **T1.** `Domain/Reservation.cs` – reemplazar por la clase con `Id`, `RoomId`, `UserId`, `StartTime`, `EndTime`.
- [ ] **T2.** `DTOs/CreateReservationDto.cs` – crear archivo nuevo con los 4 campos de entrada.
- [ ] **T3.** `DTOs/ReservationDto.cs` – reemplazar esqueleto por la clase con los 5 campos de salida.
- [ ] **Check:** `dotnet build RoomBooking.sln` verde.

## Service

- [ ] **T4.** `Services/ReservationService.cs` – definir `CreateReservationStatus` enum + `CreateReservationResult` record al final del archivo (o en namespace Services).
- [ ] **T5.** `Services/ReservationService.cs` – inyectar `AppDbContext` por constructor primario.
- [ ] **T6.** `Services/ReservationService.cs` – implementar `CreateAsync(int roomId, int userId, DateTime startTime, DateTime endTime)` con las 7 validaciones en el orden de `plan.md`.
- [ ] **Check:** `dotnet build` verde.

## Controller

- [ ] **T7.** `Controllers/ReservationsController.cs` – inyectar `ReservationService` por constructor primario.
- [ ] **T8.** `Controllers/ReservationsController.cs` – agregar `GetById(int id)` (stub que devuelve `NotFound`) para que `CreatedAtAction` tenga un target válido. *Nota:* no está en los scenarios pero es necesario para el `Location` header del 201. Solo stub, no entra en la spec.
- [ ] **T9.** `Controllers/ReservationsController.cs` – implementar `POST` con mapeo status → HTTP de la tabla en `plan.md`.
- [ ] **T10.** Helper privado estático `MapToDto(Reservation)` (mismo patrón que `RoomsController`).
- [ ] **Check:** `dotnet build` verde. Probar manualmente con Swagger/`.http` (opcional).

## Tests

Un test por scenario, usando `NewDb()` helper.

- [ ] **T11.** Tests – agregar helper `NewDb()` al inicio de la clase.
- [ ] **T12.** `Scenario_01_CreateReservation_WithValidData_Persists` – happy path.
- [ ] **T13.** `Scenario_02_CreateReservation_WhenOverlapping_ReturnsConflict`.
- [ ] **T14.** `Scenario_03_CreateReservation_WhenRoomInactive_ReturnsBadRequest` – verifica status `RoomInactive`.
- [ ] **T15.** `Scenario_04_CreateReservation_WhenStartNotBeforeEnd_ReturnsBadRequest` – verifica status `InvalidTimeRange`.
- [ ] **T16.** `Scenario_05_CreateReservation_WhenStartInPast_ReturnsBadRequest` – verifica status `StartInPast`.
- [ ] **T17.** `Scenario_06_CreateReservation_WhenRoomDoesNotExist_ReturnsNotFound` – verifica status `RoomNotFound`.
- [ ] **T18.** `Scenario_07_CreateReservation_WhenUserDoesNotExist_ReturnsNotFound` – verifica status `UserNotFound`.
- [ ] **T19.** Borrar el `Placeholder_ReplaceWithScenarioBasedTests`.
- [ ] **Check final:** `dotnet test RoomBooking.sln` → 7 tests verdes.

## Criterio de done del feature

1. `dotnet build` sin warnings nuevos.
2. `dotnet test` con 7 tests verdes (Scenario_01 a Scenario_07).
3. Cada row de la tabla **Cobertura** de `spec.md` tiene su test correspondiente.
4. Ningún test toca archivos fuera de `Reservation*`, `ReservationService`, `ReservationsController`.

# Tasks – Cancelar Reserva

Orden asume que Crear Reserva ya está implementado (entidad + DbSet + registración DI).

## Service

- [ ] **T1.** `Services/ReservationService.cs` – agregar enum `CancelReservationStatus` (`Success`, `NotFound`, `AlreadyStarted`).
- [ ] **T2.** `Services/ReservationService.cs` – implementar `CancelAsync(int id)` siguiendo el orden de validación de `plan.md`.
- [ ] **Check:** `dotnet build` verde.

## Controller

- [ ] **T3.** `Controllers/ReservationsController.cs` – agregar endpoint `DELETE {id:int}` con mapeo status → HTTP.
- [ ] **Check:** `dotnet build` verde.

## Tests

- [ ] **T4.** `Scenario_01_CancelReservation_WhenFuture_RemovesIt` – sembrar reserva con `StartTime = UtcNow.AddHours(+2)`, cancelar, assert status `Success` y `dbContext.Reservations.Count() == 0`.
- [ ] **T5.** `Scenario_02_CancelReservation_WhenNotFound_ReturnsNotFound` – db vacía, cancelar `id = 999`, assert `NotFound`.
- [ ] **T6.** `Scenario_03_CancelReservation_WhenAlreadyStarted_ReturnsConflict` – sembrar reserva con `StartTime = UtcNow.AddHours(-1)` y `EndTime = UtcNow.AddHours(+1)`, cancelar, assert `AlreadyStarted` y la reserva **sigue** en la base.
- [ ] **Check final:** `dotnet test` – nuevos 3 tests verdes, sumados a los 7 del feature A → 10 tests totales.

## Criterio de done del feature

1. `dotnet build` sin warnings nuevos.
2. `dotnet test` con 3 tests nuevos verdes (Scenario_01 a 03 de cancelación) + los 7 de Crear Reserva.
3. Cada row de la tabla **Cobertura** de `spec.md` tiene su test.

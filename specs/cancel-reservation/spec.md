# Spec: Cancelar Reserva

## Descripción

Un usuario cancela una reserva existente identificada por su `id`. La cancelación borra la reserva del sistema (hard delete). Solo se puede cancelar una reserva que aún no haya comenzado; reservas en curso o pasadas no se pueden cancelar.

## Endpoint

- **Método:** DELETE
- **Ruta:** `/api/reservations/{id}`
- **Body de entrada:** ninguno
- **Respuesta exitosa:** `204 No Content`

## Respuestas de error

| Situación | Status |
|-----------|--------|
| No existe una reserva con ese `id` | `404 Not Found` |
| La reserva ya comenzó (`StartTime <= ahora`) | `409 Conflict` |

## Scenarios

### Scenario 1: Cancelación válida de reserva futura (happy path)

- **GIVEN** existe una reserva con `Id = 1`
- **AND** su `StartTime` es posterior al instante actual
- **WHEN** se envía `DELETE /api/reservations/1`
- **THEN** la respuesta es `204 No Content`
- **AND** no existe ninguna reserva con `Id = 1` en el sistema

### Scenario 2: Reserva inexistente

- **GIVEN** no existe ninguna reserva con `Id = 999`
- **WHEN** se envía `DELETE /api/reservations/999`
- **THEN** la respuesta es `404 Not Found`
- **AND** el estado del sistema no cambia

### Scenario 3: Reserva ya comenzada

- **GIVEN** existe una reserva con `Id = 2`
- **AND** su `StartTime` es anterior o igual al instante actual
- **WHEN** se envía `DELETE /api/reservations/2`
- **THEN** la respuesta es `409 Conflict`
- **AND** la reserva con `Id = 2` sigue existiendo en el sistema

## Cobertura: scenario → test

| # | Scenario | Test (nombre esperado) |
|---|----------|------------------------|
| 1 | Cancelación válida | `Scenario_01_CancelReservation_WhenFuture_RemovesIt` |
| 2 | Reserva inexistente | `Scenario_02_CancelReservation_WhenNotFound_ReturnsNotFound` |
| 3 | Reserva ya comenzada | `Scenario_03_CancelReservation_WhenAlreadyStarted_ReturnsConflict` |

## Non-goals específicos de este feature

- No se valida quién cancela (cualquier cliente con el `id` puede cancelar). Autorización está fuera de scope.
- No hay deshacer: una vez cancelada, se perdió el registro.
- No hay notificación al usuario que reservó.

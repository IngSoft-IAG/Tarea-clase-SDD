# Spec: Crear Reserva

## Descripción

Un usuario registrado reserva una sala activa para un intervalo de tiempo futuro. El sistema rechaza la reserva si el intervalo se solapa con otra reserva existente de la misma sala, si la sala está inactiva, si los datos temporales son inválidos, o si la sala o el usuario no existen.

## Endpoint

- **Método:** POST
- **Ruta:** `/api/reservations`
- **Body de entrada:**
  ```
  {
    "roomId": entero,
    "userId": entero,
    "startTime": fecha-hora UTC ISO-8601,
    "endTime":   fecha-hora UTC ISO-8601
  }
  ```
- **Respuesta exitosa:** `201 Created` + body con la reserva creada (`id`, `roomId`, `userId`, `startTime`, `endTime`) y header `Location` apuntando al recurso.

## Respuestas de error

| Situación | Status |
|-----------|--------|
| Sala o usuario no existen | `404 Not Found` |
| Sala existe pero `IsActive = false` | `400 Bad Request` |
| `StartTime >= EndTime` | `400 Bad Request` |
| `StartTime` ya pasó (anterior o igual al instante actual) | `400 Bad Request` |
| Solapamiento con otra reserva de la misma sala | `409 Conflict` |
| Cualquier campo obligatorio ausente o con tipo incorrecto | `400 Bad Request` |

## Regla de solapamiento

Dos reservas de la misma sala `A` y `B` se consideran solapadas cuando `A.StartTime < B.EndTime` **y** `B.StartTime < A.EndTime`. Intervalos adyacentes (`A.EndTime == B.StartTime`) **no** se consideran solapados.

## Scenarios

### Scenario 1: Reserva válida en sala libre (happy path)

- **GIVEN** existe una sala activa con `RoomId = 1`
- **AND** existe un usuario con `UserId = 1`
- **AND** no hay reservas previas para esa sala en el intervalo solicitado
- **WHEN** se envía `POST /api/reservations` con `roomId = 1`, `userId = 1`, `startTime = mañana 10:00 UTC`, `endTime = mañana 11:00 UTC`
- **THEN** la respuesta es `201 Created`
- **AND** el body contiene la reserva con un `id` asignado
- **AND** existe exactamente una reserva persistida con esos datos

### Scenario 2: Solapamiento con reserva existente

- **GIVEN** existe una sala activa con `RoomId = 1`
- **AND** ya existe una reserva en esa sala de `mañana 10:00 UTC` a `mañana 11:00 UTC`
- **WHEN** se envía `POST /api/reservations` con `roomId = 1`, `userId = 2`, `startTime = mañana 10:30 UTC`, `endTime = mañana 11:30 UTC`
- **THEN** la respuesta es `409 Conflict`
- **AND** no se persiste una nueva reserva

### Scenario 3: Sala inactiva

- **GIVEN** existe una sala con `RoomId = 3` e `IsActive = false`
- **WHEN** se envía `POST /api/reservations` con `roomId = 3`, un `userId` válido y un intervalo válido en el futuro
- **THEN** la respuesta es `400 Bad Request`
- **AND** no se persiste ninguna reserva

### Scenario 4: Intervalo inválido (StartTime no anterior a EndTime)

- **GIVEN** existe una sala activa con `RoomId = 1` y un usuario válido
- **WHEN** se envía `POST /api/reservations` con `startTime = mañana 11:00 UTC` y `endTime = mañana 10:00 UTC`
- **THEN** la respuesta es `400 Bad Request`
- **AND** no se persiste ninguna reserva

### Scenario 5: StartTime en el pasado

- **GIVEN** existe una sala activa con `RoomId = 1` y un usuario válido
- **WHEN** se envía `POST /api/reservations` con `startTime = ayer 10:00 UTC` y `endTime = ayer 11:00 UTC`
- **THEN** la respuesta es `400 Bad Request`
- **AND** no se persiste ninguna reserva

### Scenario 6: Sala inexistente

- **GIVEN** no existe ninguna sala con `RoomId = 999`
- **WHEN** se envía `POST /api/reservations` con `roomId = 999` y el resto de los datos válidos
- **THEN** la respuesta es `404 Not Found`
- **AND** no se persiste ninguna reserva

### Scenario 7: Usuario inexistente

- **GIVEN** existe una sala activa con `RoomId = 1`
- **AND** no existe ningún usuario con `UserId = 999`
- **WHEN** se envía `POST /api/reservations` con `userId = 999` y el resto de los datos válidos
- **THEN** la respuesta es `404 Not Found`
- **AND** no se persiste ninguna reserva

## Cobertura: scenario → test

| # | Scenario | Test (nombre esperado) |
|---|----------|------------------------|
| 1 | Reserva válida en sala libre | `Scenario_01_CreateReservation_WithValidData_Persists` |
| 2 | Solapamiento | `Scenario_02_CreateReservation_WhenOverlapping_ReturnsConflict` |
| 3 | Sala inactiva | `Scenario_03_CreateReservation_WhenRoomInactive_ReturnsBadRequest` |
| 4 | Intervalo inválido | `Scenario_04_CreateReservation_WhenStartNotBeforeEnd_ReturnsBadRequest` |
| 5 | StartTime en el pasado | `Scenario_05_CreateReservation_WhenStartInPast_ReturnsBadRequest` |
| 6 | Sala inexistente | `Scenario_06_CreateReservation_WhenRoomDoesNotExist_ReturnsNotFound` |
| 7 | Usuario inexistente | `Scenario_07_CreateReservation_WhenUserDoesNotExist_ReturnsNotFound` |

## Non-goals específicos de este feature

- No hay límite de duración mínima/máxima de una reserva.
- No se valida que el usuario no tenga otra reserva simultánea en una sala distinta.
- No se considera capacidad de la sala (reserva individual, no por grupo).

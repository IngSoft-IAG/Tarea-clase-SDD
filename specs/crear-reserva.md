# Spec: Crear Reserva

## Descripcion
Como usuario, quiero crear una reserva para una sala en un rango horario determinado, para asegurar su disponibilidad en ese período.

## Non-goals
- No incluye cancelar reservas.
- No incluye consultar disponibilidad como endpoint separado.
- No incluye editar una reserva existente.
- No incluye autenticación ni autorización.
- No incluye validaciones de reglas avanzadas como horarios laborales, capacidad de la sala o duración máxima de la reserva.
- No incluye notificaciones al usuario luego de crear la reserva.

## Modelo de datos
- La entidad `Reservation` debería incluir, como mínimo, los siguientes campos:
- `Id`
- `UserId`
- `RoomId`
- `StartTime`
- `EndTime`
- `User`
- `Room`


## Scenarios

### Scenario 1: Crear una reserva correctamente
- GIVEN existe un usuario activo, existe una sala activa y el horario solicitado no tiene reservas solapadas
- WHEN el usuario envia una solicitud POST para crear la reserva con `userId`, `roomId`, `startTime` y `endTime`
- THEN la API responde `201 Created` y devuelve la reserva creada con sus datos

### Scenario 2: Rechazar una reserva por conflicto de horario
- GIVEN existe una reserva previa para la misma sala en un horario que se solapa con el solicitado
- WHEN se intenta crear una nueva reserva para esa misma sala en ese mismo rango horario
- THEN la API responde `409 Conflict` y no crea la reserva

### Scenario 3: Rechazar una reserva con rango horario invalido
- GIVEN existe el usuario y la sala, pero `startTime` es mayor o igual que `endTime`
- WHEN se envia la solicitud POST para crear la reserva
- THEN la API responde `400 Bad Request` indicando que el rango horario no es valido


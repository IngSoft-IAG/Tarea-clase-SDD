## What to build

Implementar `GET /api/rooms/{roomId}/availability` que devuelva `RoomAvailabilityDto` con `slots` presegmentados (por defecto 30 minutos). Incluir `SlotCalculator` puro y `AvailabilityService` que recupere reservas y llame al calculador.

## Acceptance criteria

- [ ] Devuelve `200` con `RoomAvailabilityDto` incluyendo `roomId`, `requestedStartUtc`, `requestedEndUtc`, `slotMinutes`, `requiredDurationMinutes` (si aplica), `isActive` y `slots: [{startUtc,endUtc}]`.
- [ ] Slots correctamente calculados para rangos sin reservas y con reservas.
- [ ] Timestamps serializados en ISO8601 UTC (`Z`).

## Blocked by

- Blocked by #21

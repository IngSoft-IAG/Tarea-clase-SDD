## What to build

Implementar validaciones en controlador/servicio: `startUtc < endUtc`, `maxRangeDays = 30`, `slotMinutes` y `requiredDurationMinutes` (min 5, max 1440). Devolver `400 BadRequest` con mensajes claros cuando falten o sean inválidos.

## Acceptance criteria

- [ ] `400` para `startUtc >= endUtc`.
- [ ] `400` si rango > 30 días.
- [ ] `400` si `slotMinutes` o `requiredDurationMinutes` fuera de [5,1440].

## Blocked by

- Blocked by #23

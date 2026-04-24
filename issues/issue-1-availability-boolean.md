## What to build

Crear un endpoint `GET /api/rooms/{roomId}/availability` que devuelva un booleano `available` indicando si la sala está libre en el rango solicitado y el flag `isActive`. Método minimal: consulta reservas que intersecten el rango; si no hay reservas, `available=true`.

## Acceptance criteria

- [ ] `GET /api/rooms/{roomId}/availability?startUtc=...&endUtc=...` devuelve `200` con `{ "available": true, "isActive": true }` cuando no hay solapamientos.
- [ ] Devuelve `200` con `{ "available": false, "isActive": true }` cuando existe solapamiento.
- [ ] Devuelve `404` si `roomId` no existe.
- [ ] Devuelve `200` con `{ "isActive": false, "message": "La sala no está activa" }` si la sala existe pero está inactiva.
- [ ] Devuelve `400` si parámetros inválidos (p.ej. `startUtc >= endUtc`).

## Blocked by

- None - can start immediately

# Plan técnico – Crear Reserva

## Archivos a crear

Ninguno nuevo en `Domain/`, `DTOs/`, `Services/`, `Controllers/` (todos los archivos ya existen como esqueletos).

## Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `RoomBookingApi/Domain/Reservation.cs` | Agregar `RoomId`, `UserId`, `StartTime`, `EndTime` |
| `RoomBookingApi/DTOs/ReservationDto.cs` | DTO de salida (response) |
| `RoomBookingApi/DTOs/CreateReservationDto.cs` *(nuevo)* | DTO de entrada (request body) |
| `RoomBookingApi/Services/ReservationService.cs` | Lógica de validación + persistencia + resultado tipado |
| `RoomBookingApi/Controllers/ReservationsController.cs` | Endpoint `POST` + mapeo resultado → status HTTP |
| `RoomBookingApi.Tests/ReservationServiceTests.cs` | Tests del service (uno por scenario) |

`AppDbContext` ya expone `DbSet<Reservation>`, no requiere cambios. `Program.cs` ya registra `ReservationService`, no requiere cambios.

## Contratos

### `Reservation` (entidad de dominio)

```csharp
public class Reservation
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
```

### `CreateReservationDto` (request)

```csharp
public class CreateReservationDto
{
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
```

### `ReservationDto` (response)

```csharp
public class ReservationDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
```

### Firma del service

Usamos un patrón de result discriminado con un enum para el outcome. Evita lanzar excepciones en flujos de validación esperados y mantiene el controller trivial.

```csharp
public enum CreateReservationStatus
{
    Success,
    RoomNotFound,
    UserNotFound,
    RoomInactive,
    InvalidTimeRange,   // StartTime >= EndTime
    StartInPast,
    Conflict
}

public sealed record CreateReservationResult(
    CreateReservationStatus Status,
    Reservation? Reservation);

public class ReservationService(AppDbContext dbContext)
{
    public async Task<CreateReservationResult> CreateAsync(
        int roomId, int userId, DateTime startTime, DateTime endTime);
}
```

### Mapeo status del service → HTTP en el controller

| `CreateReservationStatus` | HTTP |
|---------------------------|------|
| `Success` | `201 Created` + body + `Location` |
| `RoomNotFound` | `404 Not Found` (`"Room not found"`) |
| `UserNotFound` | `404 Not Found` (`"User not found"`) |
| `RoomInactive` | `400 Bad Request` (`"Room is inactive"`) |
| `InvalidTimeRange` | `400 Bad Request` (`"StartTime must be before EndTime"`) |
| `StartInPast` | `400 Bad Request` (`"StartTime must be in the future"`) |
| `Conflict` | `409 Conflict` (`"Time slot overlaps an existing reservation"`) |

## Orden de validación en el service

Importante para que los tests sean determinísticos. Cada fallo corta el flujo (short-circuit):

1. `StartTime < EndTime` → si no, `InvalidTimeRange`
2. `StartTime > DateTime.UtcNow` → si no, `StartInPast`
3. Existe `Room` con `roomId` → si no, `RoomNotFound`
4. `Room.IsActive == true` → si no, `RoomInactive`
5. Existe `User` con `userId` → si no, `UserNotFound`
6. No hay solapamiento en esa sala → si hay, `Conflict`
7. Crear y persistir → `Success`

## Query de solapamiento

Sobre `dbContext.Reservations`, filtrar por `RoomId == roomId` y detectar cualquier reserva `r` donde `r.StartTime < endTime && startTime < r.EndTime`.

## Pruebas

Cada test instancia un `AppDbContext` en memoria con un nombre único (`Guid.NewGuid().ToString()`) para aislar estado entre tests. Se siembra lo mínimo necesario para cada scenario (una o dos rooms, un user, quizás una reservation previa).

Helper propuesto:

```csharp
private static AppDbContext NewDb()
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new AppDbContext(options);
}
```

## Riesgos y decisiones

- **`DateTime.UtcNow` dentro del service.** Hace el test 5 (start in past) determinístico siempre que se use un `DateTime` suficientemente en el pasado (ej. `UtcNow.AddDays(-1)`). No introducimos un `IClock` porque es over-engineering para el scope.
- **InMemory provider de EF.** No enforza FKs. La validación "room/user existe" es explícita en el service, no delegada a la base.
- **`Kind` del `DateTime`.** No lo forzamos a `Utc` a nivel de entidad. Asumimos UTC por contrato de API (documentado en `spec.md` raíz).

# Plan técnico – Cancelar Reserva

## Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `RoomBookingApi/Services/ReservationService.cs` | Agregar método `CancelAsync` + enum de resultado |
| `RoomBookingApi/Controllers/ReservationsController.cs` | Agregar endpoint `DELETE {id}` |
| `RoomBookingApi.Tests/ReservationServiceTests.cs` | 3 tests (uno por scenario) |

**Dependencias:** este feature asume que la entidad `Reservation` ya tiene los campos del feature A (`StartTime` al menos). Si se implementa antes que Crear Reserva, igualmente hay que definir primero la entidad.

## Contratos

### Firma del service

Mismo patrón de result discriminado que en Crear Reserva.

```csharp
public enum CancelReservationStatus
{
    Success,
    NotFound,
    AlreadyStarted
}

public class ReservationService(AppDbContext dbContext)
{
    public async Task<CancelReservationStatus> CancelAsync(int id);
}
```

### Mapeo status → HTTP

| `CancelReservationStatus` | HTTP |
|---------------------------|------|
| `Success` | `204 No Content` |
| `NotFound` | `404 Not Found` |
| `AlreadyStarted` | `409 Conflict` (`"Reservation has already started and cannot be cancelled"`) |

## Orden de validación en el service

1. Buscar `Reservation` por `id` → si no existe, `NotFound`.
2. Verificar `reservation.StartTime > DateTime.UtcNow` → si no, `AlreadyStarted`.
3. Remover y persistir → `Success`.

## Pruebas

Reutilizar el helper `NewDb()` del feature A. Cada test siembra una sola reserva para mantener el scenario limpio.

### Helper adicional recomendado

```csharp
private static Reservation SeedReservation(AppDbContext db, DateTime start, DateTime end)
{
    db.Rooms.Add(new Room { Id = 1, Name = "Sala Test", Capacity = 4, IsActive = true });
    db.Users.Add(new User { Id = 1, Name = "User Test", Email = "test@test.com" });
    var r = new Reservation { RoomId = 1, UserId = 1, StartTime = start, EndTime = end };
    db.Reservations.Add(r);
    db.SaveChanges();
    return r;
}
```

## Riesgos y decisiones

- **Regla de "ya comenzada".** La spec compara `StartTime <= UtcNow`. Un test justo sobre el borde sería difícil de hacer determinístico; usamos márgenes cómodos (ej. `UtcNow.AddHours(-1)` para "ya comenzó", `UtcNow.AddHours(+1)` para "futuro").
- **Concurrencia.** No cubrimos carrera entre DELETE y otra operación. InMemory no da garantías fuertes y está fuera de scope.

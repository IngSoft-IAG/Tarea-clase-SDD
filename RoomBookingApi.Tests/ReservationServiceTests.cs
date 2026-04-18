using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;
using RoomBookingApi.Services;

namespace RoomBookingApi.Tests;

[TestClass]
public class ReservationServiceTests
{
    private const int ActiveRoomId = 1;
    private const int InactiveRoomId = 2;
    private const int UserId = 1;

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Rooms.Add(new Room { Id = ActiveRoomId, Name = "Sala Test", Capacity = 10, IsActive = true });
        context.Rooms.Add(new Room { Id = InactiveRoomId, Name = "Sala Inactiva", Capacity = 5, IsActive = false });
        context.Users.Add(new User { Id = UserId, Name = "Ana", Email = "ana@test.com" });
        context.SaveChanges();
        return context;
    }

    private static Reservation NewReservation(int hoursFromNow, int durationHours, int roomId = ActiveRoomId, int userId = UserId)
    {
        var start = DateTime.UtcNow.AddHours(hoursFromNow);
        return new Reservation
        {
            RoomId = roomId,
            UserId = userId,
            StartAt = start,
            EndAt = start.AddHours(durationHours)
        };
    }

    // Scenario A1: Happy path - crear reserva valida en sala activa
    [TestMethod]
    public async Task CreateAsync_ValidRequest_ReturnsReservationWithActiveStatus()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var input = NewReservation(hoursFromNow: 1, durationHours: 1);

        var (reservation, error) = await service.CreateAsync(input);

        Assert.IsNull(error);
        Assert.IsNotNull(reservation);
        Assert.AreEqual(ReservationStatus.Active, reservation!.Status);
        Assert.IsTrue(reservation.Id > 0);
        Assert.IsNull(reservation.CancelledAt);
        Assert.AreEqual(1, await db.Reservations.CountAsync());
    }

    // Scenario A2: Sala inactiva
    [TestMethod]
    public async Task CreateAsync_InactiveRoom_ReturnsErrorRoomNotActive()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var input = NewReservation(hoursFromNow: 1, durationHours: 1, roomId: InactiveRoomId);

        var (reservation, error) = await service.CreateAsync(input);

        Assert.IsNull(reservation);
        Assert.AreEqual("Room is not active.", error);
    }

    // Scenario A3: Overlap exacto con reserva activa
    [TestMethod]
    public async Task CreateAsync_ExactOverlap_ReturnsErrorAlreadyBooked()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var first = NewReservation(hoursFromNow: 1, durationHours: 1);
        await service.CreateAsync(first);

        var duplicate = new Reservation
        {
            RoomId = first.RoomId,
            UserId = first.UserId,
            StartAt = first.StartAt,
            EndAt = first.EndAt
        };
        var (reservation, error) = await service.CreateAsync(duplicate);

        Assert.IsNull(reservation);
        Assert.AreEqual("Room is already booked for the requested range.", error);
    }

    // Scenario A4: Overlap parcial
    [TestMethod]
    public async Task CreateAsync_PartialOverlap_ReturnsErrorAlreadyBooked()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (existing, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 2));

        var overlapping = new Reservation
        {
            RoomId = ActiveRoomId,
            UserId = UserId,
            StartAt = existing!.StartAt.AddHours(1),
            EndAt = existing.EndAt.AddHours(1)
        };
        var (reservation, error) = await service.CreateAsync(overlapping);

        Assert.IsNull(reservation);
        Assert.AreEqual("Room is already booked for the requested range.", error);
    }

    // Scenario A5: Back-to-back permitido (EndAt es exclusivo)
    [TestMethod]
    public async Task CreateAsync_BackToBackRanges_Succeeds()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (first, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));

        var next = new Reservation
        {
            RoomId = ActiveRoomId,
            UserId = UserId,
            StartAt = first!.EndAt,
            EndAt = first.EndAt.AddHours(1)
        };
        var (reservation, error) = await service.CreateAsync(next);

        Assert.IsNull(error);
        Assert.IsNotNull(reservation);
        Assert.AreEqual(2, await db.Reservations.CountAsync());
    }

    // Scenario A6: Fechas invertidas (EndAt <= StartAt)
    [TestMethod]
    public async Task CreateAsync_EndBeforeOrEqualStart_ReturnsValidationError()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var start = DateTime.UtcNow.AddHours(1);
        var input = new Reservation
        {
            RoomId = ActiveRoomId,
            UserId = UserId,
            StartAt = start,
            EndAt = start
        };

        var (reservation, error) = await service.CreateAsync(input);

        Assert.IsNull(reservation);
        Assert.AreEqual("EndAt must be after StartAt.", error);
    }

    // Scenario A7: StartAt en el pasado
    [TestMethod]
    public async Task CreateAsync_StartInPast_ReturnsValidationError()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var input = NewReservation(hoursFromNow: -2, durationHours: 1);

        var (reservation, error) = await service.CreateAsync(input);

        Assert.IsNull(reservation);
        Assert.AreEqual("StartAt cannot be in the past.", error);
    }

    // Scenario A8: Usuario inexistente
    [TestMethod]
    public async Task CreateAsync_UserDoesNotExist_ReturnsNotFoundError()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var input = NewReservation(hoursFromNow: 1, durationHours: 1, userId: 9999);

        var (reservation, error) = await service.CreateAsync(input);

        Assert.IsNull(reservation);
        Assert.AreEqual("User not found.", error);
    }

    // Scenario A9: Sala inexistente
    [TestMethod]
    public async Task CreateAsync_RoomDoesNotExist_ReturnsNotFoundError()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var input = NewReservation(hoursFromNow: 1, durationHours: 1, roomId: 9999);

        var (reservation, error) = await service.CreateAsync(input);

        Assert.IsNull(reservation);
        Assert.AreEqual("Room not found.", error);
    }

    // Scenario A10: Overlap con reserva cancelada permite crear
    [TestMethod]
    public async Task CreateAsync_OverlapWithCancelledReservation_Succeeds()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (first, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));
        await service.CancelAsync(first!.Id);

        var overlapping = new Reservation
        {
            RoomId = first.RoomId,
            UserId = first.UserId,
            StartAt = first.StartAt,
            EndAt = first.EndAt
        };
        var (reservation, error) = await service.CreateAsync(overlapping);

        Assert.IsNull(error);
        Assert.IsNotNull(reservation);
    }

    // Scenario B1: Cancelar reserva activa setea Status=Cancelled y CancelledAt
    [TestMethod]
    public async Task CancelAsync_ActiveReservation_SetsStatusCancelledAndCancelledAt()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (created, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));

        var (success, error) = await service.CancelAsync(created!.Id);

        Assert.IsTrue(success);
        Assert.IsNull(error);
        var persisted = await db.Reservations.FindAsync(created.Id);
        Assert.IsNotNull(persisted);
        Assert.AreEqual(ReservationStatus.Cancelled, persisted!.Status);
        Assert.IsNotNull(persisted.CancelledAt);
    }

    // Scenario B2: Cancelar reserva ya cancelada
    [TestMethod]
    public async Task CancelAsync_AlreadyCancelled_ReturnsErrorAlreadyCancelled()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (created, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));
        await service.CancelAsync(created!.Id);

        var (success, error) = await service.CancelAsync(created.Id);

        Assert.IsFalse(success);
        Assert.AreEqual("Reservation already cancelled.", error);
    }

    // Scenario B3: Cancelar id inexistente
    [TestMethod]
    public async Task CancelAsync_NonExistentId_ReturnsNotFoundError()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);

        var (success, error) = await service.CancelAsync(9999);

        Assert.IsFalse(success);
        Assert.AreEqual("Reservation not found.", error);
    }

    // Scenario C1: Rango libre
    [TestMethod]
    public async Task IsRoomAvailableAsync_FreeRange_ReturnsTrueWithZeroConflicts()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var start = DateTime.UtcNow.AddHours(1);

        var (isAvailable, count) = await service.IsRoomAvailableAsync(ActiveRoomId, start, start.AddHours(1));

        Assert.IsTrue(isAvailable);
        Assert.AreEqual(0, count);
    }

    // Scenario C2: Rango bloqueado por reserva activa
    [TestMethod]
    public async Task IsRoomAvailableAsync_RangeBlockedByActive_ReturnsFalseWithOneConflict()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (created, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));

        var (isAvailable, count) = await service.IsRoomAvailableAsync(ActiveRoomId, created!.StartAt, created.EndAt);

        Assert.IsFalse(isAvailable);
        Assert.AreEqual(1, count);
    }

    // Scenario C3: Solape parcial
    [TestMethod]
    public async Task IsRoomAvailableAsync_PartialOverlap_ReturnsFalse()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (created, _) = await service.CreateAsync(NewReservation(hoursFromNow: 2, durationHours: 2));

        var queryStart = created!.StartAt.AddHours(1);
        var (isAvailable, count) = await service.IsRoomAvailableAsync(ActiveRoomId, queryStart, queryStart.AddHours(2));

        Assert.IsFalse(isAvailable);
        Assert.AreEqual(1, count);
    }

    // Scenario C4: Solo reserva cancelada en el rango no bloquea
    [TestMethod]
    public async Task IsRoomAvailableAsync_OnlyCancelledInRange_ReturnsTrue()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (created, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));
        await service.CancelAsync(created!.Id);

        var (isAvailable, count) = await service.IsRoomAvailableAsync(ActiveRoomId, created.StartAt, created.EndAt);

        Assert.IsTrue(isAvailable);
        Assert.AreEqual(0, count);
    }

    // Scenario D1: Usuario con varias reservas - devuelve todas ordenadas por StartAt asc (incluye canceladas)
    [TestMethod]
    public async Task GetByUserAsync_UserWithMultipleReservations_ReturnsAllOrderedByStartAt()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        var (later, _) = await service.CreateAsync(NewReservation(hoursFromNow: 5, durationHours: 1));
        var (earlier, _) = await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));
        var (latest, _) = await service.CreateAsync(NewReservation(hoursFromNow: 10, durationHours: 1));
        await service.CancelAsync(latest!.Id);

        var result = await service.GetByUserAsync(UserId);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(earlier!.Id, result[0].Id);
        Assert.AreEqual(later!.Id, result[1].Id);
        Assert.AreEqual(latest.Id, result[2].Id);
        Assert.AreEqual(ReservationStatus.Cancelled, result[2].Status);
    }

    // Scenario D2: Usuario sin reservas devuelve lista vacia
    [TestMethod]
    public async Task GetByUserAsync_UserWithNoReservations_ReturnsEmptyList()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);

        var result = await service.GetByUserAsync(UserId);

        Assert.AreEqual(0, result.Count);
    }

    // Scenario D3: Usuario inexistente devuelve lista vacia
    [TestMethod]
    public async Task GetByUserAsync_NonExistentUserId_ReturnsEmptyList()
    {
        using var db = CreateDbContext();
        var service = new ReservationService(db);
        await service.CreateAsync(NewReservation(hoursFromNow: 1, durationHours: 1));

        var result = await service.GetByUserAsync(9999);

        Assert.AreEqual(0, result.Count);
    }
}

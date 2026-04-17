using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;
using RoomBookingApi.Services;

namespace RoomBookingApi.Tests;

[TestClass]
public class ReservationServiceTests
{
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Reservation SeedReservation(AppDbContext db, DateTime start, DateTime end)
    {
        db.Rooms.Add(new Room { Id = 1, Name = "Sala Test", Capacity = 4, IsActive = true });
        db.Users.Add(new User { Id = 1, Name = "User Test", Email = "test@test.com" });
        var r = new Reservation { RoomId = 1, UserId = 1, StartTime = start, EndTime = end };
        db.Reservations.Add(r);
        db.SaveChanges();
        return r;
    }

    // ── Crear Reserva ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Scenario_01_CreateReservation_WithValidData_Persists()
    {
        var db = NewDb();
        db.Rooms.Add(new Room { Id = 1, Name = "R1", Capacity = 4, IsActive = true });
        db.Users.Add(new User { Id = 1, Name = "U1", Email = "u1@test.com" });
        db.SaveChanges();

        var svc = new ReservationService(db);
        var start = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var end = start.AddHours(1);

        var result = await svc.CreateAsync(1, 1, start, end);

        Assert.AreEqual(CreateReservationStatus.Success, result.Status);
        Assert.IsNotNull(result.Reservation);
        Assert.IsTrue(result.Reservation.Id > 0);
        Assert.AreEqual(1, db.Reservations.Count());
    }

    [TestMethod]
    public async Task Scenario_02_CreateReservation_WhenOverlapping_ReturnsConflict()
    {
        var db = NewDb();
        db.Rooms.Add(new Room { Id = 1, Name = "R1", Capacity = 4, IsActive = true });
        db.Users.Add(new User { Id = 1, Name = "U1", Email = "u1@test.com" });
        db.Users.Add(new User { Id = 2, Name = "U2", Email = "u2@test.com" });
        var start = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        db.Reservations.Add(new Reservation { RoomId = 1, UserId = 1, StartTime = start, EndTime = start.AddHours(1) });
        db.SaveChanges();

        var svc = new ReservationService(db);
        var result = await svc.CreateAsync(1, 2, start.AddMinutes(30), start.AddHours(1).AddMinutes(30));

        Assert.AreEqual(CreateReservationStatus.Conflict, result.Status);
        Assert.AreEqual(1, db.Reservations.Count());
    }

    [TestMethod]
    public async Task Scenario_03_CreateReservation_WhenRoomInactive_ReturnsBadRequest()
    {
        var db = NewDb();
        db.Rooms.Add(new Room { Id = 3, Name = "R3", Capacity = 4, IsActive = false });
        db.Users.Add(new User { Id = 1, Name = "U1", Email = "u1@test.com" });
        db.SaveChanges();

        var svc = new ReservationService(db);
        var start = DateTime.UtcNow.AddDays(1);
        var result = await svc.CreateAsync(3, 1, start, start.AddHours(1));

        Assert.AreEqual(CreateReservationStatus.RoomInactive, result.Status);
        Assert.AreEqual(0, db.Reservations.Count());
    }

    [TestMethod]
    public async Task Scenario_04_CreateReservation_WhenStartNotBeforeEnd_ReturnsBadRequest()
    {
        var db = NewDb();
        db.Rooms.Add(new Room { Id = 1, Name = "R1", Capacity = 4, IsActive = true });
        db.Users.Add(new User { Id = 1, Name = "U1", Email = "u1@test.com" });
        db.SaveChanges();

        var svc = new ReservationService(db);
        var tomorrow = DateTime.UtcNow.AddDays(1).Date;
        var result = await svc.CreateAsync(1, 1, tomorrow.AddHours(11), tomorrow.AddHours(10));

        Assert.AreEqual(CreateReservationStatus.InvalidTimeRange, result.Status);
        Assert.AreEqual(0, db.Reservations.Count());
    }

    [TestMethod]
    public async Task Scenario_05_CreateReservation_WhenStartInPast_ReturnsBadRequest()
    {
        var db = NewDb();
        db.Rooms.Add(new Room { Id = 1, Name = "R1", Capacity = 4, IsActive = true });
        db.Users.Add(new User { Id = 1, Name = "U1", Email = "u1@test.com" });
        db.SaveChanges();

        var svc = new ReservationService(db);
        var yesterday = DateTime.UtcNow.AddDays(-1).Date.AddHours(10);
        var result = await svc.CreateAsync(1, 1, yesterday, yesterday.AddHours(1));

        Assert.AreEqual(CreateReservationStatus.StartInPast, result.Status);
        Assert.AreEqual(0, db.Reservations.Count());
    }

    [TestMethod]
    public async Task Scenario_06_CreateReservation_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        var db = NewDb();
        db.Users.Add(new User { Id = 1, Name = "U1", Email = "u1@test.com" });
        db.SaveChanges();

        var svc = new ReservationService(db);
        var start = DateTime.UtcNow.AddDays(1);
        var result = await svc.CreateAsync(999, 1, start, start.AddHours(1));

        Assert.AreEqual(CreateReservationStatus.RoomNotFound, result.Status);
        Assert.AreEqual(0, db.Reservations.Count());
    }

    [TestMethod]
    public async Task Scenario_07_CreateReservation_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var db = NewDb();
        db.Rooms.Add(new Room { Id = 1, Name = "R1", Capacity = 4, IsActive = true });
        db.SaveChanges();

        var svc = new ReservationService(db);
        var start = DateTime.UtcNow.AddDays(1);
        var result = await svc.CreateAsync(1, 999, start, start.AddHours(1));

        Assert.AreEqual(CreateReservationStatus.UserNotFound, result.Status);
        Assert.AreEqual(0, db.Reservations.Count());
    }

    // ── Cancelar Reserva ─────────────────────────────────────────────────────

    [TestMethod]
    public async Task Scenario_01_CancelReservation_WhenFuture_RemovesIt()
    {
        var db = NewDb();
        var start = DateTime.UtcNow.AddHours(2);
        var reservation = SeedReservation(db, start, start.AddHours(1));

        var svc = new ReservationService(db);
        var status = await svc.CancelAsync(reservation.Id);

        Assert.AreEqual(CancelReservationStatus.Success, status);
        Assert.AreEqual(0, db.Reservations.Count());
    }

    [TestMethod]
    public async Task Scenario_02_CancelReservation_WhenNotFound_ReturnsNotFound()
    {
        var db = NewDb();
        var svc = new ReservationService(db);

        var status = await svc.CancelAsync(999);

        Assert.AreEqual(CancelReservationStatus.NotFound, status);
    }

    [TestMethod]
    public async Task Scenario_03_CancelReservation_WhenAlreadyStarted_ReturnsConflict()
    {
        var db = NewDb();
        var start = DateTime.UtcNow.AddHours(-1);
        var reservation = SeedReservation(db, start, DateTime.UtcNow.AddHours(1));

        var svc = new ReservationService(db);
        var status = await svc.CancelAsync(reservation.Id);

        Assert.AreEqual(CancelReservationStatus.AlreadyStarted, status);
        Assert.AreEqual(1, db.Reservations.Count());
    }
}

using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;
using RoomBookingApi.Services;

namespace RoomBookingApi.Tests;

[TestClass]
public class ReservationServiceTests
{
    [TestMethod]
    public async Task CreateAsync_HappyPath_PersistsReservationAndReturnsSuccess()
    {
        await using var dbContext = CreateDbContext();
        var user = new User { Name = "Ana", Email = "ana@test.com" };
        var room = new Room { Name = "Sala A", Capacity = 8, IsActive = true };
        dbContext.Users.Add(user);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        var fixedNow = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        var service = new ReservationService(dbContext, () => fixedNow);
        var startUtc = new DateTime(2026, 4, 23, 9, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(user.Id, room.Id, startUtc, endUtc));

        Assert.AreEqual(ReservationCreateStatus.Success, result.Status);
        Assert.IsNotNull(result.Reservation);

        var created = result.Reservation!;
        Assert.IsTrue(created.Id > 0);
        Assert.AreEqual(user.Id, created.UserId);
        Assert.AreEqual(room.Id, created.RoomId);
        Assert.AreEqual(startUtc, created.StartUtc);
        Assert.AreEqual(endUtc, created.EndUtc);
        Assert.AreEqual(fixedNow, created.CreatedAtUtc);

        var persisted = await dbContext.Reservations.SingleAsync();
        Assert.AreEqual(created.Id, persisted.Id);
        Assert.AreEqual(created.CreatedAtUtc, persisted.CreatedAtUtc);
    }

    [TestMethod]
    public async Task CreateAsync_WhenTimestampsAreNotUtc_ReturnsInvalid()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReservationService(dbContext);

        var start = new DateTime(2026, 4, 23, 9, 0, 0, DateTimeKind.Unspecified);
        var end = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(1, 1, start, end));

        Assert.AreEqual(ReservationCreateStatus.Invalid, result.Status);
        Assert.AreEqual("StartUtc and EndUtc must be UTC.", result.Message);
        Assert.AreEqual(0, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WhenStartIsNotBeforeEnd_ReturnsInvalid()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReservationService(dbContext);
        var start = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(1, 1, start, end));

        Assert.AreEqual(ReservationCreateStatus.Invalid, result.Status);
        Assert.AreEqual("StartUtc must be earlier than EndUtc.", result.Message);
        Assert.AreEqual(0, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WhenDurationIsLessThan30Minutes_ReturnsInvalid()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReservationService(dbContext);
        var start = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 23, 10, 29, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(1, 1, start, end));

        Assert.AreEqual(ReservationCreateStatus.Invalid, result.Status);
        Assert.AreEqual("Reservation must be at least 30 minutes long.", result.Message);
        Assert.AreEqual(0, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var room = new Room { Name = "Sala A", Capacity = 8, IsActive = true };
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var start = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 23, 10, 30, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(999, room.Id, start, end));

        Assert.AreEqual(ReservationCreateStatus.NotFound, result.Status);
        Assert.AreEqual("User not found.", result.Message);
        Assert.AreEqual(0, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var user = new User { Name = "Ana", Email = "ana@test.com" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var start = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 23, 10, 30, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(user.Id, 999, start, end));

        Assert.AreEqual(ReservationCreateStatus.NotFound, result.Status);
        Assert.AreEqual("Room not found.", result.Message);
        Assert.AreEqual(0, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WhenRoomIsInactive_ReturnsInvalid()
    {
        await using var dbContext = CreateDbContext();
        var user = new User { Name = "Ana", Email = "ana@test.com" };
        var room = new Room { Name = "Sala Legacy", Capacity = 4, IsActive = false };
        dbContext.Users.Add(user);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var start = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 23, 10, 30, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(user.Id, room.Id, start, end));

        Assert.AreEqual(ReservationCreateStatus.Invalid, result.Status);
        Assert.AreEqual("Room is inactive.", result.Message);
        Assert.AreEqual(0, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WhenTimeRangeOverlaps_ReturnsConflict()
    {
        await using var dbContext = CreateDbContext();
        var user = new User { Name = "Ana", Email = "ana@test.com" };
        var room = new Room { Name = "Sala A", Capacity = 8, IsActive = true };
        dbContext.Users.Add(user);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        dbContext.Reservations.Add(new Reservation
        {
            UserId = user.Id,
            RoomId = room.Id,
            StartUtc = new DateTime(2026, 4, 23, 9, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc),
            CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var result = await service.CreateAsync(new ReservationCreateCommand(
            user.Id,
            room.Id,
            new DateTime(2026, 4, 23, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 23, 10, 30, 0, DateTimeKind.Utc)));

        Assert.AreEqual(ReservationCreateStatus.Conflict, result.Status);
        Assert.AreEqual("Room is already reserved for the selected time range.", result.Message);
        Assert.AreEqual(1, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WhenBackToBackWithExistingReservation_ReturnsSuccess()
    {
        await using var dbContext = CreateDbContext();
        var user = new User { Name = "Ana", Email = "ana@test.com" };
        var room = new Room { Name = "Sala A", Capacity = 8, IsActive = true };
        dbContext.Users.Add(user);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        dbContext.Reservations.Add(new Reservation
        {
            UserId = user.Id,
            RoomId = room.Id,
            StartUtc = new DateTime(2026, 4, 23, 9, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc),
            CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var result = await service.CreateAsync(new ReservationCreateCommand(
            user.Id,
            room.Id,
            new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 23, 10, 30, 0, DateTimeKind.Utc)));

        Assert.AreEqual(ReservationCreateStatus.Success, result.Status);
        Assert.IsNotNull(result.Reservation);
        Assert.AreEqual(2, await dbContext.Reservations.CountAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var dbName = $"reservation-service-{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }
}

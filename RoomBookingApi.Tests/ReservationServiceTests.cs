using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;
using RoomBookingApi.DTOs;
using RoomBookingApi.Services;

namespace RoomBookingApi.Tests;

[TestClass]
public class ReservationServiceTests
{
    [TestMethod]
    public async Task CreateReservationAsync_CreatesReservation_WhenDataIsValid()
    {
        await using var dbContext = BuildDbContext();
        dbContext.Users.Add(new User { Id = 1, Name = "Ana", Email = "ana@empresa.com" });
        dbContext.Rooms.Add(new Room { Id = 1, Name = "Sala Norte", Capacity = 8, IsActive = true });
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var request = new ReservationDto
        {
            UserId = 1,
            RoomId = 1,
            StartTime = new DateTime(2026, 4, 17, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 4, 17, 10, 0, 0, DateTimeKind.Utc)
        };

        var result = await service.CreateReservationAsync(request);

        Assert.AreEqual(CreateReservationStatus.Created, result.Status);
        Assert.IsNotNull(result.Reservation);
        Assert.AreEqual(1, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateReservationAsync_ReturnsOverlap_WhenRoomHasConflictingReservation()
    {
        await using var dbContext = BuildDbContext();
        dbContext.Users.Add(new User { Id = 1, Name = "Ana", Email = "ana@empresa.com" });
        dbContext.Rooms.Add(new Room { Id = 1, Name = "Sala Norte", Capacity = 8, IsActive = true });
        dbContext.Reservations.Add(new Reservation
        {
            UserId = 1,
            RoomId = 1,
            StartTime = new DateTime(2026, 4, 17, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 4, 17, 10, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var request = new ReservationDto
        {
            UserId = 1,
            RoomId = 1,
            StartTime = new DateTime(2026, 4, 17, 9, 30, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 4, 17, 10, 30, 0, DateTimeKind.Utc)
        };

        var result = await service.CreateReservationAsync(request);

        Assert.AreEqual(CreateReservationStatus.Overlap, result.Status);
        Assert.AreEqual(1, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateReservationAsync_ReturnsInvalidTimeRange_WhenStartIsAfterOrEqualEnd()
    {
        await using var dbContext = BuildDbContext();
        dbContext.Users.Add(new User { Id = 1, Name = "Ana", Email = "ana@empresa.com" });
        dbContext.Rooms.Add(new Room { Id = 1, Name = "Sala Norte", Capacity = 8, IsActive = true });
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var request = new ReservationDto
        {
            UserId = 1,
            RoomId = 1,
            StartTime = new DateTime(2026, 4, 17, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 4, 17, 10, 0, 0, DateTimeKind.Utc)
        };

        var result = await service.CreateReservationAsync(request);

        Assert.AreEqual(CreateReservationStatus.InvalidTimeRange, result.Status);
        Assert.AreEqual(0, await dbContext.Reservations.CountAsync());
    }

    private static AppDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"RoomBookingTestDb-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}

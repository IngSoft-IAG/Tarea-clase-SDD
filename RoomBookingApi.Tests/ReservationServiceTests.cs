using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;
using RoomBookingApi.Services;

namespace RoomBookingApi.Tests;

[TestClass]
public class ReservationServiceTests
{
    [TestMethod]
    public async Task CreateAsync_WithValidData_CreatesReservation()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReservationService(dbContext);
        var reservation = new Reservation
        {
            RoomId = 1,
            UserId = 1,
            StartTime = new DateTime(2026, 4, 17, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 4, 17, 10, 0, 0, DateTimeKind.Utc)
        };

        var created = await service.CreateAsync(reservation);

        Assert.IsNotNull(created);
        Assert.AreNotEqual(0, created.Id);
        Assert.AreEqual(1, await dbContext.Reservations.CountAsync());
    }

    [TestMethod]
    public async Task CreateAsync_WithOverlappingRoomReservation_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Reservations.Add(new Reservation
        {
            RoomId = 1,
            UserId = 1,
            StartTime = new DateTime(2026, 4, 17, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 4, 17, 10, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new ReservationService(dbContext);
        var overlappingReservation = new Reservation
        {
            RoomId = 1,
            UserId = 2,
            StartTime = new DateTime(2026, 4, 17, 9, 30, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 4, 17, 10, 30, 0, DateTimeKind.Utc)
        };

        var created = await service.CreateAsync(overlappingReservation);

        Assert.IsNull(created);
        Assert.AreEqual(1, await dbContext.Reservations.CountAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(options);
        AppDbSeeder.Seed(dbContext);
        return dbContext;
    }
}

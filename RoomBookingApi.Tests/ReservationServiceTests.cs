using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Services;

namespace RoomBookingApi.Tests;

[TestClass]
public class ReservationServiceTests
{
    [TestMethod]
    public async Task CreateAsync_HappyPath_PersistsReservationAndReturnsSuccess()
    {
        var dbName = $"reservation-service-{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        await using var dbContext = new AppDbContext(options);

        var fixedNow = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        var service = new ReservationService(dbContext, () => fixedNow);
        var startUtc = new DateTime(2026, 4, 23, 9, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);

        var result = await service.CreateAsync(new ReservationCreateCommand(1, 2, startUtc, endUtc));

        Assert.AreEqual(ReservationCreateStatus.Success, result.Status);
        Assert.IsNotNull(result.Reservation);

        var created = result.Reservation!;
        Assert.IsTrue(created.Id > 0);
        Assert.AreEqual(1, created.UserId);
        Assert.AreEqual(2, created.RoomId);
        Assert.AreEqual(startUtc, created.StartUtc);
        Assert.AreEqual(endUtc, created.EndUtc);
        Assert.AreEqual(fixedNow, created.CreatedAtUtc);

        var persisted = await dbContext.Reservations.SingleAsync();
        Assert.AreEqual(created.Id, persisted.Id);
        Assert.AreEqual(created.CreatedAtUtc, persisted.CreatedAtUtc);
    }
}

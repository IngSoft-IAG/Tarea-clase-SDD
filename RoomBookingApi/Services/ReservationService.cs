using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;

namespace RoomBookingApi.Services;

public enum CreateReservationStatus
{
    Success,
    RoomNotFound,
    UserNotFound,
    RoomInactive,
    InvalidTimeRange,
    StartInPast,
    Conflict
}

public sealed record CreateReservationResult(CreateReservationStatus Status, Reservation? Reservation);

public enum CancelReservationStatus
{
    Success,
    NotFound,
    AlreadyStarted
}

public class ReservationService(AppDbContext dbContext)
{
    public async Task<CreateReservationResult> CreateAsync(
        int roomId, int userId, DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            return new CreateReservationResult(CreateReservationStatus.InvalidTimeRange, null);

        if (startTime <= DateTime.UtcNow)
            return new CreateReservationResult(CreateReservationStatus.StartInPast, null);

        var room = await dbContext.Rooms.FindAsync(roomId);
        if (room is null)
            return new CreateReservationResult(CreateReservationStatus.RoomNotFound, null);

        if (!room.IsActive)
            return new CreateReservationResult(CreateReservationStatus.RoomInactive, null);

        var user = await dbContext.Users.FindAsync(userId);
        if (user is null)
            return new CreateReservationResult(CreateReservationStatus.UserNotFound, null);

        var hasOverlap = await dbContext.Reservations
            .AnyAsync(r => r.RoomId == roomId && r.StartTime < endTime && startTime < r.EndTime);
        if (hasOverlap)
            return new CreateReservationResult(CreateReservationStatus.Conflict, null);

        var reservation = new Reservation
        {
            RoomId = roomId,
            UserId = userId,
            StartTime = startTime,
            EndTime = endTime
        };
        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        return new CreateReservationResult(CreateReservationStatus.Success, reservation);
    }

    public async Task<CancelReservationStatus> CancelAsync(int id)
    {
        var reservation = await dbContext.Reservations.FindAsync(id);
        if (reservation is null)
            return CancelReservationStatus.NotFound;

        if (reservation.StartTime <= DateTime.UtcNow)
            return CancelReservationStatus.AlreadyStarted;

        dbContext.Reservations.Remove(reservation);
        await dbContext.SaveChangesAsync();

        return CancelReservationStatus.Success;
    }
}

using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;

namespace RoomBookingApi.Services;

public enum ReservationCreationError
{
    InvalidTimeRange,
    UserNotFound,
    RoomNotFoundOrInactive,
    RoomAlreadyReserved
}

public record ReservationCreationResult(Reservation? Reservation, ReservationCreationError? Error)
{
    public static ReservationCreationResult Success(Reservation reservation) => new(reservation, null);
    public static ReservationCreationResult Failure(ReservationCreationError error) => new(null, error);
}

public class ReservationService(AppDbContext dbContext)
{
    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await dbContext.Reservations.FindAsync(id);
    }

    public async Task<ReservationCreationResult> CreateAsync(Reservation reservation)
    {
        if (reservation.EndTime <= reservation.StartTime)
        {
            return ReservationCreationResult.Failure(ReservationCreationError.InvalidTimeRange);
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == reservation.UserId);
        if (!userExists)
        {
            return ReservationCreationResult.Failure(ReservationCreationError.UserNotFound);
        }

        var room = await dbContext.Rooms.FindAsync(reservation.RoomId);
        if (room is null || !room.IsActive)
        {
            return ReservationCreationResult.Failure(ReservationCreationError.RoomNotFoundOrInactive);
        }

        var roomHasOverlap = await dbContext.Reservations.AnyAsync(r =>
            r.RoomId == reservation.RoomId &&
            r.StartTime < reservation.EndTime &&
            reservation.StartTime < r.EndTime);

        if (roomHasOverlap)
        {
            return ReservationCreationResult.Failure(ReservationCreationError.RoomAlreadyReserved);
        }

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();
        return ReservationCreationResult.Success(reservation);
    }
}

using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;

namespace RoomBookingApi.Services;

public class ReservationService(AppDbContext dbContext)
{
    public async Task<List<Reservation>> GetAllAsync()
    {
        return await dbContext.Reservations
            .OrderBy(r => r.StartAt)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await dbContext.Reservations.FindAsync(id);
    }

    public async Task<List<Reservation>> GetByUserAsync(int userId)
    {
        return await dbContext.Reservations
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.StartAt)
            .ToListAsync();
    }

    public async Task<(Reservation? Reservation, string? Error)> CreateAsync(Reservation reservation)
    {
        var startUtc = NormalizeToUtc(reservation.StartAt);
        var endUtc = NormalizeToUtc(reservation.EndAt);

        if (endUtc <= startUtc)
        {
            return (null, "EndAt must be after StartAt.");
        }

        if (startUtc < DateTime.UtcNow)
        {
            return (null, "StartAt cannot be in the past.");
        }

        var user = await dbContext.Users.FindAsync(reservation.UserId);
        if (user is null)
        {
            return (null, "User not found.");
        }

        var room = await dbContext.Rooms.FindAsync(reservation.RoomId);
        if (room is null)
        {
            return (null, "Room not found.");
        }

        if (!room.IsActive)
        {
            return (null, "Room is not active.");
        }

        var hasOverlap = await dbContext.Reservations
            .Where(r => r.RoomId == reservation.RoomId && r.Status == ReservationStatus.Active)
            .AnyAsync(r => r.StartAt < endUtc && r.EndAt > startUtc);

        if (hasOverlap)
        {
            return (null, "Room is already booked for the requested range.");
        }

        reservation.StartAt = startUtc;
        reservation.EndAt = endUtc;
        reservation.Status = ReservationStatus.Active;
        reservation.CreatedAt = DateTime.UtcNow;
        reservation.CancelledAt = null;

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        return (reservation, null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id)
    {
        var reservation = await dbContext.Reservations.FindAsync(id);
        if (reservation is null)
        {
            return (false, "Reservation not found.");
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            return (false, "Reservation already cancelled.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool IsAvailable, int ConflictingCount)> IsRoomAvailableAsync(int roomId, DateTime startAt, DateTime endAt)
    {
        var startUtc = NormalizeToUtc(startAt);
        var endUtc = NormalizeToUtc(endAt);

        var conflicting = await dbContext.Reservations
            .Where(r => r.RoomId == roomId
                        && r.Status == ReservationStatus.Active
                        && r.StartAt < endUtc
                        && r.EndAt > startUtc)
            .CountAsync();

        return (conflicting == 0, conflicting);
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}

using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;

namespace RoomBookingApi.Services;

public class ReservationService(AppDbContext dbContext)
{
    public async Task<Reservation?> CreateAsync(Reservation reservation)
    {
        if (reservation.EndTime <= reservation.StartTime)
        {
            return null;
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == reservation.UserId);
        if (!userExists)
        {
            return null;
        }

        var room = await dbContext.Rooms.FindAsync(reservation.RoomId);
        if (room is null || !room.IsActive)
        {
            return null;
        }

        var roomHasOverlap = await dbContext.Reservations.AnyAsync(r =>
            r.RoomId == reservation.RoomId &&
            r.StartTime < reservation.EndTime &&
            reservation.StartTime < r.EndTime);

        if (roomHasOverlap)
        {
            return null;
        }

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();
        return reservation;
    }
}

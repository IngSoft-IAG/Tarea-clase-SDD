using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;
using RoomBookingApi.DTOs;

namespace RoomBookingApi.Services;

public enum CreateReservationStatus
{
    Created,
    InvalidTimeRange,
    Overlap,
    UserNotFound,
    RoomNotFound,
    RoomInactive
}

public class CreateReservationResult
{
    public CreateReservationStatus Status { get; set; }
    public Reservation? Reservation { get; set; }
}

public class ReservationService(AppDbContext dbContext)
{
    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await dbContext.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<CreateReservationResult> CreateReservationAsync(ReservationDto request)
    {
        if (request.StartTime >= request.EndTime)
        {
            return new CreateReservationResult { Status = CreateReservationStatus.InvalidTimeRange };
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
        {
            return new CreateReservationResult { Status = CreateReservationStatus.UserNotFound };
        }

        var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId);
        if (room is null)
        {
            return new CreateReservationResult { Status = CreateReservationStatus.RoomNotFound };
        }

        if (!room.IsActive)
        {
            return new CreateReservationResult { Status = CreateReservationStatus.RoomInactive };
        }

        var hasOverlap = await dbContext.Reservations.AnyAsync(r =>
            r.RoomId == request.RoomId &&
            request.StartTime < r.EndTime &&
            request.EndTime > r.StartTime);

        if (hasOverlap)
        {
            return new CreateReservationResult { Status = CreateReservationStatus.Overlap };
        }

        var reservation = new Reservation
        {
            UserId = request.UserId,
            RoomId = request.RoomId,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        return new CreateReservationResult
        {
            Status = CreateReservationStatus.Created,
            Reservation = reservation
        };
    }
}

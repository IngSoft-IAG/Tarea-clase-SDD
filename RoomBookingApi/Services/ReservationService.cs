using RoomBookingApi.Data;
using RoomBookingApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace RoomBookingApi.Services;

public interface IReservationService
{
    Task<ReservationCreateResult> CreateAsync(ReservationCreateCommand command);
    Task<Reservation?> GetByIdAsync(int id);
}

public sealed class ReservationService(AppDbContext dbContext, Func<DateTime>? utcNow = null) : IReservationService
{
    private const int MinimumReservationMinutes = 30;
    private const string UtcValidationMessage = "StartUtc and EndUtc must be UTC.";
    private const string StartBeforeEndValidationMessage = "StartUtc must be earlier than EndUtc.";
    private const string MinimumDurationValidationMessage = "Reservation must be at least 30 minutes long.";
    private const string UserNotFoundMessage = "User not found.";
    private const string RoomNotFoundMessage = "Room not found.";
    private const string RoomInactiveMessage = "Room is inactive.";
    private const string RoomConflictMessage = "Room is already reserved for the selected time range.";

    public async Task<ReservationCreateResult> CreateAsync(ReservationCreateCommand command)
    {
        if (command.StartUtc.Kind != DateTimeKind.Utc || command.EndUtc.Kind != DateTimeKind.Utc)
        {
            return ReservationCreateResult.Invalid(UtcValidationMessage);
        }

        if (command.StartUtc >= command.EndUtc)
        {
            return ReservationCreateResult.Invalid(StartBeforeEndValidationMessage);
        }

        if ((command.EndUtc - command.StartUtc) < TimeSpan.FromMinutes(MinimumReservationMinutes))
        {
            return ReservationCreateResult.Invalid(MinimumDurationValidationMessage);
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == command.UserId);
        if (!userExists)
        {
            return ReservationCreateResult.NotFound(UserNotFoundMessage);
        }

        var room = await dbContext.Rooms.FindAsync(command.RoomId);
        if (room is null)
        {
            return ReservationCreateResult.NotFound(RoomNotFoundMessage);
        }

        if (!room.IsActive)
        {
            return ReservationCreateResult.Invalid(RoomInactiveMessage);
        }

        var hasConflict = await dbContext.Reservations.AnyAsync(r =>
            r.RoomId == command.RoomId &&
            r.StartUtc < command.EndUtc &&
            r.EndUtc > command.StartUtc);

        if (hasConflict)
        {
            return ReservationCreateResult.Conflict(RoomConflictMessage);
        }

        var reservation = new Reservation
        {
            UserId = command.UserId,
            RoomId = command.RoomId,
            StartUtc = command.StartUtc,
            EndUtc = command.EndUtc,
            CreatedAtUtc = (utcNow ?? (() => DateTime.UtcNow))()
        };

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        return ReservationCreateResult.Success(reservation);
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await dbContext.Reservations.FindAsync(id);
    }
}

public sealed record ReservationCreateCommand(int UserId, int RoomId, DateTime StartUtc, DateTime EndUtc);

public enum ReservationCreateStatus
{
    Success,
    Invalid,
    NotFound,
    Conflict
}

public sealed class ReservationCreateResult
{
    public ReservationCreateStatus Status { get; }
    public Reservation? Reservation { get; }
    public string? Message { get; }

    private ReservationCreateResult(ReservationCreateStatus status, Reservation? reservation, string? message)
    {
        Status = status;
        Reservation = reservation;
        Message = message;
    }

    public static ReservationCreateResult Success(Reservation reservation)
    {
        return new ReservationCreateResult(ReservationCreateStatus.Success, reservation, null);
    }

    public static ReservationCreateResult Invalid(string message)
    {
        return new ReservationCreateResult(ReservationCreateStatus.Invalid, null, message);
    }

    public static ReservationCreateResult NotFound(string message)
    {
        return new ReservationCreateResult(ReservationCreateStatus.NotFound, null, message);
    }

    public static ReservationCreateResult Conflict(string message)
    {
        return new ReservationCreateResult(ReservationCreateStatus.Conflict, null, message);
    }
}

using RoomBookingApi.Data;
using RoomBookingApi.Domain;

namespace RoomBookingApi.Services;

public interface IReservationService
{
    Task<ReservationCreateResult> CreateAsync(ReservationCreateCommand command);
    Task<Reservation?> GetByIdAsync(int id);
}

public sealed class ReservationService(AppDbContext dbContext, Func<DateTime>? utcNow = null) : IReservationService
{
    public async Task<ReservationCreateResult> CreateAsync(ReservationCreateCommand command)
    {
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

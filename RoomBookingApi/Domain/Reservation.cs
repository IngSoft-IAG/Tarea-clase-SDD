namespace RoomBookingApi.Domain;

public enum ReservationStatus
{
    Active = 0,
    Cancelled = 1
}

public class Reservation
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
}

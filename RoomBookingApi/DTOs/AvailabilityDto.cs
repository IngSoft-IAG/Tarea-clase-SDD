namespace RoomBookingApi.DTOs;

public class AvailabilityDto
{
    public int RoomId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsAvailable { get; set; }
    public int ConflictingReservationsCount { get; set; }
}

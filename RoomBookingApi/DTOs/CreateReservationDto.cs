namespace RoomBookingApi.DTOs;

public class CreateReservationDto
{
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}

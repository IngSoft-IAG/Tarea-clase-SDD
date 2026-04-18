namespace RoomBookingApi.DTOs;

public class CreateReservationDto
{
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}

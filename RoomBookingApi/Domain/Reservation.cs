namespace RoomBookingApi.Domain;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public User? User { get; set; }
    public Room? Room { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using RoomBookingApi.Domain;
using RoomBookingApi.DTOs;
using RoomBookingApi.Services;

namespace RoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(ReservationService reservationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => NotFound();

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationDto dto)
    {
        var result = await reservationService.CreateAsync(
            dto.RoomId, dto.UserId, dto.StartTime, dto.EndTime);

        return result.Status switch
        {
            CreateReservationStatus.Success =>
                CreatedAtAction(nameof(GetById), new { id = result.Reservation!.Id }, MapToDto(result.Reservation)),
            CreateReservationStatus.RoomNotFound => NotFound("Room not found"),
            CreateReservationStatus.UserNotFound => NotFound("User not found"),
            CreateReservationStatus.RoomInactive => BadRequest("Room is inactive"),
            CreateReservationStatus.InvalidTimeRange => BadRequest("StartTime must be before EndTime"),
            CreateReservationStatus.StartInPast => BadRequest("StartTime must be in the future"),
            CreateReservationStatus.Conflict => Conflict("Time slot overlaps an existing reservation"),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var status = await reservationService.CancelAsync(id);

        return status switch
        {
            CancelReservationStatus.Success => NoContent(),
            CancelReservationStatus.NotFound => NotFound(),
            CancelReservationStatus.AlreadyStarted => Conflict("Reservation has already started and cannot be cancelled"),
            _ => StatusCode(500)
        };
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            RoomId = reservation.RoomId,
            UserId = reservation.UserId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        };
    }
}

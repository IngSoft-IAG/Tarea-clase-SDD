using Microsoft.AspNetCore.Mvc;
using RoomBookingApi.Domain;
using RoomBookingApi.DTOs;
using RoomBookingApi.Services;

namespace RoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(ReservationService reservationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(ReservationDto dto)
    {
        var reservation = new Reservation
        {
            RoomId = dto.RoomId,
            UserId = dto.UserId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        var created = await reservationService.CreateAsync(reservation);
        if (created is null)
        {
            return BadRequest("Invalid reservation data.");
        }

        var result = MapToDto(created);
        return StatusCode(StatusCodes.Status201Created, result);
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

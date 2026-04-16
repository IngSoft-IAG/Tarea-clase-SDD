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
    public async Task<ActionResult<ReservationDto>> GetById(int id)
    {
        var reservation = await reservationService.GetByIdAsync(id);
        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(reservation));
    }

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

        var creationResult = await reservationService.CreateAsync(reservation);
        if (creationResult.Error is not null)
        {
            return creationResult.Error switch
            {
                ReservationCreationError.InvalidTimeRange => BadRequest("EndTime must be greater than StartTime."),
                ReservationCreationError.UserNotFound => NotFound("User not found."),
                ReservationCreationError.RoomNotFoundOrInactive => NotFound("Room not found or inactive."),
                ReservationCreationError.RoomAlreadyReserved => Conflict("Room already reserved for the selected time range."),
                _ => BadRequest("Invalid reservation data.")
            };
        }

        var result = MapToDto(creationResult.Reservation!);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

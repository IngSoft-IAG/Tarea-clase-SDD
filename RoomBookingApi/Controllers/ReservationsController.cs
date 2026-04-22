using Microsoft.AspNetCore.Mvc;
using RoomBookingApi.DTOs;
using RoomBookingApi.Services;

namespace RoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(IReservationService reservationService) : ControllerBase
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
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationDto dto)
    {
        var command = new ReservationCreateCommand(dto.UserId, dto.RoomId, dto.StartUtc, dto.EndUtc);
        var result = await reservationService.CreateAsync(command);

        if (result.Status == ReservationCreateStatus.Success && result.Reservation is not null)
        {
            var created = MapToDto(result.Reservation);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        return result.Status switch
        {
            ReservationCreateStatus.Invalid => BadRequest(result.Message ?? "Invalid reservation request."),
            ReservationCreateStatus.NotFound => NotFound(result.Message),
            ReservationCreateStatus.Conflict => Conflict(result.Message),
            _ => BadRequest("Unable to create reservation.")
        };
    }

    private static ReservationDto MapToDto(Domain.Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            RoomId = reservation.RoomId,
            StartUtc = reservation.StartUtc,
            EndUtc = reservation.EndUtc,
            CreatedAtUtc = reservation.CreatedAtUtc
        };
    }
}

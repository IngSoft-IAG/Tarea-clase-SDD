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
        var result = await reservationService.CreateReservationAsync(dto);

        if (result.Status == CreateReservationStatus.InvalidTimeRange)
        {
            return BadRequest("startTime must be earlier than endTime.");
        }

        if (result.Status == CreateReservationStatus.Overlap)
        {
            return Conflict("The room already has a reservation in the requested time range.");
        }

        if (result.Status == CreateReservationStatus.UserNotFound)
        {
            return BadRequest("User does not exist.");
        }

        if (result.Status == CreateReservationStatus.RoomNotFound)
        {
            return BadRequest("Room does not exist.");
        }

        if (result.Status == CreateReservationStatus.RoomInactive)
        {
            return BadRequest("Room is inactive.");
        }

        var reservation = result.Reservation!;
        var response = MapToDto(reservation);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            RoomId = reservation.RoomId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        };
    }
}

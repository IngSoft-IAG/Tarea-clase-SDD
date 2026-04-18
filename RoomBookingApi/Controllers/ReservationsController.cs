using Microsoft.AspNetCore.Mvc;
using RoomBookingApi.Domain;
using RoomBookingApi.DTOs;
using RoomBookingApi.Services;

namespace RoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(ReservationService reservationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAll()
    {
        var reservations = await reservationService.GetAllAsync();
        return Ok(reservations.Select(MapToDto));
    }

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
        if (dto.RoomId <= 0 || dto.UserId <= 0)
        {
            return BadRequest("RoomId and UserId are required and must be greater than 0.");
        }

        if (dto.StartAt == default || dto.EndAt == default)
        {
            return BadRequest("StartAt and EndAt are required.");
        }

        var reservation = new Reservation
        {
            RoomId = dto.RoomId,
            UserId = dto.UserId,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt
        };

        var (created, error) = await reservationService.CreateAsync(reservation);
        if (error is not null)
        {
            return MapErrorToResult(error);
        }

        var result = MapToDto(created!);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await reservationService.CancelAsync(id);
        if (success)
        {
            return NoContent();
        }

        return MapErrorToResult(error!);
    }

    [HttpGet("availability")]
    public async Task<ActionResult<AvailabilityDto>> GetAvailability(
        [FromQuery] int roomId,
        [FromQuery] DateTime startAt,
        [FromQuery] DateTime endAt)
    {
        if (roomId <= 0)
        {
            return BadRequest("RoomId is required and must be greater than 0.");
        }

        if (startAt == default || endAt == default)
        {
            return BadRequest("StartAt and EndAt are required.");
        }

        if (endAt <= startAt)
        {
            return BadRequest("EndAt must be after StartAt.");
        }

        var (isAvailable, conflictingCount) =
            await reservationService.IsRoomAvailableAsync(roomId, startAt, endAt);

        return Ok(new AvailabilityDto
        {
            RoomId = roomId,
            StartAt = startAt,
            EndAt = endAt,
            IsAvailable = isAvailable,
            ConflictingReservationsCount = conflictingCount
        });
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetByUser(int userId)
    {
        var reservations = await reservationService.GetByUserAsync(userId);
        return Ok(reservations.Select(MapToDto));
    }

    private ActionResult MapErrorToResult(string error)
    {
        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(error);
        }

        if (error.Contains("already", StringComparison.OrdinalIgnoreCase)
            || error.Contains("booked", StringComparison.OrdinalIgnoreCase)
            || error.Contains("not active", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(error);
        }

        return BadRequest(error);
    }

    private static ReservationDto MapToDto(Reservation r)
    {
        return new ReservationDto
        {
            Id = r.Id,
            RoomId = r.RoomId,
            UserId = r.UserId,
            StartAt = r.StartAt,
            EndAt = r.EndAt,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt,
            CancelledAt = r.CancelledAt
        };
    }
}

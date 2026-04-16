using Microsoft.AspNetCore.Mvc;
using RoomBookingApi.Domain;
using RoomBookingApi.DTOs;
using RoomBookingApi.Services;

namespace RoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController(RoomService roomService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll()
    {
        var rooms = await roomService.GetAllAsync();
        return Ok(rooms.Select(MapToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int id)
    {
        var room = await roomService.GetByIdAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(room));
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Capacity <= 0)
        {
            return BadRequest("Name is required and capacity must be greater than 0.");
        }

        var room = new Room
        {
            Name = dto.Name.Trim(),
            Capacity = dto.Capacity,
            IsActive = dto.IsActive
        };

        var created = await roomService.CreateAsync(room);
        var result = MapToDto(created);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoomDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Capacity <= 0)
        {
            return BadRequest("Name is required and capacity must be greater than 0.");
        }

        var room = new Room
        {
            Name = dto.Name.Trim(),
            Capacity = dto.Capacity,
            IsActive = dto.IsActive
        };

        var updated = await roomService.UpdateAsync(id, room);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await roomService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            IsActive = room.IsActive
        };
    }
}

using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;

namespace RoomBookingApi.Services;

public class RoomService(AppDbContext dbContext)
{
    public async Task<List<Room>> GetAllAsync()
    {
        return await dbContext.Rooms.OrderBy(r => r.Id).ToListAsync();
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await dbContext.Rooms.FindAsync(id);
    }

    public async Task<Room> CreateAsync(Room room)
    {
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();
        return room;
    }

    public async Task<bool> UpdateAsync(int id, Room updatedRoom)
    {
        var existingRoom = await dbContext.Rooms.FindAsync(id);
        if (existingRoom is null)
        {
            return false;
        }

        existingRoom.Name = updatedRoom.Name;
        existingRoom.Capacity = updatedRoom.Capacity;
        existingRoom.IsActive = updatedRoom.IsActive;
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existingRoom = await dbContext.Rooms.FindAsync(id);
        if (existingRoom is null)
        {
            return false;
        }

        dbContext.Rooms.Remove(existingRoom);
        await dbContext.SaveChangesAsync();

        return true;
    }
}

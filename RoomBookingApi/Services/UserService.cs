using Microsoft.EntityFrameworkCore;
using RoomBookingApi.Data;
using RoomBookingApi.Domain;

namespace RoomBookingApi.Services;

public class UserService(AppDbContext dbContext)
{
    public async Task<List<User>> GetAllAsync()
    {
        return await dbContext.Users.OrderBy(u => u.Id).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await dbContext.Users.FindAsync(id);
    }

    public async Task<User> CreateAsync(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(int id, User updatedUser)
    {
        var existingUser = await dbContext.Users.FindAsync(id);
        if (existingUser is null)
        {
            return false;
        }

        existingUser.Name = updatedUser.Name;
        existingUser.Email = updatedUser.Email;
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existingUser = await dbContext.Users.FindAsync(id);
        if (existingUser is null)
        {
            return false;
        }

        dbContext.Users.Remove(existingUser);
        await dbContext.SaveChangesAsync();

        return true;
    }
}

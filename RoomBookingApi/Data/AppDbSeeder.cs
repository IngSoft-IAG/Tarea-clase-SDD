using RoomBookingApi.Domain;

namespace RoomBookingApi.Data;

public static class AppDbSeeder
{
    public static void Seed(AppDbContext dbContext)
    {
        if (!dbContext.Rooms.Any())
        {
            dbContext.Rooms.AddRange(
                new Room { Name = "Sala Norte", Capacity = 8, IsActive = true },
                new Room { Name = "Sala Sur", Capacity = 12, IsActive = true },
                new Room { Name = "Sala Legacy", Capacity = 4, IsActive = false }
            );
        }

        if (!dbContext.Users.Any())
        {
            dbContext.Users.AddRange(
                new User { Name = "Ana Silva", Email = "ana@empresa.com" },
                new User { Name = "Bruno Perez", Email = "bruno@empresa.com" }
            );
        }

        dbContext.SaveChanges();
    }
}

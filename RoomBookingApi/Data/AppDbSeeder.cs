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

        if (!dbContext.Reservations.Any())
        {
            var firstActiveRoom = dbContext.Rooms.FirstOrDefault(r => r.IsActive);
            var firstUser = dbContext.Users.OrderBy(u => u.Id).FirstOrDefault();
            var secondUser = dbContext.Users.OrderBy(u => u.Id).Skip(1).FirstOrDefault();

            if (firstActiveRoom is not null && firstUser is not null)
            {
                var tomorrow10 = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
                dbContext.Reservations.Add(new Reservation
                {
                    RoomId = firstActiveRoom.Id,
                    UserId = firstUser.Id,
                    StartAt = tomorrow10,
                    EndAt = tomorrow10.AddHours(1),
                    Status = ReservationStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });

                if (secondUser is not null)
                {
                    var twoDays15 = DateTime.UtcNow.Date.AddDays(2).AddHours(15);
                    dbContext.Reservations.Add(new Reservation
                    {
                        RoomId = firstActiveRoom.Id,
                        UserId = secondUser.Id,
                        StartAt = twoDays15,
                        EndAt = twoDays15.AddHours(2),
                        Status = ReservationStatus.Cancelled,
                        CreatedAt = DateTime.UtcNow.AddHours(-1),
                        CancelledAt = DateTime.UtcNow
                    });
                }

                dbContext.SaveChanges();
            }
        }
    }
}

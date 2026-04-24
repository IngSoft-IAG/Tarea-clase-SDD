using Microsoft.AspNetCore.Mvc;
using RoomBookingApi.Controllers;
using RoomBookingApi.Domain;
using RoomBookingApi.DTOs;
using RoomBookingApi.Services;

namespace RoomBookingApi.Tests;

[TestClass]
public class ReservationsControllerTests
{
    [TestMethod]
    public async Task Create_HappyPath_ReturnsCreatedAtActionWithReservationDto()
    {
        var startUtc = new DateTime(2026, 4, 23, 9, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc);
        var createdAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        var service = new FakeReservationService
        {
            CreateResult = ReservationCreateResult.Success(new Reservation
            {
                Id = 7,
                UserId = 1,
                RoomId = 2,
                StartUtc = startUtc,
                EndUtc = endUtc,
                CreatedAtUtc = createdAtUtc
            })
        };

        var controller = new ReservationsController(service);
        var request = new CreateReservationDto
        {
            UserId = 1,
            RoomId = 2,
            StartUtc = startUtc,
            EndUtc = endUtc
        };

        var actionResult = await controller.Create(request);

        Assert.IsNotNull(service.LastCommand);
        Assert.AreEqual(request.UserId, service.LastCommand!.UserId);
        Assert.AreEqual(request.RoomId, service.LastCommand.RoomId);
        Assert.AreEqual(request.StartUtc, service.LastCommand.StartUtc);
        Assert.AreEqual(request.EndUtc, service.LastCommand.EndUtc);

        var createdAt = actionResult.Result as CreatedAtActionResult;
        Assert.IsNotNull(createdAt);
        Assert.AreEqual(nameof(ReservationsController.GetById), createdAt.ActionName);

        var dto = createdAt.Value as ReservationDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual(7, dto.Id);
        Assert.AreEqual(1, dto.UserId);
        Assert.AreEqual(2, dto.RoomId);
        Assert.AreEqual(startUtc, dto.StartUtc);
        Assert.AreEqual(endUtc, dto.EndUtc);
        Assert.AreEqual(createdAtUtc, dto.CreatedAtUtc);
    }

    [TestMethod]
    public async Task Create_WhenServiceReturnsInvalid_ReturnsBadRequestWithExactMessage()
    {
        const string errorMessage = "StartUtc and EndUtc must be UTC.";
        var service = new FakeReservationService
        {
            CreateResult = ReservationCreateResult.Invalid(errorMessage)
        };

        var controller = new ReservationsController(service);
        var actionResult = await controller.Create(CreateRequest());

        var badRequest = actionResult.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(errorMessage, badRequest.Value);
    }

    [TestMethod]
    public async Task Create_WhenServiceReturnsNotFound_ReturnsNotFoundWithExactMessage()
    {
        const string errorMessage = "User not found.";
        var service = new FakeReservationService
        {
            CreateResult = ReservationCreateResult.NotFound(errorMessage)
        };

        var controller = new ReservationsController(service);
        var actionResult = await controller.Create(CreateRequest());

        var notFound = actionResult.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound);
        Assert.AreEqual(errorMessage, notFound.Value);
    }

    [TestMethod]
    public async Task Create_WhenServiceReturnsConflict_ReturnsConflictWithExactMessage()
    {
        const string errorMessage = "Room is already reserved for the selected time range.";
        var service = new FakeReservationService
        {
            CreateResult = ReservationCreateResult.Conflict(errorMessage)
        };

        var controller = new ReservationsController(service);
        var actionResult = await controller.Create(CreateRequest());

        var conflict = actionResult.Result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
        Assert.AreEqual(errorMessage, conflict.Value);
    }

    private static CreateReservationDto CreateRequest()
    {
        return new CreateReservationDto
        {
            UserId = 1,
            RoomId = 2,
            StartUtc = new DateTime(2026, 4, 23, 9, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc)
        };
    }

    private sealed class FakeReservationService : IReservationService
    {
        public ReservationCreateResult CreateResult { get; set; } = ReservationCreateResult.Invalid("Not configured.");
        public ReservationCreateCommand? LastCommand { get; private set; }

        public Task<ReservationCreateResult> CreateAsync(ReservationCreateCommand command)
        {
            LastCommand = command;
            return Task.FromResult(CreateResult);
        }

        public Task<Reservation?> GetByIdAsync(int id)
        {
            return Task.FromResult<Reservation?>(null);
        }
    }
}

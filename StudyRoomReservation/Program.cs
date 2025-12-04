namespace StudyRoomReservation;
using Concurrency;
using Repository;
using Services;
using Microsoft.AspNetCore.Mvc;
class Program
{
    static async Task Main(string[] args)
    {
        DatabaseConfig.Load();
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddSingleton<RoomRepository>();
        builder.Services.AddSingleton<ReservationRepository>();
        
        builder.Services.AddSingleton<RoomService>();
        builder.Services.AddSingleton<ReservationService>();
        
        builder.Services.AddSingleton<ReservationProcessor>();

        var app = builder.Build();
        
        var processor = app.Services.GetRequiredService<ReservationProcessor>();
        processor.Start();
        Console.WriteLine("Reservation processor started");
        
        app.MapGet("/api/rooms", ([FromServices] RoomService roomService) =>
        {
            var rooms = roomService.GetAllRooms();
            var result = rooms.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                capacity = r.Capacity,
                seats = (r.Seats ?? new List<Seat>()).Select(s => new { id = s.Id }).ToList()
            }).ToList();

            Console.WriteLine("JSON to send: " + System.Text.Json.JsonSerializer.Serialize(result));
            return Results.Ok(result);
        });
        
        app.MapPost("/api/reserve", async (ReservationRequest request, ReservationService reservationService) =>
        {
            try
            {
                Console.WriteLine("Reservation Request Received");
                Console.WriteLine($"Request object is null: {request == null}");

                if (request == null)
                {
                    Console.WriteLine("Request is null");
                    return Results.BadRequest(new { error = "Request body is null" });
                }

                Console.WriteLine($"RoomId: {request.RoomId}");
                Console.WriteLine($"SeatId: {request.SeatId}");
                Console.WriteLine($"Username: '{request.Username}'");
                Console.WriteLine($"StartTime: {request.StartTime}");
                Console.WriteLine($"EndTime: {request.EndTime}");
                
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    Console.WriteLine("Username is missing or empty");
                    return Results.BadRequest(new { error = "Username is required" });
                }

                if (request.SeatId <= 0)
                {
                    Console.WriteLine($"Invalid SeatId: {request.SeatId}");
                    return Results.BadRequest(new { error = "Valid SeatId is required" });
                }

                if (request.StartTime >= request.EndTime)
                {
                    Console.WriteLine($"StartTime ({request.StartTime}) >= EndTime ({request.EndTime})");
                    return Results.BadRequest(new { error = "StartTime must be before EndTime" });
                }

                Console.WriteLine("Validation passed, creating reservation...");
                
                var reservation = reservationService.CreateReservation(request);

                Console.WriteLine($"Reservation created successfully - ID: {reservation.Id}");

                return Results.Ok(new 
                { 
                    success = true,
                    message = "Reservation created successfully",
                    reservation = new
                    {
                        id = reservation.Id,
                        roomId = reservation.RoomId,
                        seatId = reservation.SeatId,
                        username = reservation.Username,
                        startTime = reservation.StartTime,
                        endTime = reservation.EndTime
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"InvalidOperationException: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack: {ex.InnerException.StackTrace}");
                }
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();

        await app.RunAsync();
    }
}
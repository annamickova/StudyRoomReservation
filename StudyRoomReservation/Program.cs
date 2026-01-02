namespace StudyRoomReservation;
using Concurrency;
using Repository;
using Services;
using Microsoft.AspNetCore.Mvc;
class Program
{
    static async Task Main(string[] args)
    {
        Logger.Configure();

        try
        {
            DatabaseConfig.Load();
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<UserRepository>();

            builder.Services.AddSingleton<RoomRepository>();
            builder.Services.AddSingleton<ReservationRepository>();
            
            builder.Services.AddSingleton<RoomService>();
            builder.Services.AddSingleton<ReservationService>();
            
            builder.Services.AddSingleton<ReservationProcessor>();

            var app = builder.Build();
            
            var processor = app.Services.GetRequiredService<ReservationProcessor>();
            processor.Start();
            Logger.Info("Reservation processor started");
            
            app.MapGet("/api/rooms", ([FromServices] RoomService roomService) =>
            {
                var rooms = roomService.GetAllRooms();
                var result = rooms.Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    capacity = r.Capacity,
                    seats = r.Seats.Select(s => new { id = s.Id }).ToList()
                }).ToList();

                Logger.Debug("JSON to send: " + System.Text.Json.JsonSerializer.Serialize(result));
                return Results.Ok(result);
            });
            
            app.MapPost("/api/reserve", async (ReservationRequest request, ReservationService reservationService) =>
            {
                try
                {
                    Logger.Info("Reservation Request Received");
                    Logger.Debug($"Request object null: {request == null}");

                    if (request == null)
                    {
                        Logger.Warning("Request is null");
                        return Results.BadRequest(new { error = "Request body is null" });
                    }

                    Logger.Debug($"RoomId: {request.RoomId}");
                    Logger.Debug($"SeatId: {request.SeatId}");
                    Logger.Debug($"Username: '{request.Username}'");
                    Logger.Debug($"StartTime: {request.StartTime}");
                    Logger.Debug($"EndTime: {request.EndTime}");
                    
                    if (string.IsNullOrWhiteSpace(request.Username))
                    {
                        Logger.Warning("Username is missing or empty");
                        return Results.BadRequest(new { error = "Username is required" });
                    }

                    if (request.SeatId <= 0)
                    {
                        Logger.Warning($"Invalid SeatId: {request.SeatId}");
                        return Results.BadRequest(new { error = "Valid SeatId is required" });
                    }

                    if (request.StartTime >= request.EndTime)
                    {
                        Logger.Warning($"StartTime ({request.StartTime}) >= EndTime ({request.EndTime})");
                        return Results.BadRequest(new { error = "StartTime must be before EndTime" });
                    }

                    Logger.Info("Validation passed, creating reservation.");
                    
                    var reservation = reservationService.CreateReservation(request);

                    Logger.Info($"Reservation {reservation.Id} created successfully");

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
                    Logger.Error($"Failed to create reservation for {request.Username}"); 
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    Logger.Error($"Exception Type: {ex.GetType().Name}");
                    Logger.Error($"Message: {ex.Message}");
                    Logger.Error($"StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Logger.Error($"Inner: {ex.InnerException.Message}");
                    }

                    return Results.Problem(detail: ex.Message, statusCode: 500);
                }
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            await app.RunAsync(); 
        }
        catch (Exception)
        {
            Logger.Error("Application failed to start");
            throw;
        }
        
    }
}
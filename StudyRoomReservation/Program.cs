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
            builder.Services.AddSingleton<ImportRepository>();

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
                    floor = r.Floor,
                    seats = r.Seats.Select(s => new { id = s.Id }).ToList(),
                    equipment = (r.Equipment ?? new List<Equipment>()).Select(e => new 
                    { 
                        id = e.Id, 
                        name = e.Name 
                    }).ToList()
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
            
            app.MapGet("/api/reservations", ([FromServices] ReservationRepository repository) =>
            {
                try
                {
                    Logger.Info("Fetching all reservations...");
                    var reservations = repository.GetAllReservationsWithDetails();
                    Logger.Info($"Retrieved {reservations.Count} reservations");
                    return Results.Ok(reservations);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to get reservations: {ex.Message}");
                    Console.WriteLine($"Full exception: {ex}");  // ← Add this for more detail
                    return Results.Problem(ex.Message);
                }
            });
                        
            app.MapDelete("/api/reservations/{id:int}", (int id, [FromServices] ReservationRepository repository) =>
            {
                try
                {
                    var success = repository.DeleteReservation(id);
                    if (success)
                    {
                        Logger.Info($"Deleted reservation {id}");
                        return Results.Ok(new { message = "Reservation deleted successfully" });
                    }
                    else
                    {
                        Logger.Warning($"Reservation {id} not found");
                        return Results.NotFound(new { error = "Reservation not found" });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to delete reservation {id}");
                    return Results.Problem(ex.Message);
                }
            });

            app.MapPut("/api/reservations/{id:int}", (int id, [FromBody] UpdateReservationRequest request, [FromServices] ReservationRepository repository) =>
            {
                try
                {
                    var success = repository.UpdateReservation(id, request.StartTime, request.EndTime);
                    if (success)
                    {
                        Logger.Info($"Updated reservation {id}");
                        return Results.Ok(new { message = "Reservation updated successfully" });
                    }
                    else
                    {
                        Logger.Warning($"Reservation {id} not found");
                        return Results.NotFound(new { error = "Reservation not found" });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Warning($"Update conflict for reservation {id}: {ex.Message}");
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to update reservation {id}");
                    return Results.Problem(ex.Message);
                }
            });
            
            app.MapPost("/api/reservations/{id:int}/confirm", (int id, [FromServices] ReservationRepository repository) =>
            {
                try
                {
                    var success = repository.ConfirmReservation(id);
                    if (success)
                    {
                        Logger.Info($"Confirmed reservation {id}");
                        return Results.Ok(new { message = "Reservation confirmed" });
                    }
                    else
                    {
                        return Results.NotFound(new { error = "Reservation not found" });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to confirm reservation {id}");
                    return Results.Problem(ex.Message);
                }
            });
            
            app.MapPost("/api/import/rooms", async (HttpRequest request, [FromServices] ImportRepository importRepo) =>
            {
                try
                {
                    using var reader = new StreamReader(request.Body);
                    var csvContent = await reader.ReadToEndAsync();

                    if (string.IsNullOrWhiteSpace(csvContent))
                        return Results.BadRequest(new { error = "Empty CSV content" });

                    int count = importRepo.ImportRoomsFromCsv(csvContent);
        
                    return Results.Ok(new { message = $"Imported {count} rooms", count });
                }
                catch (Exception ex)
                {
                    Logger.Error("Room import failed");
                    return Results.BadRequest(new { error = ex.Message });
                }
            });
            
            app.MapPost("/api/import/equipment", async (HttpRequest request, [FromServices] ImportRepository importRepo) =>
            {
                try
                {
                    using var reader = new StreamReader(request.Body);
                    var csvContent = await reader.ReadToEndAsync();

                    if (string.IsNullOrWhiteSpace(csvContent))
                        return Results.BadRequest(new { error = "Empty CSV content" });

                    int count = importRepo.ImportEquipmentFromCsv(csvContent);
        
                    return Results.Ok(new { message = $"Imported {count} equipment items", count });
                }
                catch (Exception ex)
                {
                    Logger.Error("Equipment import failed");
                    return Results.BadRequest(new { error = ex.Message });
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

public class UpdateReservationRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
using StudyRoomReservation.Concurrency;
using StudyRoomReservation.Repository;
using StudyRoomReservation.Services;

namespace StudyRoomReservation;

class Program
{
    static async Task Main(string[] args)
    {
        DatabaseConfig.Load();
        var roomRepo = new RoomRepository();
        var reservationRepo = new ReservationRepository();
        
        var roomService = new RoomService(roomRepo);
        var reservationService = new ReservationService(roomService, reservationRepo);
        
        var processor = new ReservationProcessor(reservationService, roomService);
        processor.Start();
        Console.WriteLine("=== STARTING TEST ===");

        processor.Start();
        var room = new Room("TestRoom", 10);
        roomService.AddRoom(room);
        Console.WriteLine($"Room created {room.Id}");
        var request1 = new ReservationRequest(room.Id, room.Seats[0].Id, "Emma", DateTime.Now, DateTime.Now.AddHours(1));
        var request2 = new ReservationRequest(room.Id, room.Seats[0].Id, "Oleg", DateTime.Now.AddHours(1), DateTime.Now.AddHours(2));
        var request3 = new ReservationRequest(room.Id, room.Seats[2].Id, "Alex", DateTime.Now.AddMinutes(30), DateTime.Now.AddHours(1.5));
        
        var task1 = processor.EnqueueAsync(request1);
        var task2 = processor.EnqueueAsync(request2);
        var task3 = processor.EnqueueAsync(request3);
        try
        { 
            var res1 = await task1; 
            Console.WriteLine($"Reservation 1 created, seat: {res1.SeatId}");
            var res2 = await task2; 
            Console.WriteLine($"Reservation 2 created, seat: {res2.SeatId}");
            var res3 = await task3; 
            Console.WriteLine($"Reservation 3 created, seat: {res3.SeatId}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Correctly blocked conflict: {ex.Message}");
        }   
        
        Console.WriteLine("=== TEST COMPLETE ===");
    }
    
        
}
using System.Collections.Concurrent;
using StudyRoomReservation.Services;

namespace StudyRoomReservation.Concurrency;

/// <summary>
/// Process incoming reservation request. Ensures thread-safety.
/// </summary>
public class ReservationProcessor
{
    private readonly ReservationService _reservationService;
    private readonly RoomService _roomService;
    private readonly ConcurrentQueue<ReservationRequest> _queue = new();
    private readonly AutoResetEvent _signal = new(false);
    public ReservationProcessor(ReservationService reservationService, RoomService roomService)
    {
        _reservationService = reservationService;
        _roomService = roomService;
    }
    
    /// <summary>
    /// Enqueues a reservation request for asynchronous processing.
    /// </summary>
    public Task<Reservation> EnqueueAsync(ReservationRequest request)
    {
        _queue.Enqueue(request);
        _signal.Set();
        return request.Completion.Task;
    }

    /// <summary>
    /// Starts process that handles incoming reservation requests.
    /// </summary>
    public void Start()
    {
        Task.Run(() =>
        {
            while (true)
            {
                _signal.WaitOne();

                while (_queue.TryDequeue(out var req))
                {
                    try
                    {
                        ProcessRequest(req);
                    }
                    catch (Exception e)
                    {
                        req.Completion.SetException(e);
                    }
                }
            }
        });
    }
    
    /// <summary>
    /// Processes a single reservation request: validates the room and seat, creates a reservation, 
    /// and completes the associated Task.
    /// </summary>
    /// <param name="req"></param>
    /// <exception cref="Exception"></exception>
    private void ProcessRequest(ReservationRequest req)
    {
        var room = _roomService.GetRoom(req.RoomId)
                   ?? throw new Exception("Room not found.");

        if (!room.Seats.Any(s => s.Id == req.SeatId))
            throw new Exception("Seat not found in room.");

        var reservation = new Reservation(req.RoomId, req.SeatId,
            "username", req.StartTime, req.EndTime);

        _reservationService.CreateReservation(reservation);

        req.Completion.SetResult(reservation);
    }

}
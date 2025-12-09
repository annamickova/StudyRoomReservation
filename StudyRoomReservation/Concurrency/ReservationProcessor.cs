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
    
    private readonly int _workerCount = 4;
    private readonly List<Thread> _workers = new();
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
        Logger.Info($"Starting {_workerCount} worker threads...");
        
        for (int i = 0; i < _workerCount; i++)
        {
            var worker = new Thread(ProcessQueue)
            {
                IsBackground = true,
                Name = $"Worker-{i + 1}"
            };
            _workers.Add(worker);
            worker.Start();
            Logger.Info($"Worker {i + 1} started.");
        }
    }
    
    /// <summary>
    /// Worker thread method that continuously processes queued reservation requests.
    /// </summary>
    private void ProcessQueue()
    {
        while (true)
        {
            _signal.WaitOne();

            while (_queue.TryDequeue(out var req))
            {
                Logger.Info($"Processing request {req}");
                try
                {
                    ProcessRequest(req);
                    Logger.Info("Request completed");
                }
                catch (Exception e)
                {
                    req.Completion.SetException(e);
                    Logger.Error("Failed to process request");
                }
            }
        }
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

        var reservation = _reservationService.CreateReservation(req);

        req.Completion.SetResult(reservation);
    }

}
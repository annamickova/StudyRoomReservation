namespace StudyRoomReservation.Services;

/// <summary>
/// Class responsible for creating valid reservations.
/// </summary>
public class ReservationService
{
    private readonly RoomService _roomService;
    private readonly List<Reservation> _reservations = new();
    private readonly object _lock = new();
    
    /// <summary>
    /// Initializes a new instance of the RoomService.
    /// </summary>
    /// <param name="roomService"></param>
    public ReservationService(RoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Attempts to create a new reservation.
    /// </summary>
    /// <param name="reservation">Reservation to be created</param>
    /// <exception cref="ArgumentNullException">Thrown if reservation is null</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the seat is already reserved during the requested time or room/seat does not exist
    /// </exception>
    public void CreateReservation(Reservation reservation)
    {
        if (reservation == null) throw new ArgumentNullException(nameof(reservation));
        var seat = _roomService.GetSeat(reservation.RoomId, reservation.SeatId);

        lock (_lock)
        {
            var conflicting = _reservations
                .Where(r => r.RoomId == reservation.RoomId && r.SeatId == reservation.SeatId)
                .Any(r => TimeOverlap(r.StartTime, r.EndTime, reservation.StartTime, reservation.EndTime));

            if (conflicting) throw new InvalidOperationException("Seat is already reserved during this time interval.");
            _reservations.Add(reservation);
        }
    }
    
    /// <summary>
    /// Returns all reservations.
    /// </summary>
    public IReadOnlyList<Reservation> GetAllReservations()
    {
        lock (_lock)
        {
            return _reservations.AsReadOnly();
        }
    }

    /// <summary>
    /// Determines whether two time intervals overlap.
    /// </summary>
    private bool TimeOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }
}
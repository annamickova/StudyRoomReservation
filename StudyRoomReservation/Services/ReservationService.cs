using StudyRoomReservation.Concurrency;
using StudyRoomReservation.Repository;

namespace StudyRoomReservation.Services;

/// <summary>
/// Class responsible for creating valid reservations.
/// </summary>
public class ReservationService
{
    private readonly RoomService _roomService;
    private readonly ReservationRepository _repository;
    
    /// <summary>
    /// Initializes a new instance of the ReservationService.
    /// </summary>
    /// <param name="roomService">Room service for validating rooms and seats</param>
    /// <param name="repository">Repository for persistence</param>
    public ReservationService(RoomService roomService, ReservationRepository repository)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Attempts to create a new reservation in the database.
    /// </summary>
    /// <param name="reservation">Reservation to be created</param>
    /// <exception cref="ArgumentNullException">Thrown if reservation is null</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the seat is already reserved during the requested time or room/seat does not exist
    /// </exception>
    public Reservation CreateReservation(ReservationRequest request)
    {
        var rooms = _roomService.GetAllRooms();
        var room = rooms.FirstOrDefault(r => r.Seats.Any(s => s.Id == request.SeatId));
        
        if (room == null)
            throw new InvalidOperationException($"Seat with ID {request.SeatId} not found in any room");
        
        if (request.RoomId == 0)
            request.RoomId = room.Id;

        var reservation = new Reservation(
            request.RoomId,
            request.SeatId,
            request.Username,
            request.StartTime,
            request.EndTime
        );

        _repository.AddReservation(reservation);
        return reservation;
    }

    /// <summary>
    /// Returns all reservations from the database for a specific room or all rooms.
    /// </summary>
    /// <param name="roomId">Optional room ID filter</param>
    public List<Reservation> GetAllReservations(int? roomId = null)
    {
        if (roomId.HasValue)
            return _repository.GetReservationsForRoom(roomId.Value);
        else
            return _repository.GetAllReservations();
    }
}

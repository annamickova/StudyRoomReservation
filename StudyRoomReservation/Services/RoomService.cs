namespace StudyRoomReservation.Services;
/// <summary>
/// Service responsible for managing rooms and seats. 
/// </summary>
public class RoomService
{
    private readonly List<Room> _rooms = new();

    /// <summary>
    /// Creates a new room.
    /// </summary>
    /// <param name="room">Room to be added</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void AddRoom(Room room)
    {
        if (room == null) throw new ArgumentNullException(nameof(room));
        if (_rooms.Any(r => r.Id == room.Id))  
            throw new InvalidOperationException($"Room with ID {room.Id} already exists.");
        _rooms.Add(room);
    }
    
    /// <summary>
    /// Returns all current rooms.
    /// </summary>
    public IReadOnlyList<Room> GetAllRooms()
    {
        return _rooms.AsReadOnly();
    }

    /// <summary>
    /// Returns a room by id.
    /// </summary>
    /// <param name="id">Id of the room</param>
    /// <returns>Room if found</returns>
    public Room? GetRoom(int id)
    {
        return _rooms.FirstOrDefault(r => r.Id == id);
    }

    /// <summary>
    /// Returns a specific seat from room.
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="seatId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Seat GetSeat(int roomId, int seatId)
    {
        var room = GetRoom(roomId);
        if (room == null) throw new InvalidOperationException($"Room with ID {roomId} does not exist.");
        var seat = room.Seats.FirstOrDefault(s => s.Id == seatId);
        if (seat == null) throw new InvalidOperationException($"Seat with ID {seatId} does not exist.");
        return seat;
    }
}
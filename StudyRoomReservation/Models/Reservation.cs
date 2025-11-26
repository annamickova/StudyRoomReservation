namespace StudyRoomReservation;
/// <summary>
/// Model class representing one reservation on a chosen seat in a room.
/// </summary>
public class Reservation
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int SeatId { get; set; }
    public string Username { get; set; } = String.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Creates a new reservation instance.
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="room">Id of the room</param>
    /// <param name="seat">Id of the seat</param>
    /// <param name="username">Name of user who made reservation</param>
    /// <param name="start">Start time of the reservation</param>
    /// <param name="end">End time of the reservation</param>
    /// <exception cref="ArgumentException">Thrown when end time is earlier than start time</exception>
    public Reservation(int room, int seat, string username, DateTime start, DateTime end)
    {
        if (start >= end) throw new ArgumentException("Start time must be before end time.");
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.");
        
        RoomId = room;
        SeatId = seat;
        Username = username;
        StartTime = start;
        EndTime = end;
    }
}
namespace StudyRoomReservation;
/// <summary>
/// Model class representing a single seat in a room, that can be reserved.
/// </summary>
public class Seat
{
    public int Id { get; set; }
    public int RoomId { get; set; }

    /// <summary>
    /// Initializes instance of Seat class.
    /// </summary>
    public Seat(int roomId)
    {
        RoomId = roomId;
    }

    /// <summary>
    /// Empty constructor for json.
    /// </summary>
    public Seat() { }
}


namespace StudyRoomReservation;
/// <summary>
/// Model class representing room for studying that can be reserved by users.
/// </summary>
public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public List<Seat> Seats { get; set; } = new List<Seat>();
    public int Capacity { get; set; }

    /// <summary>
    /// Initializes a new instance of Room class.
    /// </summary>
    /// <param name="id">Unique room identifier</param>
    /// <param name="name">Name of the room</param>
    /// <param name="capacity">Maximum number of seats</param>
    /// <exception cref="ArgumentException"></exception>
    public Room(int id, string name, int capacity)
    {
        if (id <= 0) throw new ArgumentException("Room id must be greater than zero.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Room name cannot be empty.");
        if (capacity <= 0) throw new ArgumentException("Room capacity must be greater than zero.");

        Id = id;
        Name = name;
        Capacity = capacity;

        for (int i = 1; i < capacity; i++)
        {
            Seats.Add(new Seat(i));
        }
    }

    /// <summary>
    /// Method finding a seat by seat's number.
    /// </summary>
    /// <param name="seatNumber"></param>
    /// <returns>Found Seat</returns>
    /// <exception cref="ArgumentException">Exception if no matching Seat exits</exception>
    public Seat GetSeat(int seatNumber)
    {
        return Seats.FirstOrDefault(s => s.Id == seatNumber)
               ?? throw new ArgumentException("Seat does not exist in this room.");
    }
}
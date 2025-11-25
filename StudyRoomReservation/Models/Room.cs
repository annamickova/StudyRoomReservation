namespace StudyRoomReservation;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public List<Seat> Seats { get; set; } = new List<Seat>();
    public int Capacity { get; set; }

    public Room(int id, string name, int capacity)
    {
        if (id <= 0) throw new ArgumentException("Room id must be greater than zero.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Room name cannot be empty.");
        if (capacity <= 0) throw new ArgumentException("Room capacity must be greater than zero.");

        Id = id;
        Name = name;
        Capacity = capacity;

        for (int i = 0; i < capacity; i++)
        {
            Seats.Add(new Seat(i));
        }
    }

    public Seat GetSeat(int seatNumber)
    {
        return Seats.FirstOrDefault(s => s.Id == seatNumber)
               ?? throw new ArgumentException("Seat does not exist in this room.");
    }
}
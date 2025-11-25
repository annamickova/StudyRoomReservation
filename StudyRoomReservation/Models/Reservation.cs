namespace StudyRoomReservation;

public class Reservation
{
    public int Id { get; set; }
    public Room Room { get; set; }
    public Seat Seat { get; set; }
    public string Username { get; set; } = String.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public Reservation(int id, Room room, Seat seat, string username, DateTime start, DateTime end)
    {
        if (id <= 0) throw new ArgumentException("Id must be greater than zero.");
        if (start >= end) throw new ArgumentException("Start time must be before end time.");
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.");
        
        Id = id;
        Room = room;
        Seat = seat;
        Username = username;
        StartTime = start;
        EndTime = end;
    }
}
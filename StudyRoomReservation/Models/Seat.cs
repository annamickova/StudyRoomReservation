namespace StudyRoomReservation;

public class Seat
{
    public int Id { get; set; }
    public bool IsReserved { get; set; }
    public string? ReservedBy { get; set; }
    
    public Seat(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Seat ID must be positive.");

        Id = id;
    }

    public void Reserve(string user)
    {
        if (IsReserved)
        {
            throw new InvalidOperationException("Seat is already reserved.");
        }

        ReservedBy = user;
        IsReserved = true;
    }

    public void CancelReservation()
    {
        IsReserved = false;
        ReservedBy = null;
    }
    
}


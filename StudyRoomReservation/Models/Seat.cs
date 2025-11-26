namespace StudyRoomReservation;
/// <summary>
/// Model class representing a single seat in a room, that can be reserved.
/// </summary>
public class Seat
{
    public int Id { get; set; }
    public bool IsReserved { get; set; }
    public string? ReservedBy { get; set; }
    
    /// <summary>
    /// Initializes instance of Seat class.
    /// </summary>
    public Seat() { }

    /// <summary>
    /// Method reserving new seat.
    /// </summary>
    /// <param name="user">User that reserved the seat</param>
    /// <exception cref="InvalidOperationException">If seat is already reserved</exception>
    public void Reserve(string user)
    {
        if (IsReserved)
        {
            throw new InvalidOperationException("Seat is already reserved.");
        }

        ReservedBy = user;
        IsReserved = true;
    }

    /// <summary>
    /// Canceling reservation.
    /// </summary>
    public void CancelReservation()
    {
        IsReserved = false;
        ReservedBy = null;
    }
    
}


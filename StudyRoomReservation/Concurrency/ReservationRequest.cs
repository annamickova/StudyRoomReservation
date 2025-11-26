namespace StudyRoomReservation.Concurrency;

/// <summary>
/// Represents a reservation request.
/// </summary>
public class ReservationRequest
{
    public int RoomId { get; set; }
    public int SeatId { get; set; }
    public string Username { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public TaskCompletionSource<Reservation> Completion { get; set; } = new();

    /// <summary>
    /// Convenience constructor to create a request in one call.
    /// </summary>
    public ReservationRequest(int roomId, int seatId, string username, DateTime startTime, DateTime endTime)
    {
        RoomId = roomId;
        SeatId = seatId;
        Username = username;
        StartTime = startTime;
        EndTime = endTime;
    }
}
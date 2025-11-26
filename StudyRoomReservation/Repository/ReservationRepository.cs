namespace StudyRoomReservation.Repository;
using MySql.Data.MySqlClient;
/// <summary>
/// Repository handling saving reservations into database.
/// </summary>
public class ReservationRepository
{
    /// <summary>
    /// Adds new reservation to database
    /// and checking if reservation can be created on wanted time block.
    /// </summary>
    /// <param name="reservation">New reservation</param>
    /// <exception cref="Exception">Thrown if reservation can not be made
    /// due to time collision with some other reservation</exception>
    public void AddReservation(Reservation reservation)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();
        
        var startTime = new DateTime(reservation.StartTime.Year, reservation.StartTime.Month, reservation.StartTime.Day,
            reservation.StartTime.Hour, reservation.StartTime.Minute, reservation.StartTime.Second);
        var endTime = new DateTime(reservation.EndTime.Year, reservation.EndTime.Month, reservation.EndTime.Day,
            reservation.EndTime.Hour, reservation.EndTime.Minute, reservation.EndTime.Second);

        using var command = new MySqlCommand(@"
        SELECT COUNT(*) FROM reservation 
        WHERE room_id=@room_id AND seat_id=@seat_id 
          AND ((start_time < @end_time) AND (end_time > @start_time))", conn);

        command.Parameters.AddWithValue("@room_id", reservation.RoomId);
        command.Parameters.AddWithValue("@seat_id", reservation.SeatId);
        command.Parameters.AddWithValue("@start_time", startTime);
        command.Parameters.AddWithValue("@end_time", endTime);

        var conflicts = Convert.ToInt32(command.ExecuteScalar());
        if (conflicts > 0)
            throw new InvalidOperationException("Seat is already reserved during this time interval.");

        
        using var insertCmd = new MySqlCommand(@"
            INSERT INTO reservation (room_id, seat_id, username, start_time, end_time) 
            VALUES (@room_id, @seat_id, @username, @start, @end)", conn);
        insertCmd.Parameters.AddWithValue("@room_id", reservation.RoomId);
        insertCmd.Parameters.AddWithValue("@seat_id", reservation.SeatId);
        insertCmd.Parameters.AddWithValue("@username", reservation.Username);
        insertCmd.Parameters.AddWithValue("@start", reservation.StartTime);
        insertCmd.Parameters.AddWithValue("@end", reservation.EndTime);

        insertCmd.ExecuteNonQuery();
        
        reservation.Id = (int)insertCmd.LastInsertedId;
    }
    
    /// <summary>
    /// Gets reservations from specific room from database.
    /// </summary>
    /// <param name="roomId">Room's id</param>
    /// <returns>A list of all reservations from one room</returns>
    public List<Reservation> GetReservationsForRoom(int roomId)
    {
        var list = new List<Reservation>();

        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "SELECT id, room_id, seat_id, username, start_time, end_time FROM reservation WHERE room_id=@room_id", conn);
        cmd.Parameters.AddWithValue("@room_id", roomId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Reservation(
                reader.GetInt32("room_id"),
                 reader.GetInt32("seat_id"),
                reader.GetString("username"),
                reader.GetDateTime("start_time"), 
                reader.GetDateTime("end_time")));
        }

        return list;
    }
    
    /// <summary>
    /// Gets list of all reservations and information about them.
    /// </summary>
    /// <returns>All saved reservations</returns>
    public List<Reservation> GetAllReservations()
    {
        var list = new List<Reservation>();
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "SELECT id, room_id, seat_id, username, start_time, end_time FROM reservation", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Reservation(
                reader.GetInt32("room_id"), 
                reader.GetInt32("seat_id"),
                reader.GetString("username"),
                reader.GetDateTime("start_time"), 
                reader.GetDateTime("end_time")));
        }

        return list;
    } 
}
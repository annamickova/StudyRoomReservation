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
        
        using var transaction = conn.BeginTransaction();

        try
        {
            var startTime = new DateTime(reservation.StartTime.Year, reservation.StartTime.Month, reservation.StartTime.Day,
                reservation.StartTime.Hour, reservation.StartTime.Minute, reservation.StartTime.Second);
            var endTime = new DateTime(reservation.EndTime.Year, reservation.EndTime.Month, reservation.EndTime.Day,
                reservation.EndTime.Hour, reservation.EndTime.Minute, reservation.EndTime.Second);

            using var command = new MySqlCommand(@"
            SELECT COUNT(*) FROM reservation 
            WHERE seat_id=@seat_id 
            AND ((start_time < @end_time) AND (end_time > @start_time))", conn, transaction);

            command.Parameters.AddWithValue("@seat_id", reservation.SeatId);
            command.Parameters.AddWithValue("@start_time", startTime);
            command.Parameters.AddWithValue("@end_time", endTime);

            var conflicts = Convert.ToInt32(command.ExecuteScalar());
            if (conflicts > 0)
                throw new InvalidOperationException("Seat is already reserved during this time interval.");

            using var insertCmd = new MySqlCommand(@"
            INSERT INTO reservation (seat_id, user_id, start_time, end_time, is_confirmed) 
            VALUES (@seat_id, @user_id, @start, @end, @is_confirmed)", conn, transaction);
    
            insertCmd.Parameters.AddWithValue("@seat_id", reservation.SeatId);
            insertCmd.Parameters.AddWithValue("@user_id", reservation.UserId);
            insertCmd.Parameters.AddWithValue("@start", reservation.StartTime);
            insertCmd.Parameters.AddWithValue("@end", reservation.EndTime);
            insertCmd.Parameters.AddWithValue("@is_confirmed", reservation.IsConfirmed);

            insertCmd.ExecuteNonQuery();
        
            reservation.Id = (int)insertCmd.LastInsertedId;
            
            using (var logCmd = new MySqlCommand(@"
            INSERT INTO reservation_log (reservation_id, action, created_at) 
            VALUES (@reservation_id, @action, NOW())", conn, transaction))
            {
                logCmd.Parameters.AddWithValue("@reservation_id", reservation.Id);
                logCmd.Parameters.AddWithValue("@action", "CREATED");

                logCmd.ExecuteNonQuery();
            }
            transaction.Commit();
            Logger.Info($"Reservation ID {reservation.Id} saved to database");
        }
        catch (Exception)
        {
           transaction.Rollback();
           Logger.Error($"Transaction rolled back for user: {reservation.Username}, seat: {reservation.SeatId}");
           throw;
        }
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
            "SELECT id, seat_id, user_id, start_time, end_time, is_confirmed FROM reservation WHERE room_id=@room_id", conn);
        cmd.Parameters.AddWithValue("@room_id", roomId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Reservation(
                reader.GetInt32("seat_id"),
                reader.GetInt32("user_id"),
                reader.GetDateTime("start_time"), 
                reader.GetDateTime("end_time"),
                reader.GetBoolean("is_confirmed")));
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
            "SELECT id, seat_id, user_id, start_time, end_time, is_confirmed FROM reservation", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Reservation(
                reader.GetInt32("seat_id"),
                reader.GetInt32("user_id"),
                reader.GetDateTime("start_time"), 
                reader.GetDateTime("end_time"),
                reader.GetBoolean("is_confirmed")));
        }

        return list;
    }
    
    /// <summary>
    /// Gets list of all reservations and information about them in specific time range.
    /// </summary>
    public List<dynamic> GetReservationsByTimeRange(DateTime startTime, DateTime endTime)
    {
        var reservations = new List<dynamic>();

        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        Logger.Info("GetReservationsByTimeRange called with:");
        Logger.Info($"  startTime: {startTime:yyyy-MM-dd HH:mm:ss}");
        Logger.Info($"  endTime: {endTime:yyyy-MM-dd HH:mm:ss}");

        var query = @"SELECT r.id, r.seat_id as seatId,s.room_id as roomId, r.start_time as startTime, r.end_time as endTime, u.username
            FROM reservation r JOIN seat s ON r.seat_id = s.id JOIN my_user u ON r.user_id = u.id AND r.start_time < @endTime 
            AND r.end_time > @startTime ORDER BY r.start_time";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@startTime", startTime);
        cmd.Parameters.AddWithValue("@endTime", endTime);

        Logger.Debug($"Executing query: {query}");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var reservation = new
            {
                id = reader.GetInt32("id"),
                seatId = reader.GetInt32("seatId"),
                roomId = reader.GetInt32("roomId"),
                startTime = reader.GetDateTime("startTime"),
                endTime = reader.GetDateTime("endTime"),
                username = reader.GetString("username")
            };
            
            Logger.Info($"Found reservation: Seat {reservation.seatId}, Room {reservation.roomId}, {reservation.startTime:yyyy-MM-dd HH:mm:ss} - {reservation.endTime:yyyy-MM-dd HH:mm:ss}");
            
            reservations.Add(reservation);
        }

        Logger.Info($"Total reservations found: {reservations.Count}");
        return reservations;
    }
    
    /// <summary>
    /// Gets all reservations with user and room information.
    /// </summary>
    public List<ReservationViewModel> GetAllReservationsWithDetails()
    {
        var reservations = new List<ReservationViewModel>();
        
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        
        try
        {
            conn.Open();
            Console.WriteLine("Database connection opened successfully");

            using var cmd = new MySqlCommand(@"
                SELECT r.id, r.seat_id, r.start_time, r.end_time, r.is_confirmed, u.username, u.role,
                       s.room_id, rm.name AS room_name
                FROM reservation r
                JOIN my_user u ON r.user_id = u.id
                JOIN seat s ON r.seat_id = s.id
                JOIN room rm ON s.room_id = rm.id
                ORDER BY r.start_time DESC", conn);

            using var reader = cmd.ExecuteReader();
            
            int count = 0;
            while (reader.Read())
            {
                count++;
                try
                {
                    var reservation = new ReservationViewModel
                    {
                        Id = reader.GetInt32("id"),
                        SeatId = reader.GetInt32("seat_id"),
                        Username = reader.GetString("username"),
                        UserRole = reader.GetString("role"),
                        RoomId = reader.GetInt32("room_id"),
                        RoomName = reader.GetString("room_name"),
                        StartTime = reader.GetDateTime("start_time"),
                        EndTime = reader.GetDateTime("end_time"),
                        IsConfirmed = reader.GetBoolean("is_confirmed")
                    };
                    
                    reservations.Add(reservation);
                    Console.WriteLine($"Loaded reservation {reservation.Id} for {reservation.Username}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading row {count}: {ex.Message}");
                    throw;
                }
            }
            
            Console.WriteLine($"Total reservations loaded: {reservations.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
        return reservations;
    }


    /// <summary>
    /// Deletes a reservation by ID.
    /// </summary>
    public bool DeleteReservation(int reservationId)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand("DELETE FROM reservation WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", reservationId);

        int rowsAffected = cmd.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Updates a reservation's time.
    /// </summary>
    public bool UpdateReservation(int reservationId, DateTime newStart, DateTime newEnd)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();
        
        using var checkCmd = new MySqlCommand(@"
            SELECT COUNT(*) FROM reservation 
            WHERE id != @id 
              AND seat_id = (SELECT seat_id FROM reservation WHERE id = @id)
              AND ((start_time < @end_time) AND (end_time > @start_time))", conn);
        
        checkCmd.Parameters.AddWithValue("@id", reservationId);
        checkCmd.Parameters.AddWithValue("@start_time", newStart);
        checkCmd.Parameters.AddWithValue("@end_time", newEnd);

        var conflicts = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (conflicts > 0)
            throw new InvalidOperationException("New time conflicts with existing reservation");
        
        using var cmd = new MySqlCommand(@"
            UPDATE reservation 
            SET start_time = @start, end_time = @end 
            WHERE id = @id", conn);
        
        cmd.Parameters.AddWithValue("@id", reservationId);
        cmd.Parameters.AddWithValue("@start", newStart);
        cmd.Parameters.AddWithValue("@end", newEnd);

        int rowsAffected = cmd.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Confirms a reservation.
    /// </summary>
    public bool ConfirmReservation(int reservationId)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(@"
            UPDATE reservation 
            SET is_confirmed = TRUE 
            WHERE id = @id", conn);
        
        cmd.Parameters.AddWithValue("@id", reservationId);

        int rowsAffected = cmd.ExecuteNonQuery();
        return rowsAffected > 0;
    }
}

public class ReservationViewModel
{
    public int Id { get; set; }
    public int SeatId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsConfirmed { get; set; }
}


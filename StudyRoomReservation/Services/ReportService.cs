namespace StudyRoomReservation.Services;
using MySql.Data.MySqlClient;

/// <summary>
/// Class generating reports from database data.
/// </summary>
public class ReportService
{
    private readonly string _connectionString = DatabaseConfig.ConnectionString;

    /// <summary>
    /// Generating summary numbers from reservations, equipment, users and seats.
    /// </summary>
    /// <returns>Summary report</returns>
    public ReservationSummaryReport GetReservationSummaryReport()
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();

        return new ReservationSummaryReport
        {
            TotalReservations = ExecuteScalar<int>(conn, "SELECT COUNT(*) FROM reservation"),
            TotalUsers = ExecuteScalar<int>(conn, "SELECT COUNT(DISTINCT user_id) FROM reservation"),
            TotalRooms = ExecuteScalar<int>(conn, "SELECT COUNT(*) FROM room"),
            TotalSeats = ExecuteScalar<int>(conn, "SELECT COUNT(*) FROM seat"),
            ConfirmedReservations = ExecuteScalar<int>(conn, "SELECT COUNT(*) FROM reservation WHERE is_confirmed = TRUE"),
            PendingReservations = ExecuteScalar<int>(conn, "SELECT COUNT(*) FROM reservation WHERE is_confirmed = FALSE"),
            RoomStatistics = GetRoomStats(conn),
            UserStatistics = GetUserStats(conn),
            EquipmentStatistics = GetEquipmentStats(conn)
        };
    }

    /// <summary>
    ///  Executes a SQL query and returns the first column of the first row.
    /// </summary>
    /// <param name="conn">Db connenction</param>
    /// <param name="query">Db query</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T ExecuteScalar<T>(MySqlConnection conn, string query)
    {
        using var cmd = new MySqlCommand(query, conn);
        var result = cmd.ExecuteScalar();
        return result == null || result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>
    /// Getting room statistics
    /// </summary>
    /// <param name="conn">Db connection</param>
    /// <returns></returns>
    private List<RoomStatistic> GetRoomStats(MySqlConnection conn)
    {
        var stats = new List<RoomStatistic>();
        using var cmd = new MySqlCommand(@"
            SELECT rm.id, rm.name, rm.capacity, rm.floor, COUNT(s.id) as seatCount,
                   COUNT(r.id) as reservationCount, SUM(IF(r.is_confirmed = TRUE, 1, 0)) as confirmedCount
            FROM room rm
            LEFT JOIN seat s ON rm.id = s.room_id
            LEFT JOIN reservation r ON s.id = r.seat_id
            GROUP BY rm.id", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            stats.Add(new RoomStatistic
            {
                RoomId = reader.GetInt32(0),
                RoomName = reader.GetString(1),
                Capacity = reader.GetInt32(2),
                Floor = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                SeatCount = reader.GetInt32(4),
                ReservationCount = reader.GetInt32(5),
                ConfirmedCount = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
            });
        }
        return stats;
    }

    /// <summary>
    /// Getting user statistics
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    private List<UserStatistic> GetUserStats(MySqlConnection conn)
    {
        var stats = new List<UserStatistic>();
        using var cmd = new MySqlCommand(@"
            SELECT u.id, u.username, u.role, COUNT(r.id) as reservationCount,
                   SUM(IF(r.is_confirmed = TRUE, 1, 0)) as confirmedCount
            FROM my_user u
            LEFT JOIN reservation r ON u.id = r.user_id
            GROUP BY u.id
            ORDER BY reservationCount DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            stats.Add(new UserStatistic
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                Role = reader.GetString(2),
                ReservationCount = reader.GetInt32(3),
                ConfirmedCount = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
            });
        }
        return stats;
    }

    /// <summary>
    /// Getting equipment statistics
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    private List<EquipmentStatistic> GetEquipmentStats(MySqlConnection conn)
    {
        var stats = new List<EquipmentStatistic>();
        using var cmd = new MySqlCommand(@"
            SELECT e.id, e.name, COUNT(DISTINCT rm.id) as roomCount
            FROM equipment e
            LEFT JOIN room_equipment re ON e.id = re.equipment_id
            LEFT JOIN room rm ON re.room_id = rm.id
            GROUP BY e.id", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            stats.Add(new EquipmentStatistic
            {
                EquipmentId = reader.GetInt32(0),
                EquipmentName = reader.GetString(1),
                RoomCount = reader.GetInt32(2)
            });
        }
        return stats;
    }
}

/// <summary>
/// Models
/// </summary>
public class ReservationSummaryReport
{
    public int TotalReservations { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRooms { get; set; }
    public int TotalSeats { get; set; }
    public int ConfirmedReservations { get; set; }
    public int PendingReservations { get; set; }
    public List<RoomStatistic> RoomStatistics { get; set; } = new();
    public List<UserStatistic> UserStatistics { get; set; } = new();
    public List<EquipmentStatistic> EquipmentStatistics { get; set; } = new();
}

public class RoomStatistic
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int? Floor { get; set; }
    public int SeatCount { get; set; }
    public int ReservationCount { get; set; }
    public int ConfirmedCount { get; set; }
}

public class UserStatistic
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int ReservationCount { get; set; }
    public int ConfirmedCount { get; set; }
}

public class EquipmentStatistic
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public int RoomCount { get; set; }
}
using System.Data;

namespace StudyRoomReservation.Repository;
using MySql.Data.MySqlClient;

/// <summary>
/// Repository handling saving rooms into database.
/// </summary>
public class RoomRepository
{
    /// <summary>
    /// Adds new room to database.
    /// </summary>
    /// <param name="room">Room with parameters - name, capacity</param>
    public int AddRoom(Room room)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "INSERT INTO room (name, capacity) VALUES (@name, @capacity); SELECT LAST_INSERT_ID();",
            conn);

        cmd.Parameters.AddWithValue("@name", room.Name);
        cmd.Parameters.AddWithValue("@capacity", room.Capacity);

        int newId = Convert.ToInt32(cmd.ExecuteScalar());
        room.Id = newId;
        
        foreach (var seat in room.Seats)
        {
            using var seatCmd = new MySqlCommand(
                "INSERT INTO seat (room_id) VALUES (@room_id); SELECT LAST_INSERT_ID();",
                conn);

            seatCmd.Parameters.AddWithValue("@room_id", newId);

            int seatId = Convert.ToInt32(seatCmd.ExecuteScalar());
            seat.Id = seatId;
        }

        return newId;
    }

    
    /// <summary>
    /// Gets information about specific room and seats from database.
    /// </summary>
    /// <param name="id">Room's id</param>
    /// <returns>Returns object Room</returns>
    public Room? GetRoomById(int id)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        Room? room = null;
        
        using (var cmd = new MySqlCommand(
                   "SELECT id, name, capacity FROM room WHERE id=@id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                room = new Room
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Capacity = reader.GetInt32("capacity"),
                    Seats = new List<Seat>()
                };
            }
        }

        if (room == null) return null;
        using (var seatCmd = new MySqlCommand(
                   "SELECT id, is_reserved, reserved_by FROM seat WHERE room_id=@room_id", conn))
        {
            seatCmd.Parameters.AddWithValue("@room_id", id);

            using var seatReader = seatCmd.ExecuteReader();
            while (seatReader.Read())
            {
                room.Seats.Add(new Seat
                {
                    Id = seatReader.GetInt32("id"),
                    IsReserved = seatReader.GetBoolean("is_reserved"),
                    ReservedBy = seatReader.IsDBNull("reserved_by") ? null : seatReader.GetString("reserved_by")
                });
            }
        }

        return room;
    }

    
    /// <summary>
    /// Gets list of all rooms and information about them.
    /// </summary>
    /// <returns>All saved rooms</returns>
    public List<Room> GetAllRooms()
    {
        var rooms = new List<Room>();

        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();
        
        using (var cmd = new MySqlCommand("SELECT id, name, capacity FROM room", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                rooms.Add(new Room
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Capacity = reader.GetInt32("capacity"),
                });
            }
        }
        
        foreach (var room in rooms)
        {
            using var seatCmd = new MySqlCommand("SELECT id FROM seat WHERE room_id=@room_id", conn);
            seatCmd.Parameters.AddWithValue("@room_id", room.Id);

            using var seatReader = seatCmd.ExecuteReader();
            while (seatReader.Read())
            {
                room.Seats.Add(new Seat
                {
                    Id = seatReader.GetInt32("id"),
                    IsReserved = false,
                    ReservedBy = null
                });
            }
        }

        return rooms;
    }

    
}
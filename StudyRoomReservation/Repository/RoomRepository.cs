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
            seat.RoomId = newId;
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
                   "SELECT id, name, capacity, floor FROM room WHERE id=@id", conn))
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
                    Floor = reader.IsDBNull("floor") ? null : reader.GetInt32("floor"),
                    Seats = new List<Seat>(),
                    Equipment = new List<Equipment>()
                };
            }
        }

        if (room == null) return null;
        
        // Get seats
        using (var seatCmd = new MySqlCommand(
                   "SELECT id FROM seat WHERE room_id=@room_id", conn))
        {
            seatCmd.Parameters.AddWithValue("@room_id", id);

            using var seatReader = seatCmd.ExecuteReader();
            while (seatReader.Read())
            {
                room.Seats.Add(new Seat
                {
                    Id = seatReader.GetInt32("id"),
                    RoomId = id
                });
            }
        }
        
        // Get equipment
        using (var equipCmd = new MySqlCommand(@"
            SELECT e.id, e.name 
            FROM room_equipment re
            JOIN equipment e ON re.equipment_id = e.id
            WHERE re.room_id = @room_id", conn))
        {
            equipCmd.Parameters.AddWithValue("@room_id", id);

            using var equipReader = equipCmd.ExecuteReader();
            while (equipReader.Read())
            {
                room.Equipment.Add(new Equipment
                {
                    Id = equipReader.GetInt32("id"),
                    Name = equipReader.GetString("name")
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
        
        using (var cmd = new MySqlCommand("SELECT id, name, capacity, floor FROM room", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                rooms.Add(new Room
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Capacity = reader.GetInt32("capacity"),
                    Floor = reader.IsDBNull("floor") ? null : reader.GetInt32("floor"),
                    Seats = new List<Seat>(),
                    Equipment = new List<Equipment>()
                });
            }
        }
        
        var seatMap = new Dictionary<int, List<Seat>>(); // key = roomId
        using (var seatCmd = new MySqlCommand("SELECT id, room_id FROM seat", conn))
        using (var seatReader = seatCmd.ExecuteReader())
        {
            while (seatReader.Read())
            {
                int roomId = seatReader.GetInt32("room_id");
                var seat = new Seat { Id = seatReader.GetInt32("id"), RoomId = roomId };

                if (!seatMap.ContainsKey(roomId))
                    seatMap[roomId] = new List<Seat>();

                seatMap[roomId].Add(seat);
            }
        }
        
        var equipmentMap = new Dictionary<int, List<Equipment>>();
        using (var equipCmd = new MySqlCommand(@"
            SELECT re.room_id, e.id, e.name 
            FROM room_equipment re
            JOIN equipment e ON re.equipment_id = e.id", conn))
        using (var equipReader = equipCmd.ExecuteReader())
        {
            while (equipReader.Read())
            {
                int roomId = equipReader.GetInt32("room_id");
                var equipment = new Equipment
                {
                    Id = equipReader.GetInt32("id"),
                    Name = equipReader.GetString("name")
                };

                if (!equipmentMap.ContainsKey(roomId))
                    equipmentMap[roomId] = new List<Equipment>();

                equipmentMap[roomId].Add(equipment);
            }
        }
        
        foreach (var room in rooms)
        {
            if (seatMap.ContainsKey(room.Id))
                room.Seats = seatMap[room.Id];
            
            if (equipmentMap.ContainsKey(room.Id))
                room.Equipment = equipmentMap[room.Id];
        }

        return rooms;
    }
    
}
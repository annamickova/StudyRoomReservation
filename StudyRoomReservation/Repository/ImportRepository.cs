namespace StudyRoomReservation.Repository;
using MySql.Data.MySqlClient;

public class ImportRepository
{
    /// <summary>
    /// Imports rooms from CSV text.
    /// Format: name,capacity,floor
    /// </summary>
    public int ImportRoomsFromCsv(string csvContent)
    {
        var lines = csvContent.Split('\n');
        int count = 0;

        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("name"))
                    continue;

                var parts = trimmed.Split(',');
                if (parts.Length < 2) continue;

                string name = parts[0].Trim().Trim('"');
                int capacity = int.Parse(parts[1].Trim());
                int? floor = parts.Length > 2 ? int.Parse(parts[2].Trim()) : null;

                using var cmd = new MySqlCommand(
                    "INSERT INTO room (name, capacity, floor) VALUES (@name, @capacity, @floor)",
                    conn, transaction);
                
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@capacity", capacity);
                cmd.Parameters.AddWithValue("@floor", floor.HasValue ? floor.Value : DBNull.Value);
                cmd.ExecuteNonQuery();

                int roomId = (int)cmd.LastInsertedId;

                for (int i = 0; i < capacity; i++)
                {
                    using var seatCmd = new MySqlCommand(
                        "INSERT INTO seat (room_id) VALUES (@room_id)", conn, transaction);
                    seatCmd.Parameters.AddWithValue("@room_id", roomId);
                    seatCmd.ExecuteNonQuery();
                }

                count++;
            }

            transaction.Commit();
            Logger.Info($"Imported {count} rooms from CSV");
            return count;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Logger.Error("CSV import failed");
            throw new InvalidOperationException($"Import failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Imports equipment from CSV text.
    /// Format: name (one per line)
    /// </summary>
    public int ImportEquipmentFromCsv(string csvContent)
    {
        var lines = csvContent.Split('\n');
        int count = 0;

        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            foreach (var line in lines)
            {
                var trimmed = line.Trim().Trim('"');
                if (string.IsNullOrEmpty(trimmed) || trimmed.ToLower() == "name")
                    continue;

                using var checkCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM equipment WHERE name = @name", conn, transaction);
                checkCmd.Parameters.AddWithValue("@name", trimmed);
                
                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    continue; // Skip duplicates

                // Insert equipment
                using var cmd = new MySqlCommand(
                    "INSERT INTO equipment (name) VALUES (@name)", conn, transaction);
                cmd.Parameters.AddWithValue("@name", trimmed);
                cmd.ExecuteNonQuery();

                count++;
            }

            transaction.Commit();
            Logger.Info($"Imported {count} equipment items from CSV");
            return count;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Logger.Error("Equipment import failed");
            throw new InvalidOperationException($"Import failed: {ex.Message}");
        }
    }
}
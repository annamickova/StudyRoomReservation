namespace StudyRoomReservation.Repository;
using MySql.Data.MySqlClient;

public class UserRepository
{
    public User? GetUserByUsername(string username)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand("SELECT id, username, role FROM my_user WHERE username=@username", conn);
        cmd.Parameters.AddWithValue("@username", username);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                Role = Enum.Parse<UserRole>(reader.GetString("role"))
            };
        }

        return null;
    }

    public int AddUser(User user)
    {
        using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "INSERT INTO my_user (username, role) VALUES (@username, @role); SELECT LAST_INSERT_ID();", conn);

        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@role", user.Role.ToString());

        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
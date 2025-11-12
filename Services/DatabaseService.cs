using Npgsql;
using AndenStemesterEksamensProjekt.Models;

namespace AndenStemesterEksamensProjekt.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// Get a user by email address
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT uid, email, password_hash, created_at, updated_at, last_login, is_active FROM users WHERE email = @email AND is_active = true",
                connection);
            
            command.Parameters.AddWithValue("@email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Uid = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    UpdatedAt = reader.GetDateTime(4),
                    LastLogin = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6)
                };
            }

            return null;
        }

        /// <summary>
        /// Update last login timestamp for a user
        /// </summary>
        public async Task UpdateLastLoginAsync(int uid)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "UPDATE users SET last_login = @lastLogin WHERE uid = @uid",
                connection);
            
            command.Parameters.AddWithValue("@lastLogin", DateTime.UtcNow);
            command.Parameters.AddWithValue("@uid", uid);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Create a new user with hashed password
        /// </summary>
        public async Task<int> CreateUserAsync(string email, string passwordHash)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "INSERT INTO users (email, password_hash) VALUES (@email, @passwordHash) RETURNING uid",
                connection);
            
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);

            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}

using BCrypt.Net;
using Npgsql;

namespace Repository.service
{
    public class ForgotService : IForgotService
    {
        private readonly NpgsqlConnection conn;

        public ForgotService(NpgsqlConnection _conn)
        {
            conn = _conn;
            
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string query = @"SELECT 1 FROM t_customer WHERE c_email = @email LIMIT 1;";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            await conn.CloseAsync();

            return result != null;
        }

        public async Task<bool> UpdatePasswordAsync(string email, string newPassword)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            const string query = @"
                UPDATE t_customer
                SET c_password = @password
                WHERE c_email = @email;
            ";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.Parameters.AddWithValue("@email", email);

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            int rows = await cmd.ExecuteNonQueryAsync();

            await conn.CloseAsync();

            return rows > 0;
        }
    }
}

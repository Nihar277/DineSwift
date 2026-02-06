using Npgsql;
using BCrypt.Net;

namespace Repository.service;

public class PasswordRepository : IPasswordRepository
{
    private readonly NpgsqlConnection _conn;

    public PasswordRepository(NpgsqlConnection conn)
    {
        _conn = conn;
    }

    public async Task<bool> VerifyCurrentPasswordAsync(int customerId, string currentPassword)
    {
        try
        {
            await _conn.OpenAsync();
            

            var query = @"SELECT c_password FROM t_customer WHERE c_customerid = @customerId";

            using var cmd = new NpgsqlCommand(query, _conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);

            var storedHash = await cmd.ExecuteScalarAsync() as string;

            Console.WriteLine($"Entered password: '{currentPassword}'");
            Console.WriteLine($"Stored hash: '{storedHash}'");
            Console.WriteLine($"Entered length: {currentPassword.Length}");
            Console.WriteLine($"Hash length: {storedHash?.Length}");

            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            // Correct BCrypt verification
            return BCrypt.Net.BCrypt.Verify(currentPassword, storedHash);
        }
        finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                await _conn.CloseAsync();
        }
    }

    public async Task<bool> ChangePasswordAsync(int customerId, string newHashedPassword)
    {
        try
        {
            await _conn.OpenAsync();

            var query = @"UPDATE t_customer SET c_password = @password, c_updated_time = NOW() WHERE c_customerid = @customerId";

            using var cmd = new NpgsqlCommand(query, _conn);
            cmd.Parameters.AddWithValue("@password", newHashedPassword);
            cmd.Parameters.AddWithValue("@customerId", customerId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                await _conn.CloseAsync();
        }
    }
}

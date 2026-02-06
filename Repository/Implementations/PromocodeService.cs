using Npgsql;
using Repository.Model;

namespace Repository.service
{
    public class PromocodeService : IPromocodeService
    {
        private readonly NpgsqlConnection _conn;

        public PromocodeService(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        // 🔹 Insert customer promocode usage
        public async Task<bool> SendPromocodeAsync(t_promocode promocode)
        {
            try
            {
                
                var query = @"INSERT INTO t_promocode 
                              (c_customerid, c_email, c_promocode) 
                              VALUES (@customerid, @email, @promocode)";

                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("customerid", promocode.c_customerid);
                cmd.Parameters.AddWithValue("email", promocode.c_email);
                cmd.Parameters.AddWithValue("promocode", promocode.c_promocode);

                await _conn.OpenAsync();
                var result = await cmd.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving promocode: " + ex.Message);
                return false;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        // 🔹 Check promo usage limit (max 2)
        public async Task<bool> VerifyPromocodeAsync(string promocode)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM t_promocode WHERE c_promocode = @promocode";
                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("promocode", promocode);

                await _conn.OpenAsync();
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // ✅ allow only if used less than 2 times
                return count < 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error verifying promocode usage: " + ex.Message);
                return false;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        // 🔹 Insert master promo
        public async Task<bool> SendPromoAsync(string promo)
        {
            try
            {
                var query = "INSERT INTO t_promo (c_promocode) VALUES (@promocode)";
                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("promocode", promo);

                await _conn.OpenAsync();
                var result = await cmd.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving promo: " + ex.Message);
                return false;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        // 🔹 Check promo exists
        public async Task<bool> VerifyPromoAsync(string promo)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM t_promo WHERE c_promocode = @promocode";
                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("promocode", promo);

                await _conn.OpenAsync();
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // ✅ valid only if exists
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error verifying promo: " + ex.Message);
                return false;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }
    }
}

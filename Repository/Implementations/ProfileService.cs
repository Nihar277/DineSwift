using Npgsql;
using API.Services;
using Repository.Model;
using System.Threading.Tasks;

namespace API.Services;

public class ProfileService : IProfileService
{
    private readonly NpgsqlConnection _conn;
    public ProfileService(NpgsqlConnection conn)
    {
        _conn = conn;
    }

    public async Task<t_customer?> GetProfileById(int customerId)
        {
            try
            {
                
                var query = @"SELECT 
                                c_customerid,
                                c_fname,
                                c_lname,
                                c_state,
                                c_city,
                                c_pincode,
                                c_gender,
                                c_address,
                                c_image,
                                c_email,
                                c_phonenumber
                              FROM t_customer
                             WHERE c_customerid = @id AND c_status = 'active'";

                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@id", customerId);

                await _conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new t_customer
                    {
                        c_customerid = reader.GetInt32(0),
                        c_fname = reader.GetString(1),
                        c_lname = reader.GetString(2),
                        c_state = reader.GetString(3),
                        c_city = reader.GetString(4),
                        c_pincode = reader.GetString(5),
                        c_gender = reader.GetString(6),
                        c_address = reader.GetString(7),
                        c_image = reader.IsDBNull(8) ? null : reader.GetString(8),
                        c_email = reader.GetString(9),
                        c_phonenumber = reader.GetString(10)
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetProfile error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return null;
        }


          public async Task<bool> UpdateProfile(t_customer customer)
        {
            try
            {
                var query = @"
                UPDATE t_customer SET
                    c_fname = @c_fname,
                    c_lname = @c_lname,
                    c_state = @c_state,
                    c_city = @c_city,
                    c_pincode = @c_pincode,
                    c_gender = @c_gender,
                    c_address = @c_address,
                    c_image = @c_image,
                    c_phonenumber = @c_phonenumber,
                    c_updated_time = CURRENT_TIMESTAMP
                WHERE c_customerid = @c_customerid";

                using var cmd = new NpgsqlCommand(query, _conn);

                cmd.Parameters.AddWithValue("@c_customerid", customer.c_customerid);
                cmd.Parameters.AddWithValue("@c_fname", customer.c_fname ?? "");
                cmd.Parameters.AddWithValue("@c_lname", customer.c_lname ?? "");
                cmd.Parameters.AddWithValue("@c_state", customer.c_state ?? "");
                cmd.Parameters.AddWithValue("@c_city", customer.c_city ?? "");
                cmd.Parameters.AddWithValue("@c_pincode", customer.c_pincode ?? "");
                cmd.Parameters.AddWithValue("@c_gender", customer.c_gender ?? "");
                cmd.Parameters.AddWithValue("@c_address", customer.c_address ?? "");
                cmd.Parameters.AddWithValue("@c_image", customer.c_image ?? "");
                cmd.Parameters.AddWithValue("@c_phonenumber", customer.c_phonenumber ?? "");

                await _conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateProfile error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        public async Task<bool> SoftDeleteAccount(int customerId)
{
    try
    {
        var query = @"
            UPDATE t_customer
            SET 
                c_status = 'inactive',
                c_updated_time = CURRENT_TIMESTAMP
            WHERE c_customerid = @id";

        using var cmd = new NpgsqlCommand(query, _conn);
        cmd.Parameters.AddWithValue("@id", customerId);

        await _conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine("SoftDeleteAccount error: " + ex.Message);
        return false;
    }
    finally
    {
        await _conn.CloseAsync();
    }
}


}


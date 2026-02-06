using Npgsql;
using Repository.Model;

namespace Repository.service
{
    public class RestaurantRegisterService : IRestaurantRegisterService
    {
        private readonly NpgsqlConnection _conn;

        public RestaurantRegisterService(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        // 🔹 Email exist check (NO open/close here)
        public async Task<bool> IsEmailExists(string email)
        {
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open)
                    await _conn.OpenAsync();
                    

                var qry = "SELECT COUNT(1) FROM t_customer WHERE c_email = @email";
                await using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@email", email);

                var count = (long)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("IsEmailExists Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // 🔹 Transaction based register
        public async Task<bool> RegisterVendor(t_customer customer, t_restaurant restaurant)
        {
            await _conn.OpenAsync();


            try
            {
                // 1️⃣ CUSTOMER INSERT
                var customerQry = @"
                    INSERT INTO t_customer
                    (
                      c_fname, c_lname, c_state, c_city, c_pincode, c_gender,
                      c_address, c_image, c_email, c_password, c_phonenumber,
                      c_role, c_created_time, c_updated_time, c_status
                    )
                    VALUES
                    (
                      @c_fname, @c_lname, @c_state, @c_city, @c_pincode, @c_gender,
                      @c_address, @c_image, @c_email, @c_password, @c_phonenumber,
                      @c_role, NOW(), NOW(), @c_status  
                    )
                    RETURNING c_customerid;
                    ";

                var custCmd = new NpgsqlCommand(customerQry, _conn);

                custCmd.Parameters.AddWithValue("@c_fname", customer.c_fname);
                custCmd.Parameters.AddWithValue("@c_lname", customer.c_lname);
                custCmd.Parameters.AddWithValue("@c_state", customer.c_state);
                custCmd.Parameters.AddWithValue("@c_city", customer.c_city);
                custCmd.Parameters.AddWithValue("@c_pincode", customer.c_pincode);
                custCmd.Parameters.AddWithValue("@c_gender", customer.c_gender);
                custCmd.Parameters.AddWithValue("@c_address", customer.c_address);
                custCmd.Parameters.AddWithValue("@c_image", customer.c_image ?? "");
                custCmd.Parameters.AddWithValue("@c_email", customer.c_email);
                custCmd.Parameters.AddWithValue("@c_password", customer.c_password);
                custCmd.Parameters.AddWithValue("@c_phonenumber", customer.c_phonenumber);
                custCmd.Parameters.AddWithValue("@c_role", "r");
                custCmd.Parameters.AddWithValue("@c_status", "inactive");

                int customerId = (int)await custCmd.ExecuteScalarAsync();

                // 2️⃣ RESTAURANT INSERT
                var restaurantQry = @"
                        INSERT INTO t_restaurant
                        (
                          c_r_name, c_r_email, c_r_address, c_r_state, c_r_city,
                          c_r_ac, c_r_ifsc, c_r_gst, c_r_aadhar, c_r_image,
                          c_r_isavailable, c_status, c_customerid, c_created_at
                        )
                        VALUES
                        (
                          @c_r_name, @c_r_email, @c_r_address, @c_r_state, @c_r_city,
                          @c_r_ac, @c_r_ifsc, @c_r_gst, @c_r_aadhar, @c_r_image,
                          @c_r_isavailable, @c_status, @c_customerid, NOW()
                        );
                        ";

                var restCmd = new NpgsqlCommand(restaurantQry, _conn);


                restCmd.Parameters.AddWithValue("@c_r_name", restaurant.c_r_name);
                restCmd.Parameters.AddWithValue("@c_r_email", restaurant.c_r_email);
                restCmd.Parameters.AddWithValue("@c_r_address", restaurant.c_r_address);
                restCmd.Parameters.AddWithValue("@c_r_state", restaurant.c_r_state);
                restCmd.Parameters.AddWithValue("@c_r_city", restaurant.c_r_city);
                restCmd.Parameters.AddWithValue("@c_r_ac", restaurant.c_r_ac);
                restCmd.Parameters.AddWithValue("@c_r_ifsc", restaurant.c_r_ifsc);
                restCmd.Parameters.AddWithValue("@c_r_gst", restaurant.c_r_gst);
                restCmd.Parameters.AddWithValue("@c_r_aadhar", restaurant.c_r_aadhar ?? "");
                restCmd.Parameters.AddWithValue("@c_r_image", restaurant.c_r_image ?? "");
                restCmd.Parameters.AddWithValue("@c_r_isavailable", "no");
                restCmd.Parameters.AddWithValue("@c_status", "active");
                restCmd.Parameters.AddWithValue("@c_customerid", customerId);

                await restCmd.ExecuteNonQueryAsync();


                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
    }
}

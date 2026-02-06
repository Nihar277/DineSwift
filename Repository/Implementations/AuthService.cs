using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repository.Model;


namespace Repository.Service
{
    public class AuthService : IAuthService
    {
        private readonly NpgsqlConnection _conn;

        public AuthService(NpgsqlConnection conn)
        {
            _conn = conn;
        }
        public async Task<t_customer> Login(login_vm login_vm)
        {
            try
            {
                await _conn.OpenAsync();

                string loginStr = @"SELECT c_customerid, c_fname, c_lname, c_state, c_city, c_pincode, c_gender, c_address, c_image, c_email, c_password, c_phonenumber, c_role, c_created_time, c_updated_time, c_status
	FROM public.t_customer WHERE c_email = @email AND c_status = 'active';";

                var cmd = new NpgsqlCommand(loginStr, _conn);
                cmd.Parameters.AddWithValue("email", login_vm.c_email);
                var rd = await cmd.ExecuteReaderAsync();

                if (await rd.ReadAsync())
                {
                    string hashedPassword = (string)rd["c_password"];

                    Console.WriteLine("Hash pass -- " + hashedPassword);
                    Console.WriteLine("pass -- " + (string)rd["c_password"]);
                    // bool isValid = BCrypt.Net.BCrypt.Verify("MyPassword123", hashedPassword);
                    bool isValid = BCrypt.Net.BCrypt.Verify(login_vm.c_password, hashedPassword);
                    Console.WriteLine("is valid --" + isValid);

                    if (isValid)
                    {
                        return new t_customer()
                        {
                            c_customerid = (int)rd["c_customerid"],
                            c_fname = (string)rd["c_fname"],
                            c_lname = (string)rd["c_lname"],
                            c_state = (string)rd["c_state"],
                            c_city = (string)rd["c_city"],
                            c_pincode = (string)rd["c_pincode"],
                            c_gender = (string)rd["c_gender"],
                            c_address = (string)rd["c_address"],
                            c_image = (string)rd["c_image"],
                            c_email = (string)rd["c_email"],
                            c_phonenumber = (string)rd["c_phonenumber"],
                            c_role = (string)rd["c_role"],
                            c_created_time = (DateTime)rd["c_created_time"],
                            c_updated_time = (DateTime)rd["c_updated_time"],
                            c_status = (string)rd["c_status"],
                        };
                    }
                }

                return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Login Error --- " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<int> checkEmailExists(string email)
        {
            try
            {
                await _conn.OpenAsync();

                string loginStr = @"SELECT * FROM public.t_customer WHERE c_email = @email";

                var cmd = new NpgsqlCommand(loginStr, _conn);
                cmd.Parameters.AddWithValue("email", email);
                var rd = await cmd.ExecuteReaderAsync();

                if (await rd.ReadAsync())
                {
                    return 1;
                }

                return 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Login Error --- " + ex.Message);
                return 0;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

    }
}
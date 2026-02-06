using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Repository.Interfaces;
using Repository.Model;

namespace Repository.Services
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly NpgsqlConnection _conn;

        public RestaurantRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        // ===============================
        // GET ALL RESTAURANTS (NO RATING)
        // ===============================
        public async Task<List<t_vm_restaurant>> GetAllRestaurants()
        {
            List<t_vm_restaurant> restaurants = new List<t_vm_restaurant>();

            try
            {
                await _conn.CloseAsync();

                string query = @"
                    SELECT 
                        c_r_id,
                        c_r_name,
                        c_r_email,
                        c_r_address,
                        c_r_state,
                        c_r_city,
                        c_r_image,
                        c_r_isavailable,
                        c_status,
                        c_customerid,
                        c_created_at
                    FROM t_restaurant
                    WHERE 
                        c_status = 'active'
                        AND c_r_isavailable = 'yes'
                    ORDER BY c_created_at DESC;
                ";

                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    restaurants.Add(new t_vm_restaurant()
                    {
                        c_r_id = Convert.ToInt32(reader["c_r_id"]),
                        c_r_name = reader["c_r_name"].ToString(),
                        c_r_email = reader["c_r_email"].ToString(),
                        c_r_address = reader["c_r_address"].ToString(),
                        c_r_state = reader["c_r_state"].ToString(),
                        c_r_city = reader["c_r_city"].ToString(),
                        c_r_image = reader["c_r_image"].ToString(),
                        c_r_isavailable = reader["c_r_isavailable"].ToString(),
                        c_status = reader["c_status"].ToString(),
                        c_customerid = Convert.ToInt32(reader["c_customerid"]),
                        c_created_at = Convert.ToDateTime(reader["c_created_at"])
                    });
                }

                return restaurants;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error While fetching Restaurants: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // ===============================
        // GET RESTAURANT BY ID
        // ===============================
        public async Task<t_vm_restaurant> GetRestaurantById(int id)
        {
            t_vm_restaurant restaurant = null;

            try
            {
                await _conn.CloseAsync();

                string query = @"
                    SELECT 
                        c_r_id,
                        c_r_name,
                        c_r_email,
                        c_r_address,
                        c_r_state,
                        c_r_city,
                        c_r_image,
                        c_r_isavailable,
                        c_status,
                        c_customerid,
                        c_created_at
                    FROM t_restaurant
                    WHERE 
                        c_r_id = @id
                        AND c_status = 'active'
                        AND c_r_isavailable = 'yes';
                ";

                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("id", id);

                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    restaurant = new t_vm_restaurant()
                    {
                        c_r_id = Convert.ToInt32(reader["c_r_id"]),
                        c_r_name = reader["c_r_name"].ToString(),
                        c_r_email = reader["c_r_email"].ToString(),
                        c_r_address = reader["c_r_address"].ToString(),
                        c_r_state = reader["c_r_state"].ToString(),
                        c_r_city = reader["c_r_city"].ToString(),
                        c_r_image = reader["c_r_image"].ToString(),
                        c_r_isavailable = reader["c_r_isavailable"].ToString(),
                        c_status = reader["c_status"].ToString(),
                        c_customerid = Convert.ToInt32(reader["c_customerid"]),
                        c_created_at = Convert.ToDateTime(reader["c_created_at"])
                    };
                }

                return restaurant;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error While fetching Restaurant by ID: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

       


    }
}

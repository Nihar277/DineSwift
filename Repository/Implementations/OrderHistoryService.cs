using Npgsql;
using Repository.Model;
using System.Data;

namespace API.service
{
    public class OrderHistoryService : IOrderHistoryInterface
    {
        private readonly NpgsqlConnection _conn;

        public OrderHistoryService(NpgsqlConnection conn)
        {
            _conn = conn;
            
        }

        // ===============================
        // 1️⃣ GET ALL ORDER HISTORY
        // ===============================
        public async Task<List<t_order>> GetAllOrdersAsync(int c_customerid)
        {
            List<t_order> orders = new();

            try
            {
                string query = @"
                    SELECT *
                    FROM t_orders
                    WHERE c_customerid = @c_customerid
                    ORDER BY c_bookingid DESC";

                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_customerid", c_customerid);

                await _conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    orders.Add(new t_order
                    {
                        c_bookingid = reader.GetInt32(reader.GetOrdinal("c_bookingid")),
                        c_orderid = reader.GetString(reader.GetOrdinal("c_orderid")),
                        c_customerid = reader.GetInt32(reader.GetOrdinal("c_customerid")),
                        c_restaurantid = reader.GetInt32(reader.GetOrdinal("c_restaurantid")),
                        c_orderdate = reader.GetDateTime(reader.GetOrdinal("c_orderdate")),
                        c_ordertime = reader.GetTimeSpan(reader.GetOrdinal("c_ordertime")),
                        c_housenumber = reader.GetString(reader.GetOrdinal("c_housenumber")),
                        c_societyname = reader.GetString(reader.GetOrdinal("c_societyname")),
                        c_landmark = reader["c_landmark"] as string,
                        c_city = reader.GetString(reader.GetOrdinal("c_city")),
                        c_state = reader.GetString(reader.GetOrdinal("c_state")),
                        c_totalprice = reader.GetDecimal(reader.GetOrdinal("c_totalprice")),
                        c_quantity = reader.GetInt32(reader.GetOrdinal("c_quantity")),
                        c_foodimage = reader["c_foodimage"] as string,
                        c_orderstatus = reader.GetString(reader.GetOrdinal("c_orderstatus")),
                        c_dishname = reader.GetString(reader.GetOrdinal("c_dishname")),
                        c_fooditemid = reader.GetInt32(reader.GetOrdinal("c_fooditemid"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching order history ---> " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return orders;
        }

        // ===============================
        // 2️⃣ GET ORDER BY BOOKING ID
        // ===============================
        public async Task<t_order?> GetOrderByBookingIdAsync(int c_bookingid)
        {
            t_order? order = null;

            try
            {
                string query = @"
                    SELECT *
                    FROM t_orders
                    WHERE c_bookingid = @c_bookingid";

                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_bookingid", c_bookingid);

                await _conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    order = new t_order
                    {
                        c_bookingid = reader.GetInt32(reader.GetOrdinal("c_bookingid")),
                        c_orderid = reader.GetString(reader.GetOrdinal("c_orderid")),
                        c_customerid = reader.GetInt32(reader.GetOrdinal("c_customerid")),
                        c_restaurantid = reader.GetInt32(reader.GetOrdinal("c_restaurantid")),
                        c_orderdate = reader.GetDateTime(reader.GetOrdinal("c_orderdate")),
                        c_ordertime = reader.GetTimeSpan(reader.GetOrdinal("c_ordertime")),
                        c_housenumber = reader.GetString(reader.GetOrdinal("c_housenumber")),
                        c_societyname = reader.GetString(reader.GetOrdinal("c_societyname")),
                        c_landmark = reader["c_landmark"] as string,
                        c_city = reader.GetString(reader.GetOrdinal("c_city")),
                        c_state = reader.GetString(reader.GetOrdinal("c_state")),
                        c_totalprice = reader.GetDecimal(reader.GetOrdinal("c_totalprice")),
                        c_quantity = reader.GetInt32(reader.GetOrdinal("c_quantity")),
                        c_foodimage = reader["c_foodimage"] as string,
                        c_orderstatus = reader.GetString(reader.GetOrdinal("c_orderstatus")),
                        c_dishname = reader.GetString(reader.GetOrdinal("c_dishname")),
                        c_fooditemid = reader.GetInt32(reader.GetOrdinal("c_fooditemid"))
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching order by booking id ---> " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return order;
        }

        // ===============================
        // 3️⃣ GET ORDER PROGRESS BY BOOKING ID
        // ===============================
        public async Task<string?> GetOrderProgressAsync(int c_bookingid)
        {
            string? status = null;

            try
            {
                string query = @"
                    SELECT c_orderstatus
                    FROM t_orders
                    WHERE c_bookingid = @c_bookingid";

                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_bookingid", c_bookingid);

                await _conn.OpenAsync();
                status = (string?)await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching order progress ---> " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return status;
        }
    }
}
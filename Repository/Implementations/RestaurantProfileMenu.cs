using Npgsql;
using Repository.Model;

namespace Repository.service
{
    public class RestaurantProfileMenu : IRestaurantProfileMenu
    {
        private readonly NpgsqlConnection _conn;

        public RestaurantProfileMenu(NpgsqlConnection conn)
        {
            _conn = conn;
        }
        public async Task<(int CustomerId, string OrderId)> GetOrderCustomerInfo(int bookingId)
        {
            try
            {
                string qry = @"
            SELECT c_customerid, c_orderid
            FROM t_orders
            WHERE c_bookingid = @bid
        ";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@bid", bookingId);

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    return (
                        Convert.ToInt32(r["c_customerid"]),
                        r["c_orderid"]?.ToString() ?? ""
                    );
                }

                return (0, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrderCustomerInfo Error: " + ex.Message);
                return (0, "");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<(int CustomerId, int RestaurantId, string OrderId)> GetOrderInfo(int bookingId)
        {
            try
            {
                string qry = @"
            SELECT c_customerid, c_restaurantid, c_orderid
            FROM t_orders
            WHERE c_bookingid = @bid
        ";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@bid", bookingId);

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    return (
                        Convert.ToInt32(r["c_customerid"]),
                        Convert.ToInt32(r["c_restaurantid"]),
                        r["c_orderid"]?.ToString() ?? ""
                    );
                }

                return (0, 0, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrderInfo Error: " + ex.Message);
                return (0, 0, "");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<int> GetRestaurantIdByCustomer(int customerId)
        {
            string qry = "SELECT c_r_id FROM t_restaurant WHERE c_customerid = @cid";


            await _conn.OpenAsync();

            using var cmd = new NpgsqlCommand(qry, _conn);
            cmd.Parameters.AddWithValue("@cid", customerId);

            var result = await cmd.ExecuteScalarAsync();

            await _conn.CloseAsync();

            return result == null ? 0 : Convert.ToInt32(result);
        }

        public async Task<int> GetCustomerIdByRestaurantId(int restaurantId)
        {
            try
            {
                string qry = "SELECT c_customerid FROM t_restaurant WHERE c_r_id = @rid";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@rid", restaurantId);

                var result = await cmd.ExecuteScalarAsync();

                await _conn.CloseAsync();

                return result == null ? 0 : Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCustomerIdByRestaurantId Error: " + ex.Message);
                await _conn.CloseAsync();
                return 0;
            }
        }

        // 🔹 GET RESTAURANT PROFILE
        public async Task<t_restaurant?> GetRestaurantProfile(int customerId)
        {
            try
            {
                string qry = @"
                SELECT 
                    c_r_id, c_r_name, c_r_email, c_r_address,
                    c_r_state, c_r_city, c_r_ac, c_r_ifsc,
                    c_r_gst, c_r_aadhar, c_r_image,
                    c_r_isavailable, c_status, c_customerid
                FROM t_restaurant
                WHERE c_customerid = @c_customerid";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@c_customerid", customerId);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new t_restaurant
                    {
                        c_r_id = reader.GetInt32(reader.GetOrdinal("c_r_id")),
                        c_r_name = reader["c_r_name"]?.ToString(),
                        c_r_email = reader["c_r_email"]?.ToString(),
                        c_r_address = reader["c_r_address"]?.ToString(),
                        c_r_state = reader["c_r_state"]?.ToString(),
                        c_r_city = reader["c_r_city"]?.ToString(),
                        c_r_ac = reader.GetInt64(reader.GetOrdinal("c_r_ac")),
                        c_r_ifsc = reader["c_r_ifsc"]?.ToString(),
                        c_r_gst = reader["c_r_gst"]?.ToString(),
                        c_r_aadhar = reader["c_r_aadhar"]?.ToString(),
                        c_r_image = reader["c_r_image"]?.ToString(),
                        c_r_isavailable = reader["c_r_isavailable"]?.ToString(),
                        c_status = reader["c_status"]?.ToString(),
                        c_customerid = reader.GetInt32(reader.GetOrdinal("c_customerid")),

                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetRestaurantProfile Error: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // 🔹 UPDATE RESTAURANT PROFILE
        public async Task<bool> UpdateRestaurantProfile(t_restaurant restaurant)
        {
            try
            {
                string qry = @"
                UPDATE t_restaurant SET
                    c_r_name = @c_r_name,
                    c_r_email = @c_r_email,
                    c_r_address = @c_r_address,
                    c_r_state = @c_r_state,
                    c_r_city = @c_r_city,
                    c_r_ac = @c_r_ac,
                    c_r_ifsc = @c_r_ifsc,
                    c_r_gst = @c_r_gst,
                    c_r_image = @c_r_image,
                    c_r_aadhar = @c_r_aadhar
                WHERE c_r_id = @c_r_id";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);

                cmd.Parameters.AddWithValue("@c_r_id", restaurant.c_r_id);
                cmd.Parameters.AddWithValue("@c_r_name", restaurant.c_r_name ?? "");
                cmd.Parameters.AddWithValue("@c_r_email", restaurant.c_r_email ?? "");
                cmd.Parameters.AddWithValue("@c_r_address", restaurant.c_r_address ?? "");
                cmd.Parameters.AddWithValue("@c_r_state", restaurant.c_r_state ?? "");
                cmd.Parameters.AddWithValue("@c_r_city", restaurant.c_r_city ?? "");
                cmd.Parameters.AddWithValue("@c_r_ac", restaurant.c_r_ac ?? 0);
                cmd.Parameters.AddWithValue("@c_r_ifsc", restaurant.c_r_ifsc ?? "");
                cmd.Parameters.AddWithValue("@c_r_gst", restaurant.c_r_gst ?? "");
                cmd.Parameters.AddWithValue("@c_r_image", restaurant.c_r_image ?? "");
                cmd.Parameters.AddWithValue("@c_r_aadhar", restaurant.c_r_aadhar ?? "");

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateRestaurantProfile Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }



        public async Task<t_restaurant?> GetRestaurantById(int restaurantId)
        {
            string qry = @"SELECT c_r_image, c_r_aadhar FROM t_restaurant WHERE c_r_id=@id";

            await _conn.OpenAsync();
            using var cmd = new NpgsqlCommand(qry, _conn);
            cmd.Parameters.AddWithValue("@id", restaurantId);

            using var r = await cmd.ExecuteReaderAsync();
            if (!r.Read()) return null;

            var res = new t_restaurant
            {
                c_r_image = r["c_r_image"]?.ToString(),
                c_r_aadhar = r["c_r_aadhar"]?.ToString()
            };

            await _conn.CloseAsync();
            return res;
        }

        // ================= DASHBOARD SUMMARY =================

        //     public async Task<DashboardSummaryDto> GetDashboardSummary(int restaurantId)
        //     {
        //         var dto = new DashboardSummaryDto();

        //         string qry = @"
        // -- CURRENT MONTH
        // SELECT
        //     COALESCE(SUM(c_totalprice),0),
        //     COUNT(*)
        // FROM t_orders
        // WHERE c_restaurantid=@rid
        // AND c_orderdate >= DATE_TRUNC('month', NOW());

        // -- PREVIOUS MONTH
        // SELECT
        //     COALESCE(SUM(c_totalprice),0),
        //     COUNT(*)
        // FROM t_orders
        // WHERE c_restaurantid=@rid
        // AND c_orderdate >= DATE_TRUNC('month', NOW()) - INTERVAL '1 month'
        // AND c_orderdate < DATE_TRUNC('month', NOW());

        // -- CURRENT MONTH NEW CUSTOMERS
        // SELECT COUNT(*) FROM (
        //     SELECT c_customerid, MIN(c_orderdate) first_order
        //     FROM t_orders
        //     WHERE c_restaurantid=@rid
        //     GROUP BY c_customerid
        // ) t
        // WHERE first_order >= DATE_TRUNC('month', NOW());

        // -- PREVIOUS MONTH NEW CUSTOMERS
        // SELECT COUNT(*) FROM (
        //     SELECT c_customerid, MIN(c_orderdate) first_order
        //     FROM t_orders
        //     WHERE c_restaurantid=@rid
        //     GROUP BY c_customerid
        // ) t
        // WHERE first_order >= DATE_TRUNC('month', NOW()) - INTERVAL '1 month'
        // AND first_order < DATE_TRUNC('month', NOW());
        // ";

        //         await _conn.OpenAsync();
        //         using var cmd = new NpgsqlCommand(qry, _conn);
        //         cmd.Parameters.AddWithValue("@rid", restaurantId);

        //         using var r = await cmd.ExecuteReaderAsync();

        //         decimal currRevenue = 0, prevRevenue = 0;
        //         int currOrders = 0, prevOrders = 0;
        //         int currCustomers = 0, prevCustomers = 0;

        //         if (await r.ReadAsync())
        //         {
        //             currRevenue = r.GetDecimal(0);
        //             currOrders = r.GetInt32(1);
        //         }

        //         if (await r.NextResultAsync() && await r.ReadAsync())
        //         {
        //             prevRevenue = r.GetDecimal(0);
        //             prevOrders = r.GetInt32(1);
        //         }

        //         if (await r.NextResultAsync() && await r.ReadAsync())
        //             currCustomers = r.GetInt32(0);

        //         if (await r.NextResultAsync() && await r.ReadAsync())
        //             prevCustomers = r.GetInt32(0);

        //         dto.TotalRevenue = currRevenue;
        //         dto.TotalOrders = currOrders;
        //         dto.NewCustomers = currCustomers;

        //         dto.RevenueGrowth = prevRevenue == 0 ? 100 : ((currRevenue - prevRevenue) / prevRevenue) * 100;
        //         dto.OrdersGrowth = prevOrders == 0 ? 100 : ((decimal)(currOrders - prevOrders) / prevOrders) * 100;
        //         dto.CustomersGrowth = prevCustomers == 0 ? 100 : ((decimal)(currCustomers - prevCustomers) / prevCustomers) * 100;

        //         dto.GrowthRate = (dto.RevenueGrowth + dto.OrdersGrowth) / 2;

        //         await _conn.CloseAsync();
        //         return dto;
        //     }

        public async Task<DashboardSummaryDto> GetDashboardSummary(int restaurantId)
        {
            var dto = new DashboardSummaryDto();

            string qry = @"
    -- CURRENT MONTH
    SELECT
        COALESCE(SUM(c_totalprice),0) AS revenue,
        COUNT(*) AS orders,
        COUNT(DISTINCT c_customerid) AS clients
    FROM t_orders
    WHERE c_restaurantid = @rid
      AND c_orderdate >= DATE_TRUNC('month', NOW());

    -- PREVIOUS MONTH
    SELECT
        COALESCE(SUM(c_totalprice),0) AS revenue,
        COUNT(*) AS orders,
        COUNT(DISTINCT c_customerid) AS clients
    FROM t_orders
    WHERE c_restaurantid = @rid
      AND c_orderdate >= DATE_TRUNC('month', NOW()) - INTERVAL '1 month'
      AND c_orderdate < DATE_TRUNC('month', NOW());
    ";

            await _conn.OpenAsync();
            using var cmd = new NpgsqlCommand(qry, _conn);
            cmd.Parameters.AddWithValue("@rid", restaurantId);

            using var r = await cmd.ExecuteReaderAsync();

            decimal currRevenue = 0, prevRevenue = 0;
            int currOrders = 0, prevOrders = 0;
            int currClients = 0, prevClients = 0;

            // 🔹 CURRENT MONTH
            if (await r.ReadAsync())
            {
                currRevenue = r.GetDecimal(0);
                currOrders = r.GetInt32(1);
                currClients = r.GetInt32(2);
            }

            // 🔹 PREVIOUS MONTH
            if (await r.NextResultAsync() && await r.ReadAsync())
            {
                prevRevenue = r.GetDecimal(0);
                prevOrders = r.GetInt32(1);
                prevClients = r.GetInt32(2);
            }

            // ================= CALCULATIONS =================
            decimal currPlatformFees = currOrders * 10;
            decimal prevPlatformFees = prevOrders * 10;

            dto.TotalRevenue = currRevenue;
            dto.TotalOrders = currOrders;
            dto.TotalClients = currClients;
            dto.PlatformFees = currPlatformFees;

            dto.RevenueGrowth = prevRevenue == 0 ? 100 : ((currRevenue - prevRevenue) / prevRevenue) * 100;
            dto.OrdersGrowth = prevOrders == 0 ? 100 : ((decimal)(currOrders - prevOrders) / prevOrders) * 100;
            dto.ClientsGrowth = prevClients == 0 ? 100 : ((decimal)(currClients - prevClients) / prevClients) * 100;
            dto.PlatformFeesGrowth = prevPlatformFees == 0 ? 100 : ((currPlatformFees - prevPlatformFees) / prevPlatformFees) * 100;

            await _conn.CloseAsync();
            return dto;
        }
        public async Task<List<DashboardChartDto>> GetDashboardChartData(int restaurantId, int days)
        {
            List<DashboardChartDto> list = new();

            string qry = days <= 30
            ? @"
        SELECT
            TO_CHAR(c_orderdate, 'DD Mon') AS label,
            COUNT(*) AS orders,
            COALESCE(SUM(c_totalprice),0) AS revenue,
            COUNT(DISTINCT c_customerid) AS clients
        FROM t_orders
        WHERE c_restaurantid = @rid
          AND c_orderdate >= CURRENT_DATE - @days
        GROUP BY c_orderdate
        ORDER BY c_orderdate;
      "
            : @"
        SELECT
            TO_CHAR(DATE_TRUNC('month', c_orderdate), 'Mon YYYY') AS label,
            COUNT(*) AS orders,
            COALESCE(SUM(c_totalprice),0) AS revenue,
            COUNT(DISTINCT c_customerid) AS clients
        FROM t_orders
        WHERE c_restaurantid = @rid
          AND c_orderdate >= CURRENT_DATE - @days
        GROUP BY DATE_TRUNC('month', c_orderdate)
        ORDER BY DATE_TRUNC('month', c_orderdate);
      ";

            await _conn.OpenAsync();
            using var cmd = new NpgsqlCommand(qry, _conn);
            cmd.Parameters.AddWithValue("@rid", restaurantId);
            cmd.Parameters.AddWithValue("@days", days);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                int orders = Convert.ToInt32(r["orders"]);

                list.Add(new DashboardChartDto
                {
                    Label = r["label"].ToString(),
                    Orders = orders,
                    Revenue = Convert.ToDecimal(r["revenue"]),
                    Clients = Convert.ToInt32(r["clients"]),
                    PlatformFees = orders * 10
                });
            }

            await _conn.CloseAsync();
            return list;
        }


        // public async Task<List<DashboardChartDto>> GetDashboardChartData(
        //     int restaurantId,
        //     int days)
        // {
        //     List<DashboardChartDto> list = new();

        //     string qry = days <= 30
        //         ? @"
        // SELECT
        //     TO_CHAR(c_orderdate, 'DD Mon') AS label,
        //     COUNT(*) AS orders,
        //     COALESCE(SUM(c_totalprice),0) AS revenue,
        //     COUNT(DISTINCT c_customerid) AS clients
        // FROM t_orders
        // WHERE c_restaurantid = @rid
        //   AND c_orderdate >= NOW() - INTERVAL '@days days'
        // GROUP BY c_orderdate
        // ORDER BY c_orderdate;
        // "
        //         : @"
        // SELECT
        //     TO_CHAR(DATE_TRUNC('month', c_orderdate), 'Mon YYYY') AS label,
        //     COUNT(*) AS orders,
        //     COALESCE(SUM(c_totalprice),0) AS revenue,
        //     COUNT(DISTINCT c_customerid) AS clients
        // FROM t_orders
        // WHERE c_restaurantid = @rid
        //   AND c_orderdate >= NOW() - INTERVAL '@days days'
        // GROUP BY DATE_TRUNC('month', c_orderdate)
        // ORDER BY DATE_TRUNC('month', c_orderdate);
        // ";

        //     qry = qry.Replace("@days", days.ToString());

        //     await _conn.OpenAsync();
        //     using var cmd = new NpgsqlCommand(qry, _conn);
        //     cmd.Parameters.AddWithValue("@rid", restaurantId);

        //     using var r = await cmd.ExecuteReaderAsync();
        //     while (await r.ReadAsync())
        //     {
        //         int orders = Convert.ToInt32(r["orders"]);

        //         list.Add(new DashboardChartDto
        //         {
        //             Label = r["label"].ToString(),
        //             Orders = orders,
        //             Revenue = Convert.ToDecimal(r["revenue"]),
        //             Clients = Convert.ToInt32(r["clients"]),
        //             PlatformFees = orders * 10
        //         });
        //     }

        //     await _conn.CloseAsync();
        //     return list;
        // }

        public async Task<List<RecentOrderDto>> GetRecentOrders(int restaurantId)
        {
            List<RecentOrderDto> list = new();

            try
            {
                string qry = @"
        SELECT
            c_orderid,
            c_dishname,
            c_totalprice,
            c_orderstatus,
            c_orderdate,
            c_ordertime
        FROM t_orders
        WHERE c_restaurantid = @rid
        ORDER BY c_orderdate DESC, c_ordertime DESC
       ";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@rid", restaurantId);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var date = r.GetFieldValue<DateOnly>(
                        r.GetOrdinal("c_orderdate")
                    );

                    var time = r.GetFieldValue<TimeOnly>(
                        r.GetOrdinal("c_ordertime")
                    );

                    // 🔥 COMBINE DATE + TIME
                    DateTime orderDateTime = date.ToDateTime(time);

                    list.Add(new RecentOrderDto
                    {
                        OrderId = r["c_orderid"]?.ToString(),
                        DishName = r["c_dishname"]?.ToString(),
                        Amount = Convert.ToDecimal(r["c_totalprice"]),
                        Status = r["c_orderstatus"]?.ToString(),
                        OrderDate = orderDateTime
                    });
                }
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return list;
        }
        // =====================================================
        // GET RESTAURANT ORDERS (Delivered excluded)
        // =====================================================
        public async Task<List<RestaurantOrderDto>> GetRestaurantOrders(int restaurantId)
        {
            List<RestaurantOrderDto> list = new();

            try
            {
                string qry = @"
               SELECT
    o.c_bookingid,
    o.c_orderid,

    c.c_fname,
    c.c_lname,
    c.c_phonenumber,

    o.c_dishname,
    o.c_quantity,
    o.c_totalprice,
    o.c_orderstatus,
    o.c_orderdate,
    o.c_ordertime,

    o.c_housenumber,
    o.c_societyname,
    o.c_landmark,
    o.c_city,
    o.c_state,
    o.c_foodimage
FROM t_orders o
INNER JOIN t_customer c
    ON c.c_customerid = o.c_customerid
WHERE o.c_restaurantid = @rid
  AND o.c_orderstatus <> 'Delivered'
ORDER BY o.c_orderdate DESC, o.c_ordertime DESC;

                ";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@rid", restaurantId);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    // 🔹 DATE
                    var date = r.GetFieldValue<DateOnly>(
                        r.GetOrdinal("c_orderdate")
                    );

                    // 🔹 TIME
                    var time = r.GetFieldValue<TimeOnly>(
                        r.GetOrdinal("c_ordertime")
                    );

                    // 🔹 COMBINE DATE + TIME
                    DateTime orderDateTime = date.ToDateTime(time);

                    list.Add(new RestaurantOrderDto
                    {
                        BookingId = Convert.ToInt32(r["c_bookingid"]),
                        OrderId = r["c_orderid"]?.ToString(),

                        CustomerName = $"{r["c_fname"]} {r["c_lname"]}",

                        CustomerPhone = r["c_phonenumber"]?.ToString(),

                        DishName = r["c_dishname"]?.ToString(),
                        Quantity = Convert.ToInt32(r["c_quantity"]),
                        TotalPrice = Convert.ToInt32(r["c_totalprice"]),

                        OrderStatus = r["c_orderstatus"]?.ToString(),
                        OrderDateTime = orderDateTime,

                        FullAddress = $"{r["c_housenumber"]}, {r["c_societyname"]}, {r["c_landmark"]}, {r["c_city"]}, {r["c_state"]}",

                        FoodImage = r["c_foodimage"]?.ToString()
                    });

                }
            }
            catch (Exception ex)
            {
                // 🔴 LOG ERROR (can be replaced by ILogger)
                Console.WriteLine("GetRestaurantOrders Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return list;
        }

        // =====================================================
        // UPDATE ORDER STATUS (Restaurant Action)
        // =====================================================
        public async Task<bool> UpdateOrderStatus(int bookingId, string status)
        {
            try
            {
                string qry = @"
                UPDATE t_orders
                SET c_orderstatus = @status
                WHERE c_bookingid = @bid;
                ";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@bid", bookingId);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateOrderStatus Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        // =====================================================
        // ORDER STATUS COUNTS FOR DASHBOARD CARDS
        // =====================================================
        public async Task<OrderCardSummaryDto> GetOrderCardSummary(int restaurantId)
        {
            OrderCardSummaryDto dto = new();

            try
            {
                string qry = @"
                SELECT
                    COUNT(*) FILTER (WHERE c_orderstatus = 'Processing') AS processing,
                    COUNT(*) FILTER (WHERE c_orderstatus = 'Prepared') AS prepared,
                    COUNT(*) FILTER (WHERE c_orderstatus = 'Out for delivery') AS out_for_delivery,
                    COUNT(*) FILTER (WHERE c_orderstatus = 'Delivered') AS delivered
                FROM t_orders
                WHERE c_restaurantid = @rid;
                ";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@rid", restaurantId);

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    dto.Processing = Convert.ToInt32(r["processing"]);
                    dto.Prepared = Convert.ToInt32(r["prepared"]);
                    dto.OutForDelivery = Convert.ToInt32(r["out_for_delivery"]);
                    dto.Delivered = Convert.ToInt32(r["delivered"]);
                }
            }
            catch (Exception ex)
            {
                // 🔴 Log (replace with ILogger later)
                Console.WriteLine("GetOrderCardSummary Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return dto;
        }

        // ================= CUSTOMER PROFILE METHODS =================

        public async Task<List<t_state>> GetStates()
        {
            var states = new List<t_state>();
            try
            {
                var query = "SELECT id, c_sname FROM t_state";
                using var cmd = new NpgsqlCommand(query, _conn);

                if (_conn.State != System.Data.ConnectionState.Open)
                    await _conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    states.Add(new t_state
                    {
                        id = reader.GetInt32(0),
                        c_sname = reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching states: " + ex.Message);
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
            return states;
        }
        public async Task<List<t_cityy>> GetCities(int stateId)
        {
            var cities = new List<t_cityy>();
            try
            {
                var query = "SELECT cityid, cityname, stateid FROM t_city WHERE stateid = @stateId";
                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@stateId", stateId);

                if (_conn.State != System.Data.ConnectionState.Open)
                    await _conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cities.Add(new t_cityy
                    {
                        cityid = reader.GetInt32(0),
                        cityname = reader.GetString(1),
                        stateid = reader.GetInt32(2)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching cities: " + ex.Message);
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
            return cities;
        }
        // GET CUSTOMER PROFILE
        public async Task<t_customer?> GetCustomerProfile(int customerId)
        {
            try
            {
                string qry = @"
                SELECT 
                    c_customerid, c_fname, c_lname, c_state, c_city, 
                    c_pincode, c_gender, c_address, c_image, c_email, 
                    c_password, c_phonenumber, c_role, c_created_time, 
                    c_updated_time, c_status
                FROM t_customer 
                WHERE c_customerid = @c_customerid";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@c_customerid", customerId);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new t_customer
                    {
                        c_customerid = reader.GetInt32(reader.GetOrdinal("c_customerid")),
                        c_fname = reader["c_fname"]?.ToString() ?? "",
                        c_lname = reader["c_lname"]?.ToString() ?? "",
                        c_state = reader["c_state"]?.ToString() ?? "",
                        c_city = reader["c_city"]?.ToString() ?? "",
                        c_pincode = reader["c_pincode"]?.ToString() ?? "",
                        c_gender = reader["c_gender"]?.ToString() ?? "",
                        c_address = reader["c_address"]?.ToString() ?? "",
                        c_image = reader["c_image"]?.ToString(),
                        c_email = reader["c_email"]?.ToString() ?? "",
                        c_password = reader["c_password"]?.ToString() ?? "",
                        c_phonenumber = reader["c_phonenumber"]?.ToString() ?? "",
                        c_role = reader["c_role"]?.ToString() ?? "",
                        c_created_time = reader.GetDateTime(reader.GetOrdinal("c_created_time")),
                        c_updated_time = reader.GetDateTime(reader.GetOrdinal("c_updated_time")),
                        c_status = reader["c_status"]?.ToString() ?? ""
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCustomerProfile Error: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // UPDATE CUSTOMER PROFILE (without password)
        public async Task<bool> UpdateCustomerProfile(t_customer customer)
        {
            try
            {
                string qry = @"
                UPDATE t_customer SET
                    c_fname = @c_fname,
                    c_lname = @c_lname,
                    c_state = @c_state,
                    c_city = @c_city,
                    c_pincode = @c_pincode,
                    c_gender = @c_gender,
                    c_address = @c_address,
                    c_phonenumber = @c_phonenumber,
                    c_updated_time = @c_updated_time
                WHERE c_customerid = @c_customerid";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);

                cmd.Parameters.AddWithValue("@c_customerid", customer.c_customerid);
                cmd.Parameters.AddWithValue("@c_fname", customer.c_fname ?? "");
                cmd.Parameters.AddWithValue("@c_lname", customer.c_lname ?? "");
                cmd.Parameters.AddWithValue("@c_state", customer.c_state ?? "");
                cmd.Parameters.AddWithValue("@c_city", customer.c_city ?? "");
                cmd.Parameters.AddWithValue("@c_pincode", customer.c_pincode ?? "");
                cmd.Parameters.AddWithValue("@c_gender", customer.c_gender ?? "");
                cmd.Parameters.AddWithValue("@c_address", customer.c_address ?? "");
                cmd.Parameters.AddWithValue("@c_phonenumber", customer.c_phonenumber ?? "");
                cmd.Parameters.AddWithValue("@c_updated_time", DateTime.Now);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateCustomerProfile Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // UPDATE CUSTOMER IMAGE
        public async Task<bool> UpdateCustomerImage(int customerId, string imagePath)
        {
            try
            {
                string qry = @"
                UPDATE t_customer SET
                    c_image = @c_image,
                    c_updated_time = @c_updated_time
                WHERE c_customerid = @c_customerid";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@c_customerid", customerId);
                cmd.Parameters.AddWithValue("@c_image", imagePath);
                cmd.Parameters.AddWithValue("@c_updated_time", DateTime.Now);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateCustomerImage Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> UpdatePassword(int customerId, string hashedNewPassword)
        {
            try
            {
                string qry = "UPDATE t_customer SET c_password = @pw, c_updated_time = @ut WHERE c_customerid = @id";

                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@pw", hashedNewPassword);
                cmd.Parameters.AddWithValue("@ut", DateTime.Now);
                cmd.Parameters.AddWithValue("@id", customerId);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdatePassword Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // Update restaurant availability status
        public async Task<bool> UpdateRestaurantAvailability(int customerId, string availability)
        {
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open)
                    await _conn.OpenAsync();
           
                string updateQuery = "UPDATE t_restaurant SET c_r_isavailable = @availability WHERE c_customerid = @customerId";
                await using var updateCmd = new NpgsqlCommand(updateQuery, _conn);
                updateCmd.Parameters.AddWithValue("@availability", availability);
                updateCmd.Parameters.AddWithValue("@customerId", customerId);

                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateRestaurantAvailability Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }



    }
}
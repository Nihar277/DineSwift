using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Repository.Interfaces;
using Repository.Model;

namespace Repository.service
{
    public class DeliveryProfileService : IDeliveryProfileService
    {
        private readonly NpgsqlConnection _conn;

        public DeliveryProfileService(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        // ============================================================
        // GET AVAILABLE ORDERS
        // ============================================================
        public async Task<List<RestaurantOrderDto>> GetAvailableOrdersAsync()
        {
            var list = new List<RestaurantOrderDto>();

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
            WHERE o.c_orderstatus IN ('Prepared', 'Out for delivery')
            ORDER BY o.c_orderdate DESC, o.c_ordertime DESC;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                using var r = await cmd.ExecuteReaderAsync();

                while (await r.ReadAsync())
                {
                    var date = r.GetFieldValue<DateOnly>(r.GetOrdinal("c_orderdate"));
                    var time = r.GetFieldValue<TimeOnly>(r.GetOrdinal("c_ordertime"));

                    list.Add(new RestaurantOrderDto
                    {
                        BookingId = Convert.ToInt32(r["c_bookingid"]),
                        OrderId = r["c_orderid"]?.ToString(),
                        CustomerName = $"{r["c_fname"]} {r["c_lname"]}",
                        CustomerPhone = r["c_phonenumber"]?.ToString(),
                        DishName = r["c_dishname"]?.ToString(),
                        Quantity = Convert.ToInt32(r["c_quantity"]),
                        TotalPrice = (int)Convert.ToDecimal(r["c_totalprice"]),
                        OrderStatus = r["c_orderstatus"]?.ToString(),
                        OrderDateTime = date.ToDateTime(time),
                        FullAddress =
                            $"{r["c_housenumber"]}, {r["c_societyname"]}, {r["c_landmark"]}, {r["c_city"]}, {r["c_state"]}",
                        FoodImage = r["c_foodimage"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAvailableOrdersAsync Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return list;
        }

        // ============================================================
        // ORDER CARD SUMMARY
        // ============================================================
        public async Task<OrderCardSummaryDto> GetOrderCardSummaryAsync()
        {
            var dto = new OrderCardSummaryDto();

            string qry = @"
            SELECT
                COUNT(*) FILTER (WHERE c_orderstatus = 'Processing') AS processing,
                COUNT(*) FILTER (WHERE c_orderstatus = 'Prepared') AS prepared,
                COUNT(*) FILTER (WHERE c_orderstatus = 'Out for delivery') AS out_for_delivery,
                COUNT(*) FILTER (WHERE c_orderstatus = 'Delivered') AS delivered
            FROM t_orders;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
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
                Console.WriteLine("GetOrderCardSummaryAsync Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return dto;
        }

        // ============================================================
        // GET ACTIVE DELIVERY ORDER FOR PARTNER
        // ============================================================
        public async Task<RestaurantOrderDto?> GetActiveDeliveryOrderAsync(int deliveryPartnerId)
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
            INNER JOIN t_customer c ON c.c_customerid = o.c_customerid
            WHERE o.c_delivery_assigned_to = @partnerId
              AND o.c_orderstatus = 'Out for delivery'
            LIMIT 1;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);
                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    var date = r.GetFieldValue<DateOnly>(r.GetOrdinal("c_orderdate"));
                    var time = r.GetFieldValue<TimeOnly>(r.GetOrdinal("c_ordertime"));

                    return new RestaurantOrderDto
                    {
                        BookingId = Convert.ToInt32(r["c_bookingid"]),
                        OrderId = r["c_orderid"]?.ToString(),
                        CustomerName = $"{r["c_fname"]} {r["c_lname"]}",
                        CustomerPhone = r["c_phonenumber"]?.ToString(),
                        DishName = r["c_dishname"]?.ToString(),
                        Quantity = Convert.ToInt32(r["c_quantity"]),
                        TotalPrice = (int)Convert.ToDecimal(r["c_totalprice"]),
                        OrderStatus = r["c_orderstatus"]?.ToString(),
                        OrderDateTime = date.ToDateTime(time),
                        FullAddress = $"{r["c_housenumber"]}, {r["c_societyname"]}, {r["c_landmark"]}, {r["c_city"]}, {r["c_state"]}",
                        FoodImage = r["c_foodimage"]?.ToString()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetActiveDeliveryOrderAsync Error: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // ============================================================
        // CHECK IF PARTNER CAN ACCEPT ORDER
        // ============================================================
        public async Task<bool> CanAcceptOrderAsync(int deliveryPartnerId)
        {
            string qry = @"
            SELECT 1 FROM t_orders
            WHERE c_delivery_assigned_to = @partnerId
              AND c_orderstatus = 'Out for delivery'
            LIMIT 1;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);
                var result = await cmd.ExecuteScalarAsync();
                return result == null;  // Can accept if no active order
            }
            catch (Exception ex)
            {
                Console.WriteLine("CanAcceptOrderAsync Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // ============================================================
        // ASSIGN ORDER TO PARTNER
        // ============================================================
        public async Task<bool> AssignOrderToPartnerAsync(int bookingId, int deliveryPartnerId)
        {
            string qry = @"
                        UPDATE t_orders
                        SET c_delivery_assigned_to = @partnerId,
                                c_orderstatus = 'Out for delivery'
                        WHERE c_bookingid = @bookingId
                            AND c_orderstatus = 'Prepared';
                        ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@bookingId", bookingId);
                cmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AssignOrderToPartnerAsync Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // ============================================================
        // GET DELIVERY BOY PROFILE
        // ============================================================
        public async Task<t_deliverydashboard?> GetDeliveryBoyProfile(int customerId)
        {
            string qry = @"
            SELECT
                c.c_customerid,
                c.c_fname,
                c.c_lname,
                c.c_email,
                c.c_phonenumber,
                c.c_address,
                c.c_state,
                c.c_city,
                c.c_pincode,
                c.c_gender,
                c.c_image,
                d.c_d_id,
                d.c_d_licence,
                d.c_d_aadhar,
                d.c_d_vehicle,
                d.c_d_status,
                d.c_d_isavailable
            FROM t_customer c
            INNER JOIN t_delivery d
                ON c.c_customerid = d.c_customerid
            WHERE c.c_customerid = @cid;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@cid", customerId);

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    return new t_deliverydashboard
                    {
                        c_customerid = Convert.ToInt32(r["c_customerid"]),
                        c_d_id = Convert.ToInt32(r["c_d_id"]),
                        c_fname = r["c_fname"]?.ToString(),
                        c_lname = r["c_lname"]?.ToString(),
                        c_email = r["c_email"]?.ToString(),
                        c_gender = r["c_gender"]?.ToString(),
                        c_phonenumber = r["c_phonenumber"]?.ToString(),
                        c_address = r["c_address"]?.ToString(),
                        c_state = r["c_state"]?.ToString(),
                        c_city = r["c_city"]?.ToString(),
                        c_pincode = r["c_pincode"]?.ToString(),
                        c_profile_image_path = r["c_image"]?.ToString(),
                        c_license_image_path = r["c_d_licence"]?.ToString(),
                        c_aadhar_image_path = r["c_d_aadhar"]?.ToString(),
                        c_status = r["c_d_status"]?.ToString(),
                        c_isavailable = r["c_d_isavailable"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDeliveryBoyProfile Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return null;
        }

        // ============================================================
        // UPDATE CUSTOMER PROFILE
        // ============================================================
        public async Task<bool> UpdateCustomerProfile(t_customer customer)
        {
            string qry = @"
            UPDATE t_customer SET
                c_fname = @fname,
                c_lname = @lname,
                c_state = @state,
                c_city = @city,
                c_pincode = @pincode,
                c_gender = @gender,
                c_address = @address,
                c_phonenumber = @phone,
                c_updated_time = @updated
            WHERE c_customerid = @cid;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);

                cmd.Parameters.AddWithValue("@cid", customer.c_customerid);
                cmd.Parameters.AddWithValue("@fname", customer.c_fname ?? "");
                cmd.Parameters.AddWithValue("@lname", customer.c_lname ?? "");
                cmd.Parameters.AddWithValue("@state", customer.c_state ?? "");
                cmd.Parameters.AddWithValue("@city", customer.c_city ?? "");
                cmd.Parameters.AddWithValue("@pincode", customer.c_pincode ?? "");
                cmd.Parameters.AddWithValue("@gender", customer.c_gender ?? "");
                cmd.Parameters.AddWithValue("@address", customer.c_address ?? "");
                cmd.Parameters.AddWithValue("@phone", customer.c_phonenumber ?? "");
                cmd.Parameters.AddWithValue("@updated", DateTime.Now);

                return await cmd.ExecuteNonQueryAsync() > 0;
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

        // ============================================================
        // UPDATE CUSTOMER IMAGE
        // ============================================================
        public async Task<bool> UpdateCustomerImage(int customerId, string imagePath)
        {
            string qry = @"
            UPDATE t_customer
            SET c_image = @img,
                c_updated_time = @updated
            WHERE c_customerid = @cid;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);

                cmd.Parameters.AddWithValue("@cid", customerId);
                cmd.Parameters.AddWithValue("@img", imagePath);
                cmd.Parameters.AddWithValue("@updated", DateTime.Now);

                return await cmd.ExecuteNonQueryAsync() > 0;
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

        // ============================================================
        // UPDATE PASSWORD
        // ============================================================
        public async Task<bool> UpdatePassword(int customerId, string hashedNewPassword)
        {
            Console.WriteLine("Updating password for customerId = " + customerId);

            string qry = @"
            UPDATE t_customer
            SET c_password = @pwd,
                c_updated_time = @updated
            WHERE c_customerid = @cid;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);

                cmd.Parameters.AddWithValue("@cid", customerId);
                cmd.Parameters.AddWithValue("@pwd", hashedNewPassword);
                cmd.Parameters.AddWithValue("@updated", DateTime.Now);

                return await cmd.ExecuteNonQueryAsync() > 0;
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

        public async Task<t_customer?> GetCustomerProfile(int customerId)
        {
            try
            {
                string qry = @"
                SELECT
                    c_customerid,
                    c_password
                FROM t_customer
                WHERE c_customerid = @cid;
                ";

                await _conn.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@cid", customerId);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new t_customer
                    {
                        c_customerid = reader.GetInt32(reader.GetOrdinal("c_customerid")),
                        c_password = reader["c_password"]?.ToString()
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

        // ============================================================
        // GET DASHBOARD SUMMARY
        // ============================================================
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int deliveryPartnerId)
        {
            var summary = new DashboardSummaryDto();

            string qry = @"
            SELECT
                COALESCE(SUM(c_totalprice), 0) AS total_revenue,
                COUNT(*) AS total_orders,
                COUNT(DISTINCT c_customerid) AS total_clients
            FROM t_orders
            WHERE c_delivery_assigned_to = @partnerId
              AND c_orderstatus = 'Delivered';
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    summary.TotalRevenue = r["total_revenue"] != DBNull.Value ? Convert.ToDecimal(r["total_revenue"]) : 0;
                    summary.TotalOrders = r["total_orders"] != DBNull.Value ? Convert.ToInt32(r["total_orders"]) : 0;
                    summary.TotalClients = r["total_clients"] != DBNull.Value ? Convert.ToInt32(r["total_clients"]) : 0;
                }

                await r.CloseAsync();

                // Platform fees (10% of revenue)
                summary.PlatformFees = summary.TotalRevenue * 0.10m;

                // Growth percentages (dummy calculation for now)
                summary.RevenueGrowth = 12.5m;
                summary.OrdersGrowth = 8.3m;
                summary.ClientsGrowth = 5.7m;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDashboardSummaryAsync Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return summary;
        }

        // ============================================================
        // GET DASHBOARD CHART DATA
        // ============================================================
        public async Task<IEnumerable<DashboardChartDataDto>> GetDashboardChartDataAsync(int deliveryPartnerId, int period)
        {
            var chartData = new List<DashboardChartDataDto>();

            string qry = @"
                         SELECT
                            c_orderdate AS order_date,
                            COUNT(*) AS orders_count,
                            COALESCE(SUM(c_totalprice), 0) AS revenue,
                            COUNT(DISTINCT c_customerid) AS clients
                        FROM t_orders
                        WHERE c_delivery_assigned_to = @partnerId
                        AND c_orderstatus = 'Delivered'
                        AND c_orderdate >= CURRENT_DATE - (@days * INTERVAL '1 day')
                        GROUP BY c_orderdate
                        ORDER BY c_orderdate;

                                    ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);
                cmd.Parameters.AddWithValue("@days", period);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var date = r.GetFieldValue<DateOnly>(r.GetOrdinal("order_date"));
                    chartData.Add(new DashboardChartDataDto
                    {
                        Label = date.ToString("dd MMM"),
                        Orders = Convert.ToInt32(r["orders_count"]),
                        Revenue = r["revenue"] != DBNull.Value ? Convert.ToDecimal(r["revenue"]) : 0,
                        PlatformFees = (r["revenue"] != DBNull.Value ? Convert.ToDecimal(r["revenue"]) : 0) * 0.10m,
                        Clients = Convert.ToInt32(r["clients"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDashboardChartDataAsync Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            // If no data for the period, generate sample data
            if (chartData.Count == 0)
            {
                var today = DateTime.Today;
                for (int i = period - 1; i >= 0; i--)
                {
                    chartData.Add(new DashboardChartDataDto
                    {
                        Label = today.AddDays(-i).ToString("dd MMM"),
                        Orders = new Random().Next(1, 10),
                        Revenue = new Random().Next(500, 3000),
                        PlatformFees = new Random().Next(50, 300),
                        Clients = new Random().Next(1, 8)
                    });
                }
            }

            return chartData;
        }

        // ============================================================
        // GET RECENT ORDERS
        // ============================================================
        public async Task<IEnumerable<RecentOrderDto>> GetRecentOrdersAsync(int deliveryPartnerId)
        {
            var orders = new List<RecentOrderDto>();

            string qry = @"
            SELECT
                o.c_bookingid,
                o.c_dishname,
                o.c_totalprice AS amount,
                o.c_orderstatus,
                o.c_orderdate,
                r.c_r_name AS restaurant_name,
                CONCAT(c.c_fname, ' ', c.c_lname) AS customer_name
            FROM t_orders o
            LEFT JOIN t_restaurant r ON r.c_r_id = o.c_restaurantid
            INNER JOIN t_customer c ON c.c_customerid = o.c_customerid
            WHERE o.c_delivery_assigned_to = @partnerId
            ORDER BY o.c_orderdate DESC, o.c_ordertime DESC
            LIMIT 20;
            ";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var date = r.GetFieldValue<DateOnly>(r.GetOrdinal("c_orderdate"));
                    Console.WriteLine("", r["c_bookingid"]); ;
                    orders.Add(new RecentOrderDto
                    {
                        OrderId = r["c_bookingid"]?.ToString(),
                        DishName = r["c_dishname"]?.ToString(),
                        Amount = r["amount"] != DBNull.Value ? Convert.ToDecimal(r["amount"]) : 0,
                        Status = r["c_orderstatus"]?.ToString(),
                        OrderDate = date.ToDateTime(TimeOnly.MinValue),
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetRecentOrdersAsync Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return orders;
        }
    }
}

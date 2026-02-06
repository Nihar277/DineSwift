using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using Npgsql;
using Repository.Model;
using Repository.Models;
using Repository.service;

namespace Repository.service
{
    public class AdminService : IAdminInterface
    {
        private readonly NpgsqlConnection _conn;

        public AdminService(NpgsqlConnection conn)
        {
            _conn = conn;
        }



        public async Task<List<t_customer>> getAllCustomers()
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"SELECT c_customerid, c_fname, c_lname, c_state, c_city, c_pincode, c_gender, c_address, c_image, c_email, c_password, c_phonenumber, c_role, c_created_time, c_updated_time, c_status
	FROM public.t_customer WHERE c_role = 'c' ORDER BY c_customerid;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var customers = new List<t_customer>();
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new t_customer
                            {
                                c_customerid = (int)reader["c_customerid"],
                                c_fname = (string)reader["c_fname"],
                                c_lname = (string)reader["c_lname"],
                                c_state = (string)reader["c_state"],
                                c_city = (string)reader["c_city"],
                                c_pincode = (string)reader["c_pincode"],
                                c_gender = (string)reader["c_gender"],
                                c_address = (string)reader["c_address"],
                                c_image = (string)reader["c_image"],
                                c_email = (string)reader["c_email"],
                                c_password = (string)reader["c_password"],
                                c_phonenumber = (string)reader["c_phonenumber"],
                                c_role = (string)reader["c_role"],
                                c_created_time = (DateTime)reader["c_created_time"],
                                c_updated_time = (DateTime)reader["c_updated_time"],
                                c_status = (string)reader["c_status"]
                            });
                        }
                        return customers;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<delivery_vm>> getAllDeliveryBoy()
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"SELECT d.c_customerid, d.c_d_id, d.c_d_ac, d.c_d_ifsc, d.c_d_licence, d.c_d_vehicle, d.c_d_aadhar, d.c_d_status, d.c_d_isavailable, d.c_created_at,
c.c_fname, c.c_lname, c.c_state, c.c_city, c.c_pincode, c.c_gender, c.c_address, c.c_image, c.c_email, c.c_phonenumber, c_role, c_created_time, c_updated_time, c.c_status
FROM public.t_delivery d INNER JOIN t_customer c ON d.c_customerid = c.c_customerid ORDER BY c.c_customerid;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var customers = new List<delivery_vm>();
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new delivery_vm
                            {
                                c_customerid = (int)reader["c_customerid"],
                                c_fname = (string)reader["c_fname"],
                                c_lname = (string)reader["c_lname"],
                                c_state = (string)reader["c_state"],
                                c_city = (string)reader["c_city"],
                                c_pincode = (string)reader["c_pincode"],
                                c_gender = (string)reader["c_gender"],
                                c_address = (string)reader["c_address"],
                                c_image = (string)reader["c_image"],
                                c_email = (string)reader["c_email"],
                                c_phonenumber = (string)reader["c_phonenumber"],
                                c_role = (string)reader["c_role"],
                                c_created_time = (DateTime)reader["c_created_time"],
                                c_updated_time = (DateTime)reader["c_updated_time"],
                                c_status = reader["c_status"] == DBNull.Value ? null : (string)reader["c_status"],
                                c_d_id = (int)reader["c_d_id"],
                                c_d_ac = (long)reader["c_d_ac"],
                                c_d_ifsc = (string)reader["c_d_ifsc"],
                                c_d_licence = (string)reader["c_d_licence"],
                                c_d_vehicle = (string)reader["c_d_vehicle"],
                                c_d_aadhar = (string)reader["c_d_aadhar"],
                                c_d_status = reader["c_d_status"] == DBNull.Value ? null : (string)reader["c_d_status"],
                                c_d_isavailable = reader["c_d_isavailable"] == DBNull.Value ? null : (string)reader["c_d_isavailable"]

                            });
                        }
                        return customers;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        public async Task<List<reastaurant_vm>> getAllRestaurant()
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"SELECT r.c_r_id, r.c_r_name, r.c_r_email, r.c_r_address, r.c_r_state, r.c_r_city, r.c_r_ac, r.c_r_ifsc, r.c_r_gst, r.c_r_aadhar, c_r_image, r.c_r_isavailable, r.c_status c_r_status, r.c_customerid, r.c_created_at,
c.c_fname, c.c_lname, c.c_state, c.c_city, c.c_pincode, c.c_gender, c.c_address, c.c_image, c.c_email, c.c_phonenumber, c_role, c_created_time, c_updated_time, c.c_status
FROM public.t_restaurant r INNER JOIN t_customer c ON r.c_customerid = c.c_customerid ORDER BY r.c_customerid;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var customers = new List<reastaurant_vm>();
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new reastaurant_vm
                            {
                                c_customerid = (int)reader["c_customerid"],
                                c_fname = (string)reader["c_fname"],
                                c_lname = (string)reader["c_lname"],
                                c_state = (string)reader["c_state"],
                                c_city = (string)reader["c_city"],
                                c_pincode = (string)reader["c_pincode"],
                                c_gender = (string)reader["c_gender"],
                                c_address = (string)reader["c_address"],
                                c_image = reader["c_image"] == DBNull.Value ? "" : (string)reader["c_image"],
                                c_email = (string)reader["c_email"],
                                c_phonenumber = (string)reader["c_phonenumber"],
                                c_role = (string)reader["c_role"],
                                c_created_time = (DateTime)reader["c_created_time"],
                                c_updated_time = (DateTime)reader["c_updated_time"],
                                c_status = reader["c_status"] == DBNull.Value ? null : (string)reader["c_status"],
                                c_r_id = (int)reader["c_r_id"],
                                c_r_name = (string)reader["c_r_name"],
                                c_r_email = (string)reader["c_r_email"],
                                c_r_address = (string)reader["c_r_address"],
                                c_r_state = (string)reader["c_r_state"],
                                c_r_city = (string)reader["c_r_city"],
                                c_r_ac = (long)reader["c_r_ac"],
                                c_r_ifsc = (string)reader["c_r_ifsc"],
                                c_r_gst = (string)reader["c_r_gst"],
                                c_r_aadhar = (string)reader["c_r_aadhar"],
                                c_r_image = (string)reader["c_r_image"],
                                c_r_isavailable = reader["c_r_isavailable"] == DBNull.Value ? null : (string)reader["c_r_isavailable"],
                                c_r_status = reader["c_r_status"] == DBNull.Value ? null : (string)reader["c_r_status"],

                            });
                        }
                        return customers;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task inactiveRestaurant(int id)
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"UPDATE public.t_restaurant SET   c_status= 'inactive', c_r_isavailable= 'no' WHERE c_customerid = @id;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }

                string query1 = @"UPDATE public.t_customer SET c_status= 'inactive' WHERE c_customerid= @id;";
                using (var command = new NpgsqlCommand(query1, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }

                // string query2 = @"UPDATE public.t_restaurant SET c_r_isavailable= 'no' WHERE c_r_id= @id;";
                // using (var command = new NpgsqlCommand(query2, _conn))
                // {
                //     command.Parameters.AddWithValue("id", id);
                //     await command.ExecuteNonQueryAsync();
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task activeRestaurant(int id)
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"UPDATE public.t_restaurant SET   c_status= 'active' WHERE c_customerid = @id;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }

                string query1 = @"UPDATE public.t_customer SET c_status= 'active' WHERE c_customerid= @id;";
                using (var command = new NpgsqlCommand(query1, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task activeDeliveryBoy(int id)
        {

            try
            {
                await _conn.OpenAsync();
                string query = @"UPDATE public.t_customer SET  c_status= 'active' WHERE c_customerid= @id;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }

                string query1 = @"UPDATE public.t_delivery SET c_d_status= 'active' WHERE c_customerid=@id;";
                using (var command = new NpgsqlCommand(query1, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task inactiveDeliveryBoy(int id)
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"UPDATE public.t_customer SET  c_status= 'inactive' WHERE c_customerid= @id;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }

                string query1 = @"UPDATE public.t_delivery SET c_d_status= 'inactive', c_d_isavailable= 'false' WHERE c_customerid=@id;";
                using (var command = new NpgsqlCommand(query1, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }
                // string query2 = @"UPDATE public.t_delivery SET c_d_isavailable= 'false' WHERE c_customerid=@id;";
                // using (var command = new NpgsqlCommand(query2, _conn))
                // {
                //     command.Parameters.AddWithValue("id", id);
                //     await command.ExecuteNonQueryAsync();
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task activeCustomer(int id)
        {
            try
            {
                await _conn.OpenAsync();
                // string query = @"UPDATE public.t_restaurant SET   c_status= 'active' WHERE c_r_id = @id;";
                // using (var command = new NpgsqlCommand(query, _conn))
                // {
                //     command.Parameters.AddWithValue("id", id);
                //     await command.ExecuteNonQueryAsync();
                // }

                string query1 = @"UPDATE public.t_customer SET c_status= 'active' WHERE c_customerid= @id AND c_role = 'c';";
                using (var command = new NpgsqlCommand(query1, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task inactiveCustomer(int id)
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"UPDATE public.t_customer SET  c_status= 'inactive' WHERE c_customerid= @id AND c_role = 'c';";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    await command.ExecuteNonQueryAsync();
                }

                // string query1 = @"UPDATE public.t_delivery SET c_d_status= 'inactive' WHERE c_customerid=@id;";
                // using (var command = new NpgsqlCommand(query1, _conn))
                // {
                //     command.Parameters.AddWithValue("id", id);
                //     await command.ExecuteNonQueryAsync();
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        public async Task<TodayStats> getTodayStats()
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"SELECT
                            -- Orders
                            COUNT(*) FILTER (
                                WHERE c_orderdate = CURRENT_DATE
                                AND c_orderstatus = 'Delivered'
                            ) AS total_today_orders,

                            COUNT(*) FILTER (
                                WHERE c_orderstatus = 'Delivered'
                            ) AS total_orders,

                            -- Revenue (Food Total)
                            COALESCE(
                                SUM(c_totalprice) FILTER (
                                    WHERE c_orderdate = CURRENT_DATE
                                    AND c_orderstatus = 'Delivered'
                                ), 0
                            ) AS today_food_total,

                            COALESCE(
                                SUM(c_totalprice) FILTER (
                                    WHERE c_orderstatus = 'Delivered'
                                ), 0
                            ) AS food_total,

                            -- Active Customers
                            (
                                SELECT COUNT(*)
                                FROM public.t_customer
                                WHERE c_status = 'active'
                            ) AS total_active_customers,

                            -- Stats timestamp
                            NOW() AS stats_timestamp

                        FROM public.t_orders;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    var rd = await command.ExecuteReaderAsync();

                    if (await rd.ReadAsync())
                    {
                        return new TodayStats
                        {
                            todayOrders = Convert.ToInt32(rd["total_today_orders"]),
                            totalOrders = Convert.ToInt32(rd["total_orders"]),
                            todayRevenue = Convert.ToInt32(rd["today_food_total"]),
                            totalRevenue = Convert.ToDouble(rd["food_total"]),
                            activeUsers = Convert.ToInt32(rd["total_active_customers"])
                        };
                    }
                }

                return null;
                // string query1 = @"UPDATE public.t_delivery SET c_d_status= 'inactive' WHERE c_customerid=@id;";
                // using (var command = new NpgsqlCommand(query1, _conn))
                // {
                //     command.Parameters.AddWithValue("id", id);
                //     await command.ExecuteNonQueryAsync();
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<TodayStats> getRevenueDetails()
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"SELECT
                            -- Orders
                            COUNT(*) FILTER (
                                WHERE c_orderdate = CURRENT_DATE
                                AND c_orderstatus = 'Delivered'
                            ) AS total_today_orders,

                            COUNT(*) FILTER (
                                WHERE c_orderstatus = 'Delivered'
                            ) AS total_orders,

                            -- Revenue (Food Total)
                            COALESCE(
                                SUM(c_totalprice) FILTER (
                                    WHERE c_orderdate = CURRENT_DATE
                                    AND c_orderstatus = 'Delivered'
                                ), 0
                            ) AS today_food_total,

                            COALESCE(
                                SUM(c_totalprice) FILTER (
                                    WHERE c_orderstatus = 'Delivered'
                                ), 0
                            ) AS food_total,

                            -- Active Customers
                            (
                                SELECT COUNT(*)
                                FROM public.t_customer
                                WHERE c_status = 'active'
                            ) AS total_active_customers,

                            -- Stats timestamp
                            NOW() AS stats_timestamp

                        FROM public.t_orders;";
                using (var command = new NpgsqlCommand(query, _conn))
                {
                    var rd = await command.ExecuteReaderAsync();

                    if (await rd.ReadAsync())
                    {
                        return new TodayStats
                        {
                            todayOrders = Convert.ToInt32(rd["total_today_orders"]),
                            totalOrders = Convert.ToInt32(rd["total_orders"]),
                            todayRevenue = Convert.ToInt32(rd["today_food_total"]),
                            totalRevenue = Convert.ToDouble(rd["food_total"]),
                            activeUsers = Convert.ToInt32(rd["total_active_customers"])
                        };
                    }
                }

                return null;
                // string query1 = @"UPDATE public.t_delivery SET c_d_status= 'inactive' WHERE c_customerid=@id;";
                // using (var command = new NpgsqlCommand(query1, _conn))
                // {
                //     command.Parameters.AddWithValue("id", id);
                //     await command.ExecuteNonQueryAsync();
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<orderStats>> getOrderStats(int days)
        {
            try
            {
                await _conn.OpenAsync();
                string query;
                if (days < 90)
                {
                    query = @"
                                SELECT 
                                    c_orderdate::date AS date,
                                    TRIM(TO_CHAR(c_orderdate, 'Day')) AS day,
                                    COUNT(*) AS orders,
                                    SUM(c_totalprice) AS revenue,
                                    ROUND(AVG(c_totalprice), 2) AS avg_order_value
                                FROM public.t_orders
                                WHERE c_orderdate >= CURRENT_DATE - (@days * INTERVAL '1 day')
                                AND c_orderstatus = 'Delivered'
                                GROUP BY c_orderdate::date
                                ORDER BY date ASC;
                            ";

                }
                else
                {
                    query = @"
                            SELECT
                                DATE_TRUNC('month', c_orderdate)::date AS date,
                                TO_CHAR(DATE_TRUNC('month', c_orderdate), 'Mon YYYY') AS day,
                                COUNT(*) AS orders,
                                SUM(c_totalprice) AS revenue,
                                ROUND(AVG(c_totalprice), 2) AS avg_order_value
                            FROM public.t_orders
                            WHERE c_orderdate >= CURRENT_DATE - (@days * INTERVAL '1 day')
                            AND c_orderstatus = 'Delivered'
                            GROUP BY DATE_TRUNC('month', c_orderdate)
                            ORDER BY date;
                    ";
                }

                using (var command = new NpgsqlCommand(query, _conn))
                {
                    command.Parameters.AddWithValue("@days", days);
                    var rd = await command.ExecuteReaderAsync();
                    var orders = new List<orderStats>();
                    while (await rd.ReadAsync())
                    {
                        orders.Add(new orderStats
                        {
                            date = (DateOnly)rd["date"],
                            day = rd["day"].ToString(),
                            orders = Convert.ToInt32(rd["orders"]),
                            revenue = Convert.ToDouble(rd["revenue"]) + Convert.ToDouble(rd["revenue"]) * 0.1 + Convert.ToDouble(rd["orders"]) * 10,
                            avg_order_value = Convert.ToDouble(rd["avg_order_value"])
                        });
                    }

                    return orders;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        public async Task<List<AdminStats>> getAdminStats(int days)
        {
            try
            {
                await _conn.OpenAsync();
                string query;
                if (days < 90)
                {
                    query = @"
                               SELECT 
                                    c_orderdate::date AS date,
									TO_CHAR(DATE_TRUNC('month', c_orderdate), 'Mon YYYY') AS day,
                                    COUNT(*) AS orders,
									count(*) *10 AS admin_comission
                                FROM public.t_orders
                                WHERE c_orderdate >= CURRENT_DATE - (@days * INTERVAL '1 day')
                                AND c_orderstatus = 'Delivered'
                                GROUP BY c_orderdate::date
                                ORDER BY date ASC;
                            ";

                }
                else
                {
                    query = @"
                            SELECT
								DATE_TRUNC('month', c_orderdate)::date AS date,
                                TO_CHAR(DATE_TRUNC('month', c_orderdate), 'Mon YYYY') AS day,
                                COUNT(*) AS orders,
                                COUNT(*) * 10 AS admin_comission
                            FROM public.t_orders
                            WHERE c_orderdate >= CURRENT_DATE - (@days * INTERVAL '1 day')
                            AND c_orderstatus = 'Delivered'
                            GROUP BY DATE_TRUNC('month', c_orderdate)
                            ORDER BY date;
                    ";
                }

                using (var command = new NpgsqlCommand(query, _conn))
                {
                    command.Parameters.AddWithValue("@days", days);
                    var rd = await command.ExecuteReaderAsync();
                    var orders = new List<AdminStats>();
                    while (await rd.ReadAsync())
                    {
                        orders.Add(new AdminStats
                        {
                            date = (DateOnly)rd["date"],
                            day = rd["day"].ToString(),
                            orders = Convert.ToInt32(rd["orders"]),
                            adminComission = Convert.ToDouble(rd["admin_comission"])
                        });
                    }

                    return orders;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<TodayOrderStatus>> GetTodayOrderStatus()
        {
            try
            {
                await _conn.OpenAsync();
                string query = @"
                                SELECT 
                            c_orderstatus,
                            COUNT(*) AS total_orders
                        FROM public.t_orders
                        WHERE DATE(c_orderdate) = CURRENT_DATE
                        GROUP BY c_orderstatus
                        ORDER BY 
                            CASE c_orderstatus
                                WHEN 'Delivered' THEN 1
                                WHEN 'Out for delivery' THEN 2
                                WHEN 'Processing' THEN 3
                                WHEN 'Prepared' THEN 4
                                ELSE 5
                            END
                            ";

                using (var command = new NpgsqlCommand(query, _conn))
                {
                    var rd = await command.ExecuteReaderAsync();
                    var orders = new List<TodayOrderStatus>();
                    while (await rd.ReadAsync())
                    {
                        orders.Add(new TodayOrderStatus
                        {
                            c_orderstatus = rd["c_orderstatus"].ToString(),
                            total_orders = Convert.ToInt32(rd["total_orders"])
                        });
                    }

                    return orders;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
    }
}
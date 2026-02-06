using Repository.service;
using Repository.Model;
using Npgsql;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using BCrypt.Net;



namespace Repository.service
{
    public class deliveryservice : IDeliveryService
    {

        private readonly NpgsqlConnection _con;
        private readonly IWebHostEnvironment _env;

        public deliveryservice(NpgsqlConnection con, IWebHostEnvironment env)
        {
            _con = con;
            _env = env;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var cmd = new NpgsqlCommand(
                "SELECT 1 FROM t_customer WHERE c_email = @email LIMIT 1",
                _con
            );

            cmd.Parameters.AddWithValue("@email", email);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;

        }




        public async Task<int> RegisterDeliveryPartnerAsync(t_customer customer, DeliveryPartnerForm form)
        {
            await _con.OpenAsync();

            try
            {

                bool emailExists = await EmailExistsAsync(customer.c_email);
                if (emailExists)
                {
                    throw new Exception("Email already exists");
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(customer.c_password);
                // ---------- SAVE CUSTOMER IMAGE ----------
                customer.c_image = SaveFile(
                customer.c_imagefile,
                 "customer",
                "",
                customer.c_email
                );


                // ---------- INSERT CUSTOMER ----------
                var cmdCustomer = new NpgsqlCommand(@"
                    INSERT INTO t_customer
                    (c_fname,c_lname,c_state,c_city,c_pincode,c_gender,
                     c_address,c_image,c_email,c_password,
                     c_phonenumber,c_role,c_status)
                    VALUES
                    (@fn,@ln,@st,@ct,@pin,@gen,
                     @addr,@img,@em,@pwd,@ph,'d','inactive')
                    RETURNING c_customerid;
                ", _con);

                cmdCustomer.Parameters.AddWithValue("@fn", customer.c_fname);
                cmdCustomer.Parameters.AddWithValue("@ln", customer.c_lname);
                cmdCustomer.Parameters.AddWithValue("@st", customer.c_state);
                cmdCustomer.Parameters.AddWithValue("@ct", customer.c_city);
                cmdCustomer.Parameters.AddWithValue("@pin", customer.c_pincode);
                cmdCustomer.Parameters.AddWithValue("@gen", customer.c_gender);
                cmdCustomer.Parameters.AddWithValue("@addr", customer.c_address);
                cmdCustomer.Parameters.AddWithValue("@img", customer.c_image ?? "");
                cmdCustomer.Parameters.AddWithValue("@em", customer.c_email);
                cmdCustomer.Parameters.AddWithValue("@pwd", hashedPassword);
                cmdCustomer.Parameters.AddWithValue("@ph", customer.c_phonenumber);

                int customerId = Convert.ToInt32(await cmdCustomer.ExecuteScalarAsync());

                // ---------- SAVE DELIVERY IMAGES ----------
                var delivery = new t_delivery
                {
                    c_customerid = customerId,
                    c_d_ac = form.c_d_ac,
                    c_d_ifsc = form.c_d_ifsc,
                    LicenceImageFile = form.LicenceImageFile,
                    VehicleImageFile = form.VehicleImageFile,
                    AadharImageFile = form.AadharImageFile
                };

                delivery.c_d_licence = SaveFile(
                 form.LicenceImageFile,
                 "delivery",
                 "licence",
                 customer.c_email
             );

                delivery.c_d_vehicle = SaveFile(
                    form.VehicleImageFile,
                    "delivery",
                    "vehicle",
                    customer.c_email
                );

                delivery.c_d_aadhar = SaveFile(
                    form.AadharImageFile,
                    "delivery",
                    "aadhar",
                    customer.c_email
                );


                // ---------- INSERT DELIVERY ----------
                var cmdDelivery = new NpgsqlCommand(@"
                    INSERT INTO t_delivery
                    (c_customerid,c_d_ac,c_d_ifsc,
                     c_d_licence,c_d_vehicle,c_d_aadhar,c_d_status,c_d_isavailable)
                    VALUES
                    (@cid,@ac,@ifsc,@lic,@veh,@aad,'inactive','false');
                ", _con);

                cmdDelivery.Parameters.AddWithValue("@cid", delivery.c_customerid);
                cmdDelivery.Parameters.AddWithValue("@ac", delivery.c_d_ac);
                cmdDelivery.Parameters.AddWithValue("@ifsc", delivery.c_d_ifsc);
                cmdDelivery.Parameters.AddWithValue("@lic", delivery.c_d_licence ?? "");
                cmdDelivery.Parameters.AddWithValue("@veh", delivery.c_d_vehicle ?? "");
                cmdDelivery.Parameters.AddWithValue("@aad", delivery.c_d_aadhar ?? "");

                await cmdDelivery.ExecuteNonQueryAsync();

                return customerId;
            }
            finally
            {
                await _con.CloseAsync();
            }
        }

        // ---------- FILE SAVE ----------
        private string SaveFile(IFormFile file, string mainFolder, string subFolder, string email)
        {
            if (file == null || file.Length == 0)
                return "";

            //  MVC wwwroot/Images physical path
            string rootPath = "D:\\Casepoint\\Internship\\Food Project\\DineSwift\\MVC\\wwwroot\\Images";

            //  FIX: Handle empty subFolder correctly
            string folderPath = string.IsNullOrEmpty(subFolder)
                ? Path.Combine(rootPath, mainFolder)
                : Path.Combine(rootPath, mainFolder, subFolder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            //  Safe filename from email
            string safeEmail = email
                .ToLower();


            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{safeEmail}{extension}";
            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            //  FIX: Correct relative path for DB
            string relativePath = string.IsNullOrEmpty(subFolder)
                ? $"Images/{mainFolder}/{fileName}"
                : $"Images/{mainFolder}/{subFolder}/{fileName}";

            return fileName;
        }


        public async Task<List<t_state>> GetAllStateAsync()
        {
            string query = "SELECT * FROM public.t_state ORDER BY id ASC ";
            _con.Open();
            var cmd = new NpgsqlCommand(query, _con);
            var rd = cmd.ExecuteReader();
            var states = new List<t_state>();

            while (rd.Read())
            {
                states.Add(new t_state
                {
                    id = (int)rd["id"],
                    c_sname = (string)rd["c_sname"],
                });
            }

            _con.Close();
            return states;
        }

        public async Task<List<t_city>> GetAllCityAsync(int id)
        {
            // Console.WriteLine("---------------------------------------------" + id);

            string query = "SELECT * FROM public.t_city WHERE stateid = @id ORDER BY cityid ASC ";
            _con.Open();
            var cmd = new NpgsqlCommand(query, _con);
            cmd.Parameters.AddWithValue("id", id);
            var rd = cmd.ExecuteReader();
            var states = new List<t_city>();

            while (rd.Read())
            {
                states.Add(new t_city
                {
                    id = (int)rd["cityid"],
                    stateId = (int)rd["stateid"],
                    cityName = (string)rd["cityname"],
                });
            }

            _con.Close();
            return states;
        }

        // =====================================================
        // UPDATE ORDER STATUS (Delivery Partner Action)
        // =====================================================
        public async Task<bool> UpdateOrderStatusAsync(int bookingId, string status)
        {
            try
            {
                await _con.OpenAsync();

                // Update order status
                string qry = @"
                UPDATE t_orders
                SET c_orderstatus = @status
                WHERE c_bookingid = @bid;
                ";

                using (var cmd = new NpgsqlCommand(qry, _con))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@bid", bookingId);
                    int rows = await cmd.ExecuteNonQueryAsync();
                    if (rows == 0)
                        return false;
                }

                // If delivered, insert revenue for the assigned delivery partner (once)
                if (string.Equals(status, "Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    // Get assigned partner and order amount
                    string sel = @"
                    SELECT c_delivery_assigned_to, c_totalprice
                    FROM t_orders
                    WHERE c_bookingid = @bid
                    LIMIT 1;
                    ";

                    int partnerId = 0;
                    decimal totalPrice = 0;

                    using (var selCmd = new NpgsqlCommand(sel, _con))
                    {
                        selCmd.Parameters.AddWithValue("@bid", bookingId);
                        using var r = await selCmd.ExecuteReaderAsync();
                        if (await r.ReadAsync())
                        {
                            partnerId = r["c_delivery_assigned_to"] == DBNull.Value ? 0 : Convert.ToInt32(r["c_delivery_assigned_to"]);
                            totalPrice = r["c_totalprice"] == DBNull.Value ? 0 : Convert.ToDecimal(r["c_totalprice"]);
                        }
                    }

                    if (partnerId > 0)
                    {
                        // Insert revenue only if not already recorded for this booking
                        string ins = @"
                        INSERT INTO t_d_revenue (c_bookingid, c_customerid, c_revenue)
                        SELECT @bid, @partnerId, @revenue
                        WHERE NOT EXISTS (SELECT 1 FROM t_d_revenue WHERE c_bookingid = @bid);
                        ";

                        using var revCmd = new NpgsqlCommand(ins, _con);
                        revCmd.Parameters.AddWithValue("@bid", bookingId);
                        revCmd.Parameters.AddWithValue("@partnerId", partnerId);
                        revCmd.Parameters.AddWithValue("@revenue", totalPrice);

                        await revCmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateOrderStatusAsync Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _con.CloseAsync();
            }
        }

        // =====================================================
        // ACCEPT ORDER (Assign to Delivery Partner)
        // =====================================================
        public async Task<bool> AcceptOrderAsync(int bookingId, int deliveryPartnerId)
        {
            try
            {
                await _con.OpenAsync();

                // Check if partner has active delivery order
                string checkQry = @"
                SELECT 1 FROM t_orders
                WHERE c_delivery_assigned_to = @partnerId
                  AND c_orderstatus = 'Out for delivery'
                LIMIT 1;
                ";

                using var checkCmd = new NpgsqlCommand(checkQry, _con);
                checkCmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);
                var result = await checkCmd.ExecuteScalarAsync();

                if (result != null)
                {
                    // Partner already has an active delivery order
                    return false;
                }

                // Assign order to partner and set status to Out for delivery
                string updateQry = @"
                UPDATE t_orders
                SET c_delivery_assigned_to = @partnerId,
                    c_orderstatus = 'Out for delivery'
                WHERE c_bookingid = @bookingId
                                    AND c_orderstatus = 'Prepared';
                ";

                using var updateCmd = new NpgsqlCommand(updateQry, _con);
                updateCmd.Parameters.AddWithValue("@bookingId", bookingId);
                updateCmd.Parameters.AddWithValue("@partnerId", deliveryPartnerId);

                int rows = await updateCmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AcceptOrderAsync Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _con.CloseAsync();
            }
        }


        // =====================================================
        // UPDATE ORDER STATUS (Delivery Partner Action)
        // =====================================================
        public async Task<bool> UpdateOrderStatusAsync(DeliveryOrderVM order)
        {
            try
            {
                await _con.OpenAsync();

                // ================= UPDATE ORDER STATUS =================
                string updateQry = @"
        UPDATE t_orders
        SET c_orderstatus = @status
        WHERE c_bookingid = @bid;
        ";

                using (var cmd = new NpgsqlCommand(updateQry, _con))
                {
                    cmd.Parameters.AddWithValue("@status", order.c_delivery_status);
                    cmd.Parameters.AddWithValue("@bid", order.c_bookingid);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    if (rows == 0)
                        return false;
                }

                // ================= INSERT REVENUE (ONLY IF DELIVERED) =================
                if (order.c_delivery_status == "Delivered")
                {
                    string revenueQry = @"
            INSERT INTO t_d_revenue
            (c_bookingid, c_customerid, c_revenue)
            VALUES
            (@bookingid, @customerid, @revenue);
            ";

                    using var revCmd = new NpgsqlCommand(revenueQry, _con);
                    revCmd.Parameters.AddWithValue("@bookingid", order.c_bookingid);
                    revCmd.Parameters.AddWithValue("@customerid", order.c_customerid ?? 0);
                    revCmd.Parameters.AddWithValue("@revenue", order.c_revenue);

                    await revCmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateOrderStatusAndRevenueAsync Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _con.CloseAsync();
            }
        }

        public async Task<List<DeliveryDashboardChartDto>> GetDeliveryBoyDashboardChartData(
    int deliveryBoyCustomerId,
    int days)
        {
            List<DeliveryDashboardChartDto> list = new();

            string qry = days <= 30
                ? @"
        SELECT
            TO_CHAR(o.c_orderdate, 'DD Mon') AS label,
            COUNT(*) AS orders,
            COALESCE(SUM(r.c_revenue), 0) AS revenue
        FROM t_d_revenue r
        INNER JOIN t_orders o
            ON o.c_bookingid = r.c_bookingid
        WHERE r.c_customerid = @cid
          AND o.c_orderstatus = 'Delivered'
          AND o.c_orderdate >= CURRENT_DATE - @days
        GROUP BY o.c_orderdate
        ORDER BY o.c_orderdate;
        "
                : @"
        SELECT
            TO_CHAR(DATE_TRUNC('month', o.c_orderdate), 'Mon YYYY') AS label,
            COUNT(*) AS orders,
            COALESCE(SUM(r.c_revenue), 0) AS revenue
        FROM t_d_revenue r
        INNER JOIN t_orders o
            ON o.c_bookingid = r.c_bookingid
        WHERE r.c_customerid = @cid
          AND o.c_orderstatus = 'Delivered'
          AND o.c_orderdate >= CURRENT_DATE - @days
        GROUP BY DATE_TRUNC('month', o.c_orderdate)
        ORDER BY DATE_TRUNC('month', o.c_orderdate);
        ";

            try
            {
                await _con.OpenAsync();

                using var cmd = new NpgsqlCommand(qry, _con);
                cmd.Parameters.AddWithValue("@cid", deliveryBoyCustomerId);
                cmd.Parameters.AddWithValue("@days", days);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    list.Add(new DeliveryDashboardChartDto
                    {
                        Label = r["label"].ToString(),
                        DeliveredOrders = Convert.ToInt32(r["orders"]),
                        DeliveryEarnings = Convert.ToDecimal(r["revenue"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDeliveryBoyDashboardChartData Error: " + ex.Message);
            }
            finally
            {
                await _con.CloseAsync();
            }

            return list;
        }


    }
}
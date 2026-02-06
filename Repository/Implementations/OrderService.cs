using Npgsql;
using Repository.Model;

namespace API.service;
public class OrderService : IOrderInterface
{
    private readonly NpgsqlConnection _conn;

    public OrderService(NpgsqlConnection conn)
    {
        _conn = conn;
    }

    private string GenerateOrderId()
    {
        return $"ORD-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
        
    }

    // ===============================
    // CREATE ORDER
    // ===============================
    public async Task<int> CreateOrderAsync(t_order order)
    {
        try
        {
            // c_orderid generated here
            order.c_orderid = GenerateOrderId();
            // ✅ Time without milliseconds
TimeSpan orderTime = new TimeSpan(
    DateTime.Now.Hour,
    DateTime.Now.Minute,
    DateTime.Now.Second
);


            string query = @"
                INSERT INTO t_orders
                (
                    c_orderid,
                    c_customerid,
                    c_restaurantid,
                    c_orderdate,
                    c_housenumber,
                    c_societyname,
                    c_landmark,
                    c_city,
                    c_state,
                    c_totalprice,
                    c_quantity,
                    c_foodimage,
                    c_orderstatus,
                    c_dishname,
                    c_ordertime,
                    c_fooditemid
                )
                VALUES
                (
                    @c_orderid,
                    @c_customerid,
                    @c_restaurantid,
                    @c_orderdate,
                    @c_housenumber,
                    @c_societyname,
                    @c_landmark,
                    @c_city,
                    @c_state,
                    @c_totalprice,
                    @c_quantity,
                    @c_foodimage,
                    @c_orderstatus,
                    @c_dishname,
                    @c_ordertime,
                    @c_fooditemid
                )";

            await using var cmd = new NpgsqlCommand(query, _conn);

            cmd.Parameters.AddWithValue("@c_orderid", order.c_orderid);
            cmd.Parameters.AddWithValue("@c_customerid", order.c_customerid);
            cmd.Parameters.AddWithValue("@c_restaurantid", order.c_restaurantid);
            cmd.Parameters.AddWithValue("@c_orderdate", order.c_orderdate);
            cmd.Parameters.AddWithValue("@c_housenumber", order.c_housenumber);
            cmd.Parameters.AddWithValue("@c_societyname", order.c_societyname);
            cmd.Parameters.AddWithValue("@c_landmark", (object?)order.c_landmark ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@c_city", order.c_city);
            cmd.Parameters.AddWithValue("@c_state", order.c_state);
            cmd.Parameters.AddWithValue("@c_totalprice", order.c_totalprice);
            cmd.Parameters.AddWithValue("@c_quantity", order.c_quantity);
            cmd.Parameters.AddWithValue("@c_foodimage", (object?)order.c_foodimage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@c_orderstatus", "Processing");
            cmd.Parameters.AddWithValue("@c_dishname", order.c_dishname);
            cmd.Parameters.AddWithValue("@c_ordertime", orderTime); 
            cmd.Parameters.AddWithValue("@c_fooditemid", order.c_fooditemid); 

            await _conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _conn.CloseAsync();

            return 1;
        }
        catch (Exception ex)
        {
            await _conn.CloseAsync();
            Console.WriteLine("Error while placing order in OrderService ---> " + ex.Message);
            return -1;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<int> CancelOrderAsync(int c_bookingid)
        {
            try
            {
                string query = @"
                    DELETE FROM t_orders
                    WHERE c_bookingid = @c_bookingid
                    AND c_orderstatus = 'Processing'";

                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_bookingid", c_bookingid);

                await _conn.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while cancelling order ---> " + ex.Message);
                return -1;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
}

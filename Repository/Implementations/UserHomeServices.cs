using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Npgsql;
using Repository.Interfaces;
using Repository.Model;


namespace Repository.Services
{
    public class UserHomeServices : IUserHomeServices
    {
        private readonly NpgsqlConnection _conn;
        public UserHomeServices(NpgsqlConnection conn)
        {
            _conn = conn;
            
        }
        public async Task<List<t_vm_fooditem>> getAllDishes()
        {
            List<t_vm_fooditem> items = new List<t_vm_fooditem>();

            try
            {
                string query = @"SELECT 
    f.c_itemid,
    f.c_restaurantid,
    res.c_r_name,      
    f.c_name,
    f.c_category,
    f.c_imageurl,
    f.c_price,
    AVG(r.c_rating) AS avg_rating
FROM t_fooditem f
INNER JOIN t_rating r 
    ON f.c_itemid = r.c_fooditemid
INNER JOIN t_restaurant res
    ON f.c_restaurantid = res.c_r_id
GROUP BY 
    f.c_itemid,
    f.c_restaurantid,
    res.c_r_name,
    f.c_name,
    f.c_category,
    f.c_imageurl,
    f.c_price
ORDER BY avg_rating DESC;";

                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new t_vm_fooditem()
                    {
                        c_itemid = Convert.ToInt32(reader["c_itemid"]),
                        c_restaurantid = Convert.ToInt32(reader["c_restaurantid"]),
                        c_r_name = (string)reader["c_r_name"],
                        c_name = reader["c_name"].ToString(),
                        c_category = (string)reader["c_category"],
                        c_price = Convert.ToInt32(reader["c_price"]),
                        c_imageurl = (string)reader["c_imageurl"]

                    });
                }
                return items;

            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Error While fetching Dishes: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<t_vm_fooditem>> getAllDishesforchat()
        {
            List<t_vm_fooditem> items = new List<t_vm_fooditem>();

            try
            {
                string query = @"SELECT 
    f.c_itemid,
    f.c_restaurantid,
    res.c_r_name,      
    f.c_name,
    f.c_category,
    f.c_imageurl,
    f.c_price
FROM t_fooditem f
INNER JOIN t_restaurant res
    ON f.c_restaurantid = res.c_r_id
WHERE f.c_isavailable='true';";

                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new t_vm_fooditem()
                    {
                        c_itemid = Convert.ToInt32(reader["c_itemid"]),
                        c_restaurantid = Convert.ToInt32(reader["c_restaurantid"]),
                        c_r_name = (string)reader["c_r_name"],
                        c_name = reader["c_name"].ToString(),
                        c_category = (string)reader["c_category"],
                        c_price = Convert.ToInt32(reader["c_price"]),
                        c_imageurl = (string)reader["c_imageurl"]

                    });
                }
                return items;

            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Error While fetching Dishes 2: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<t_vm_fooditem> getDishInfo(int id)
        {
            t_vm_fooditem item = new t_vm_fooditem();
            try
            {
                string query = "select f.c_itemid,f.c_name,f.c_description,f.c_ingredients,f.c_category,f.c_imageurl,f.c_price,f.c_type,r.c_r_name,r.c_r_id from t_fooditem f INNER JOIN t_restaurant r ON f.c_restaurantid=r.c_r_id where f.c_itemid=@id;";

                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("id", id);

                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    item = new t_vm_fooditem
                    {
                        c_itemid = Convert.ToInt32(reader["c_itemid"]),
                        c_name = reader["c_name"].ToString(),
                        c_category = (string)reader["c_category"],
                        c_description = (string)reader["c_description"],
                        c_ingredients = (string)reader["c_ingredients"],
                        c_price = Convert.ToInt32(reader["c_price"]),
                        c_imageurl = (string)reader["c_imageurl"],
                        c_type = (string)reader["c_type"],
                        c_r_name = (string)reader["c_r_name"],
                        c_restaurantid = (int)reader["c_r_id"]
                    };
                }
                return item;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Error While getting Dish Info: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<t_cartaddress> GetAddress(int customerId)
        {
            t_cartaddress address = new t_cartaddress();

            try
            {
                string query = @"SELECT c_address, c_city, c_state 
                         FROM t_customer 
                         WHERE c_customerid = @CustomerId";

                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("CustomerId", customerId);

                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string fullAddress = reader["c_address"]?.ToString() ?? "";

                    // Split address by comma
                    string[] parts = fullAddress.Split(',');

                    address = new t_cartaddress
                    {
                        c_housenumber = parts.Length > 0 ? parts[0].Trim() : "",
                        c_societyname = parts.Length > 1 ? parts[1].Trim() : "",
                        c_landmark = parts.Length > 2 ? parts[2].Trim() : "",
                        c_city = reader["c_city"].ToString(),
                        c_state = reader["c_state"].ToString()
                    };
                }

                return address;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Error While fetching Address: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        public async Task<List<t_vm_restaurant>> GetAllRestaurants()
        {
            List<t_vm_restaurant> restaurants = new List<t_vm_restaurant>();
            try
            {
                await _conn.CloseAsync();
                string query = @"SELECT 
                                    r.c_r_id,
                                    r.c_r_name,
                                    r.c_r_email,
                                    r.c_r_address,
                                    r.c_r_state,
                                    r.c_r_city,
                                    r.c_r_image,
                                    r.c_r_isavailable,
                                    r.c_status,
                                    r.c_customerid,
                                    r.c_created_at,
                                    ROUND(COALESCE(AVG(rt.c_rating), 0), 1) AS avg_rating,
                                    COUNT(rt.c_rating) AS total_reviews
                                FROM t_restaurant r
                                LEFT JOIN t_rating rt 
                                    ON r.c_r_id = rt.c_restaurantid
                                WHERE 
                                    r.c_status = 'active'
                                    AND r.c_r_isavailable = 'yes'
                                GROUP BY 
                                    r.c_r_id,
                                    r.c_r_name,
                                    r.c_r_email,
                                    r.c_r_address,
                                    r.c_r_state,
                                    r.c_r_city,
                                    r.c_r_image,
                                    r.c_r_isavailable,
                                    r.c_status,
                                    r.c_customerid,
                                    r.c_created_at
                                ORDER BY 
                                    avg_rating DESC,
                                    total_reviews DESC;
                                ";
                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    restaurants.Add(new t_vm_restaurant()
                    {
                        c_r_id = (int)reader["c_r_id"],
                        c_r_name = (string)reader["c_r_name"],
                        c_r_email = (string)reader["c_r_email"],
                        c_r_address = (string)reader["c_r_address"],
                        c_r_state = (string)reader["c_r_state"],
                        c_r_city = (string)reader["c_r_city"],
                        c_r_image = (string)reader["c_r_image"],
                        c_r_isavailable = (string)reader["c_r_isavailable"],
                        c_status = (string)reader["c_status"],
                        c_customerid = (int)reader["c_customerid"],
                        c_created_at = (DateTime)reader["c_created_at"],
                    });
                }
                await _conn.CloseAsync();
                return restaurants;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Error While fetching Restaturants: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> AddToCart(t_cart cart)
        {
            try
            {
                string checkQuery = @"
                SELECT COUNT(1)
                FROM t_cart
                WHERE c_customerId = @CustomerId
                AND c_itemId = @ItemId";
                using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, _conn))
                {
                    checkCmd.Parameters.AddWithValue("@CustomerId", cart.c_customerId);
                    checkCmd.Parameters.AddWithValue("@ItemId", cart.c_itemId);

                    await _conn.OpenAsync();
                    int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (count > 0)
                    {
                        // Item already exists → do not add again
                        return false;
                    }
                }

                string insertQuery = @"
                INSERT INTO t_cart
                (c_customerId, c_itemId, c_restaurantId, c_itemname, c_price, c_quantity, c_image)
                VALUES
                (@CustomerId, @ItemId, @RestaurantId, @ItemName, @Price, @Quantity, @Image)";

                using (NpgsqlCommand insertCmd = new NpgsqlCommand(insertQuery, _conn))
                {
                    insertCmd.Parameters.AddWithValue("@CustomerId", cart.c_customerId);
                    insertCmd.Parameters.AddWithValue("@ItemId", cart.c_itemId);
                    insertCmd.Parameters.AddWithValue("@RestaurantId", cart.c_restaurantId);
                    insertCmd.Parameters.AddWithValue("@ItemName", cart.c_itemname);
                    insertCmd.Parameters.AddWithValue("@Price", cart.c_price);
                    insertCmd.Parameters.AddWithValue("@Quantity", cart.c_quantity);
                    insertCmd.Parameters.AddWithValue("@Image", cart.c_image ?? "");

                    await insertCmd.ExecuteNonQueryAsync();
                }
                return true;

            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Error While Adding item to cart:" + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        public async Task<List<t_cart>> GetCartItems(int userid)
        {
            List<t_cart> cartItems = new List<t_cart>();
            try
            {
                string query = "SELECT c_cartid, c_customerid, c_itemid, c_restaurantid, c_itemname, c_price, c_quantity, c_image FROM t_cart WHERE c_customerid = @UserId;";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("UserId", userid);

                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cartItems.Add(new t_cart()
                    {
                        c_cartId = Convert.ToInt32(reader["c_cartid"]),
                        c_customerId = Convert.ToInt32(reader["c_customerid"]),
                        c_itemId = Convert.ToInt32(reader["c_itemid"]),
                        c_restaurantId = Convert.ToInt32(reader["c_restaurantid"]),
                        c_itemname = reader["c_itemname"].ToString(),
                        c_price = Convert.ToInt32(reader["c_price"]),
                        c_quantity = Convert.ToInt32(reader["c_quantity"]),
                        c_image = reader["c_image"].ToString()
                    });
                }
                return cartItems;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Error While fetching Cart Items: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<t_cart?> GetCartItemById(int cartId)
        {
            try
            {
                string query = "SELECT c_cartid, c_customerid, c_itemid, c_restaurantid, c_itemname, c_price, c_quantity, c_image FROM t_cart WHERE c_cartid = @CartId";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("CartId", cartId);

                await _conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new t_cart
                    {
                        c_cartId = reader.GetInt32(0),
                        c_customerId = reader.GetInt32(1),
                        c_itemId = reader.GetInt32(2),
                        c_restaurantId = reader.GetInt32(3),
                        c_itemname = reader.GetString(4),
                        c_price = reader.GetInt32(5),
                        c_quantity = reader.GetInt32(6),
                        c_image = reader.IsDBNull(7) ? null : reader.GetString(7)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCartItemById Error: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> DeleteCartItem(int cartId)
        {
            try
            {
                string query = "DELETE FROM t_cart WHERE c_cartid = @CartId";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("CartId", cartId);

                await _conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();

                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Delete Cart Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> UpdateCartQuantity(int cartId, int quantity)
        {
            try
            {
                string query = "UPDATE t_cart SET c_quantity = @Quantity WHERE c_cartid = @CartId";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("Quantity", quantity);
                cmd.Parameters.AddWithValue("CartId", cartId);

                await _conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();

                Console.WriteLine($"Rows affected: {rows}");
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update Cart Quantity Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<t_vm_restaurant> GetRestaurantById(int id)
        {
            t_vm_restaurant restaurant = null;
            try
            {
                await _conn.CloseAsync();
                string query = @"SELECT 
                                    r.c_r_id,
                                    r.c_r_name,
                                    r.c_r_email,
                                    r.c_r_address,
                                    r.c_r_state,
                                    r.c_r_city,
                                    r.c_r_image,
                                    r.c_r_isavailable,
                                    r.c_status,
                                    r.c_customerid,
                                    r.c_created_at
                                FROM t_restaurant r
                                WHERE r.c_r_id = @id
                                AND r.c_status = 'active'
                                AND r.c_r_isavailable = 'yes';";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("id", id);
                await _conn.OpenAsync();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    restaurant = new t_vm_restaurant()
                    {
                        c_r_id = (int)reader["c_r_id"],
                        c_r_name = (string)reader["c_r_name"],
                        c_r_email = (string)reader["c_r_email"],
                        c_r_address = (string)reader["c_r_address"],
                        c_r_state = (string)reader["c_r_state"],
                        c_r_city = (string)reader["c_r_city"],
                        c_r_image = (string)reader["c_r_image"],
                        c_r_isavailable = (string)reader["c_r_isavailable"],
                        c_status = (string)reader["c_status"],
                        c_customerid = (int)reader["c_customerid"],
                        c_created_at = (DateTime)reader["c_created_at"]
                    };
                }
                await _conn.CloseAsync();
                return restaurant;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
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
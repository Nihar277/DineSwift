using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repository.Service;
using Repository.Model;
using Repository.service;


namespace Repository.Implementation
{
    public class FoodItemServices : IFoodItemServices
    {

        private readonly IElasticSearch _elasticService;
        private readonly NpgsqlConnection _conn;
        public FoodItemServices(NpgsqlConnection conn, IElasticSearch elasticService)
        {
            _conn = conn;
            _elasticService = elasticService;
        }

        public async Task<bool> FoodItemAdd(t_fooditem fooditem)
        {
            try
            {
                await _conn.CloseAsync();
                string query = @"INSERT into t_fooditem(c_restaurantid, c_name, c_description, 
                            c_ingredients, c_category, c_imageurl, c_price, c_isavailable, c_type) 
                            VALUES(@resid, @name, @des, @ingredients, @category, @img, @price, @isavl, @type) RETURNING c_itemid;";
                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                cmd.Parameters.AddWithValue("resid", fooditem.c_restaurantid);
                cmd.Parameters.AddWithValue("name", fooditem.c_name);
                cmd.Parameters.AddWithValue("des", fooditem.c_description);
                cmd.Parameters.AddWithValue("ingredients", fooditem.c_ingredients);
                cmd.Parameters.AddWithValue("category", fooditem.c_category);
                cmd.Parameters.AddWithValue("img", fooditem.c_imageurl);
                cmd.Parameters.AddWithValue("price", fooditem.c_price);
                cmd.Parameters.AddWithValue("isavl", fooditem.c_isavailable);
                cmd.Parameters.AddWithValue("type", fooditem.c_type);
                var rd = cmd.ExecuteScalar();

                fooditem.c_itemid = Convert.ToInt32(rd);

                await _conn.CloseAsync();

                await _elasticService.UpdateFoodItemsPartial(fooditem);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Food item add error: " + ex.Message);
                await _conn.CloseAsync();
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<t_fooditem>> GetAllFoodItemByRestaurant(int id) // Restaurant ID
        {
            List<t_fooditem> fooditems = new List<t_fooditem>();
            try
            {
                await _conn.CloseAsync();
                string query = @"SELECT * FROM t_fooditem WHERE c_restaurantid=@id;";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("id", id);
                await _conn.OpenAsync();
                var read = cmd.ExecuteReader();
                while (read.Read())
                {
                    fooditems.Add(new t_fooditem
                    {
                        c_itemid = (int)read["c_itemid"],
                        c_name = (string)read["c_name"],
                        c_restaurantid = (int)read["c_restaurantid"],
                        c_description = (string)read["c_description"],
                        c_ingredients = (string)read["c_ingredients"],
                        c_category = (string)read["c_category"],
                        c_imageurl = (string)read["c_imageurl"],
                        c_price = (int)read["c_price"],
                        c_isavailable = (bool)read["c_isavailable"],
                        c_type = (string)read["c_type"],
                    });
                }
                await _conn.CloseAsync();
                return fooditems;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Food item add error: " + ex.Message);
                await _conn.CloseAsync();
                return fooditems;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> FoodItemUpdate(t_fooditem fooditem)
        {
            try
            {
                await _conn.CloseAsync();

                string query = @"
        UPDATE t_fooditem 
        SET 
            c_restaurantid = @resid,
            c_name = @name,
            c_description = @des,
            c_ingredients = @ingredients,
            c_category = @category,
            c_imageurl = COALESCE(@img, c_imageurl),
            c_price = @price,
            c_isavailable = @isavl,
            c_type = @type
        WHERE c_itemid = @id";

                using var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();

                cmd.Parameters.AddWithValue("id", fooditem.c_itemid);
                cmd.Parameters.AddWithValue("resid", fooditem.c_restaurantid);
                cmd.Parameters.AddWithValue("name", fooditem.c_name);
                cmd.Parameters.AddWithValue("des", fooditem.c_description ?? "");
                cmd.Parameters.AddWithValue("ingredients", fooditem.c_ingredients ?? "");
                cmd.Parameters.AddWithValue("category", fooditem.c_category ?? "");

                // ⭐ FIXED IMAGE PARAM
                cmd.Parameters.AddWithValue(
                    "img",
                    (object?)fooditem.c_imageurl ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue("price", fooditem.c_price);
                cmd.Parameters.AddWithValue("isavl", fooditem.c_isavailable);
                cmd.Parameters.AddWithValue("type", fooditem.c_type ?? "");

                await cmd.ExecuteNonQueryAsync();
                await _elasticService.UpdateFoodItemsPartial(fooditem);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Food item update error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> FoodItemDelete(int id)
        {
            try
            {
                await _conn.CloseAsync();
                string query = @"DELETE FROM t_fooditem WHERE c_itemid=@id;";
                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                cmd.Parameters.AddWithValue("id", id);

                var rd = cmd.ExecuteNonQuery();
                await _conn.CloseAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Food item delete error: " + ex.Message);
                await _conn.CloseAsync();
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> UpdateAvailability(int id, bool isAvailable)
        {
            try
            {
                await _conn.CloseAsync();
                string query = @"UPDATE t_fooditem SET c_isavailable=@isavl WHERE c_itemid=@id;";
                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("isavl", isAvailable);
                var rd = cmd.ExecuteNonQuery();
                await _conn.CloseAsync();
                return rd > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Food item availability update error: " + ex.Message);
                await _conn.CloseAsync();
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<t_fooditem>> GetAllFoodItem() // Restaurant ID
        {
            List<t_fooditem> fooditems = new List<t_fooditem>();
            try
            {
                await _conn.CloseAsync();
                string query = @"SELECT * FROM t_fooditem";
                var cmd = new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                var read = cmd.ExecuteReader();
                while (read.Read())
                {
                    fooditems.Add(new t_fooditem
                    {
                        c_itemid = (int)read["c_itemid"],
                        c_name = (string)read["c_name"],
                        c_restaurantid = read["c_restaurantid"] == DBNull.Value ? 0 : (int)read["c_restaurantid"],
                        c_description = (string)read["c_description"],
                        c_ingredients = (string)read["c_ingredients"],
                        c_category = (string)read["c_category"],
                        c_imageurl = (string)read["c_imageurl"],
                        c_price = (int)read["c_price"],
                        c_isavailable = (bool)read["c_isavailable"],
                        c_type = (string)read["c_type"],
                    });
                }
                await _conn.CloseAsync();
                return fooditems;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Food item add error: " + ex.Message);
                await _conn.CloseAsync();
                return fooditems;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<t_fooditem>> GetAllDishes()
        {
            List<t_fooditem> fooditems = new List<t_fooditem>();

            try
            {
                await _conn.CloseAsync();

                string query = @"SELECT * FROM t_fooditem WHERE c_isavailable = true ORDER BY c_itemid DESC;";
                var cmd = new NpgsqlCommand(query, _conn);

                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    fooditems.Add(new t_fooditem
                    {
                        c_itemid = reader.GetInt32(reader.GetOrdinal("c_itemid")),
                        c_restaurantid = reader.GetInt32(reader.GetOrdinal("c_restaurantid")),
                        c_name = reader.GetString(reader.GetOrdinal("c_name")),
                        c_description = reader.GetString(reader.GetOrdinal("c_description")),
                        c_ingredients = reader.GetString(reader.GetOrdinal("c_ingredients")),
                        c_category = reader.GetString(reader.GetOrdinal("c_category")),
                        c_imageurl = reader.IsDBNull(reader.GetOrdinal("c_imageurl"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("c_imageurl")),
                        c_price = reader.GetInt32(reader.GetOrdinal("c_price")),
                        c_isavailable = reader.GetBoolean(reader.GetOrdinal("c_isavailable")),
                        c_type = reader.GetString(reader.GetOrdinal("c_type"))
                    });
                }

                await _conn.CloseAsync();
                return fooditems;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get all dishes error: " + ex.Message);
                await _conn.CloseAsync();
                return fooditems;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

    }
}
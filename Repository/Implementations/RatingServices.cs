using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repository.Model;
using Repository.Service;

namespace Repository.Implementation
{
    public class RatingServices : IRatingServices
    {
        private readonly NpgsqlConnection _conn;
        public RatingServices(NpgsqlConnection conn)
        {
            _conn=conn;
        } 

        public async Task<List<t_rating>> GetAllRating()
        {
            List<t_rating> ratings=new List<t_rating>();
            try
            {
                
                await _conn.CloseAsync();
                string query=@"SELECT * from t_rating;";
                var cmd=new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                
                var reader=cmd.ExecuteReader();
                while (reader.Read())
                {
                    ratings.Add(new t_rating
                    {
                        c_ratingid=(int)reader["c_ratingid"],
                        c_userid=(int)reader["c_userid"],
                        c_restaurantid=(int)reader["c_restaurantid"],
                        c_fooditemid=(int)reader["c_fooditemid"],
                        c_rating=(int)reader["c_rating"],
                        c_reviewtext=(string)reader["c_reviewtext"],
                        c_date=(DateOnly)reader["c_date"],
                    });
                }
                await _conn.CloseAsync();
                return ratings;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Get all rating error: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> AddRating(t_rating rating)
        {
            try
            {
                await _conn.CloseAsync();
                string query=@"INSERT into t_rating(c_userid, c_restaurantid, c_fooditemid, c_rating, c_reviewtext, c_date) 
                                VALUES(@userid, @restaurantid, @fooditemid, @rating, @reviewtext, @date);";
                var cmd=new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                cmd.Parameters.AddWithValue("userid", rating.c_userid);
                cmd.Parameters.AddWithValue("restaurantid", rating.c_restaurantid);
                cmd.Parameters.AddWithValue("fooditemid", rating.c_fooditemid);
                cmd.Parameters.AddWithValue("rating", rating.c_rating);
                cmd.Parameters.AddWithValue("reviewtext", rating.c_reviewtext);
                cmd.Parameters.AddWithValue("date", rating.c_date);
                var rd=cmd.ExecuteNonQuery();
                await _conn.CloseAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Add rating error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<bool> DeleteRating(int id)
        {
            try
            {
                await _conn.CloseAsync();
                string query=@"DELETE from t_rating WHERE c_ratingid=@id;";
                var cmd=new NpgsqlCommand(query, _conn);
                await _conn.OpenAsync();
                cmd.Parameters.AddWithValue("id", id);
        
                var rd=cmd.ExecuteNonQuery();
                await _conn.CloseAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Delete rating error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
    }
}
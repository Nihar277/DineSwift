using Npgsql;

using Repository.Model;
using Repository.service;
using Repository.Model;
using BCrypt.Net;
namespace Repository.service;
using Repository.service;
using BCrypt.Net;


using Repository.Interfaces;
using System.Collections.Generic;

public class CustomerService : ICustomerService
{
    private readonly NpgsqlConnection _conn;
    public CustomerService(NpgsqlConnection conn)
    {
        _conn = conn;
    }

    public async Task<List<t_city>> GetCities(int stateId)
    {
        var cities = new List<t_city>();
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
            cities.Add(new t_city
            {
                id = reader.GetInt32(0),
                cityName = reader.GetString(1),
                stateId = reader.GetInt32(2)
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

    public async Task<bool> GetCustomer(string c_email)
    {
        
        try
            {
                var query = @"SELECT COUNT(*) FROM t_customer WHERE c_email = @c_email";
                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_email", c_email);

                if (_conn.State != System.Data.ConnectionState.Open)
                    await _conn.OpenAsync();   // ✅ awaited

                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Implementation of getuser: " + ex.Message);
                return false;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();  // ✅ close once
            }
    }

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


    public async Task<bool> Register(t_customer customer)
    {
        try
        {
            if (await GetCustomer(customer.c_email))
            {
                return false;
            }

            // Reset sequence to max id to avoid duplicate key
            string resetSeqQuery = "SELECT setval('t_customer_c_customerid_seq', COALESCE((SELECT MAX(c_customerid) FROM t_customer), 0));";
            using var resetCmd = new NpgsqlCommand(resetSeqQuery, _conn);
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();
            await resetCmd.ExecuteNonQueryAsync();

            string query = @"
        INSERT INTO t_customer
        (
            c_customerid, c_fname, c_lname, c_state, c_city, c_pincode,
            c_gender, c_address, c_image, c_email,
            c_password, c_phonenumber, c_role,
            c_created_time, c_updated_time, c_status
        )
        VALUES
        (
            DEFAULT, @c_fname, @c_lname, @c_state, @c_city, @c_pincode,
            @c_gender, @c_address, @c_image, @c_email,
            @c_password, @c_phonenumber, @c_role,
            @c_created_time, @c_updated_time, @c_status
        );";

            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();   // ✅ awaited

            using var cmd = new NpgsqlCommand(query, _conn);

            cmd.Parameters.AddWithValue("@c_fname", customer.c_fname);
            cmd.Parameters.AddWithValue("@c_lname", customer.c_lname);
            cmd.Parameters.AddWithValue("@c_state", customer.c_state);
            cmd.Parameters.AddWithValue("@c_city", customer.c_city);
            cmd.Parameters.AddWithValue("@c_pincode", customer.c_pincode);
            cmd.Parameters.AddWithValue("@c_gender", customer.c_gender);
            cmd.Parameters.AddWithValue("@c_address", customer.c_address);
            cmd.Parameters.AddWithValue("@c_image", customer.c_image);
            cmd.Parameters.AddWithValue("@c_email", customer.c_email);
            cmd.Parameters.AddWithValue("@c_password", customer.c_password);
            cmd.Parameters.AddWithValue("@c_phonenumber", customer.c_phonenumber);
            cmd.Parameters.AddWithValue("@c_role", customer.c_role ?? "u");
            cmd.Parameters.AddWithValue("@c_created_time", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@c_updated_time", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@c_status", customer.c_status ?? "active");

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Register implementation Error: " + ex.Message);
            return false;
        }
        finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                await _conn.CloseAsync();  // ✅ safe close
        }
    }



}

using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System;

namespace HelloPostgres;

class Program
{
    static async Task Main(string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var host = config["Db:Host"];
        var port = config["Db:Port"];
        var username = config["Db:Username"];
        var password = config["Db:Password"];
        var database = config["Db:Database"];
        var connString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        var dataSource = dataSourceBuilder.Build();

        var conn = await dataSource.OpenConnectionAsync();

        await InsertWeather(conn, "London", 10, 23, 5, 6, new DateOnly(2024, 5, 19));
        await InsertWeather(conn, "London", 10, 20, 61, 4, new DateOnly(2024, 5, 20));
        await InsertWeather(conn, "London", 10, 23, 50, 6, new DateOnly(2024, 5, 21));

        await InsertWeather(conn, "New York City", 14, 21, 64, 8, new DateOnly(2024, 5, 19));
        await InsertWeather(conn, "New York City", 14, 22, 55, 9, new DateOnly(2024, 5, 20));
        await InsertWeather(conn, "New York City", 15, 24, 56, 9, new DateOnly(2024, 5, 21));

        await InsertWeather(conn, "Tokyo", 15, 20, 83, 4, new DateOnly(2024, 5, 20));
        await InsertWeather(conn, "Tokyo", 19, 27, 52, 8, new DateOnly(2024, 5, 21));
        await InsertWeather(conn, "Tokyo", 18, 23, 40, 7, new DateOnly(2024, 5, 22));

        await InsertCity(conn, "London", new NpgsqlPoint(51.509865f, -0.118092f));
        await InsertCity(conn, "New York City", new NpgsqlPoint(40.730610f, -73.935242f));
        await InsertCity(conn, "Tokyo", new NpgsqlPoint(35.652832f, 139.839478f));

        await using (var cmd = new NpgsqlCommand("SELECT city FROM weather", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                Console.WriteLine(reader.GetString(0));
            }
        }

        await using (var cmd = new NpgsqlCommand("SELECT name FROM cities", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                Console.WriteLine(reader.GetString(0));
            }
        }
    }

    private static async Task InsertWeather(
            NpgsqlConnection connection,
            string city,
            int temperature_low,
            int temperature_high,
            short humidity,
            short uv,
            DateOnly date)
    {
        await using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO weather (city, temperature_low, temperature_high, humidity, uv, date)
    VALUES(@city, @temperature_low, @temperature_high, @humidity, @uv, @date)", connection))
        {
            cmd.Parameters.AddWithValue("city", city);
            cmd.Parameters.AddWithValue("temperature_low", temperature_low);
            cmd.Parameters.AddWithValue("temperature_high", temperature_high);
            cmd.Parameters.AddWithValue("humidity", humidity);
            cmd.Parameters.AddWithValue("uv", uv);
            cmd.Parameters.AddWithValue("date", date);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task InsertCity(NpgsqlConnection connection, string name, NpgsqlPoint location)
    {
        await using (var cmd = new NpgsqlCommand(@"INSERT INTO cities (name, location)
VALUES(@name, @location)", connection))
        {
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("location", location);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

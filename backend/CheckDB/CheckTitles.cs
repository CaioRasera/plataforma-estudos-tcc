using System;
using System.Threading.Tasks;
using Npgsql;

namespace CheckTitles;

class Program
{
    static async Task Main()
    {
        string connStr = "Host=db.dzjpexspnvqpbkvtxqsb.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=tBq!aRPhDcvBtU3d3W4x";
        using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("SELECT \"Title\" FROM \"Documents\";", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"DB Title: {reader.GetString(0)}");
        }
    }
}
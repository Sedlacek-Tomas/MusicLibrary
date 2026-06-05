using System.Collections.Generic;
using System.Threading.Tasks;
using MusicLibrary.Models;
using Npgsql;

namespace MusicLibrary.Repositories;

public class GenreRepository(string connectionString) : IGenreRepository
{
    public async Task<List<Genre>> GetAllAsync()
    {
        var list = new List<Genre>();
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT id, name FROM genres ORDER BY name", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(new Genre { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        return list;
    }
}
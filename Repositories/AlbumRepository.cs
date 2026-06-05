using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicLibrary.Models;
using Npgsql;

namespace MusicLibrary.Repositories;

public class AlbumRepository(string connectionString) : IAlbumRepository
{
    public async Task<List<Album>> GetAllAsync()
    {
        var list = new List<Album>();
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            @"SELECT a.id, a.title, a.artist, a.year, a.genre_id, COALESCE(g.name, '')
              FROM albums a
              LEFT JOIN genres g ON a.genre_id = g.id
              ORDER BY a.artist, a.title", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(Map(reader));
        return list;
    }

    public async Task<Album?> GetByIdAsync(int id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            @"SELECT a.id, a.title, a.artist, a.year, a.genre_id, COALESCE(g.name, '')
              FROM albums a
              LEFT JOIN genres g ON a.genre_id = g.id
              WHERE a.id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task AddAsync(Album a)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO albums (title, artist, year, genre_id) VALUES (@t, @ar, @y, @g)", conn);
        cmd.Parameters.AddWithValue("t", a.Title);
        cmd.Parameters.AddWithValue("ar", a.Artist);
        cmd.Parameters.AddWithValue("y", (object?)a.Year ?? DBNull.Value);
        cmd.Parameters.AddWithValue("g", a.GenreId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Album a)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE albums SET title=@t, artist=@ar, year=@y, genre_id=@g WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("t", a.Title);
        cmd.Parameters.AddWithValue("ar", a.Artist);
        cmd.Parameters.AddWithValue("y", (object?)a.Year ?? DBNull.Value);
        cmd.Parameters.AddWithValue("g", a.GenreId);
        cmd.Parameters.AddWithValue("id", a.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM albums WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Album Map(NpgsqlDataReader r) => new()
    {
        Id       = r.GetInt32(0),
        Title    = r.GetString(1),
        Artist   = r.GetString(2),
        Year     = r.IsDBNull(3) ? null : r.GetInt32(3),
        GenreId  = r.GetInt32(4),
        GenreName = r.GetString(5)
    };
}
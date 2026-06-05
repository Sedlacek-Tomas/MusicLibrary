using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicLibrary.Models;
using Npgsql;

namespace MusicLibrary.Repositories;

public class TrackRepository(string connectionString) : ITrackRepository
{
    public async Task<List<Track>> GetByAlbumIdAsync(int albumId)
    {
        var list = new List<Track>();
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            @"SELECT id, album_id, title, duration, track_no
              FROM tracks
              WHERE album_id = @albumId
              ORDER BY track_no, title", conn);
        cmd.Parameters.AddWithValue("albumId", albumId);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(new Track
            {
                Id       = reader.GetInt32(0),
                AlbumId  = reader.GetInt32(1),
                Title    = reader.GetString(2),
                Duration = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                TrackNo  = reader.IsDBNull(4) ? null : reader.GetInt32(4)
            });
        return list;
    }

    public async Task AddAsync(Track t)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO tracks (album_id, title, duration, track_no) VALUES (@a, @ti, @d, @n)", conn);
        cmd.Parameters.AddWithValue("a", t.AlbumId);
        cmd.Parameters.AddWithValue("ti", t.Title);
        cmd.Parameters.AddWithValue("d", (object?)t.Duration ?? DBNull.Value);
        cmd.Parameters.AddWithValue("n", (object?)t.TrackNo ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Track t)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE tracks SET title=@ti, duration=@d, track_no=@n WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("ti", t.Title);
        cmd.Parameters.AddWithValue("d", (object?)t.Duration ?? DBNull.Value);
        cmd.Parameters.AddWithValue("n", (object?)t.TrackNo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("id", t.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM tracks WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
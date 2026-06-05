using System.Collections.Generic;
using System.Threading.Tasks;
using MusicLibrary.Models;

namespace MusicLibrary.Repositories;

public interface ITrackRepository
{
    Task<List<Track>> GetByAlbumIdAsync(int albumId);
    Task AddAsync(Track track);
    Task UpdateAsync(Track track);
    Task DeleteAsync(int id);
}
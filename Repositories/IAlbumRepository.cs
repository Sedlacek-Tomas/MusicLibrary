using System.Collections.Generic;
using System.Threading.Tasks;
using MusicLibrary.Models;

namespace MusicLibrary.Repositories;

public interface IAlbumRepository
{
    Task<List<Album>> GetAllAsync();
    Task<Album?> GetByIdAsync(int id);
    Task AddAsync(Album album);
    Task UpdateAsync(Album album);
    Task DeleteAsync(int id);
}
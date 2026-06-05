using System.Collections.Generic;
using System.Threading.Tasks;
using MusicLibrary.Models;

namespace MusicLibrary.Repositories;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync();
}
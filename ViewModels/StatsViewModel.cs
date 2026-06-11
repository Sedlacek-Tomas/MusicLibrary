using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicLibrary.Models;
using MusicLibrary.Repositories;

namespace MusicLibrary.ViewModels;

public record GenreStat(string Genre, int AlbumCount, int TrackCount);

public partial class StatsViewModel : ViewModelBase
{
    private readonly IAlbumRepository _albumRepo;
    private readonly ITrackRepository _trackRepo;

    public ObservableCollection<GenreStat> GenreStats { get; } = new();

    [ObservableProperty] private int _totalAlbums;
    [ObservableProperty] private int _totalTracks;
    [ObservableProperty] private string _mostPopularGenre = "-";
    [ObservableProperty] private string _mostPopularArtist = "-";

    public event Action? GoBackRequested;

    public StatsViewModel(IAlbumRepository albumRepo, ITrackRepository trackRepo)
    {
        _albumRepo = albumRepo;
        _trackRepo = trackRepo;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var albums = (await _albumRepo.GetAllAsync()).ToList();
        TotalAlbums = albums.Count;

        int trackTotal = 0;
        var tracksByAlbum = new Dictionary<int, int>();
        foreach (var album in albums)
        {
            var tracks = (await _trackRepo.GetByAlbumIdAsync(album.Id)).ToList();
            tracksByAlbum[album.Id] = tracks.Count;
            trackTotal += tracks.Count;
        }
        TotalTracks = trackTotal;

        GenreStats.Clear();
        var byGenre = albums
            .GroupBy(a => string.IsNullOrEmpty(a.GenreName) ? "(bez žánru)" : a.GenreName)
            .Select(g => new GenreStat(
                g.Key,
                g.Count(),
                g.Sum(a => tracksByAlbum.GetValueOrDefault(a.Id, 0))))
            .OrderByDescending(s => s.AlbumCount);

        foreach (var stat in byGenre)
            GenreStats.Add(stat);

        MostPopularGenre = GenreStats.FirstOrDefault()?.Genre ?? "-";
        MostPopularArtist = albums
            .GroupBy(a => a.Artist)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "-";
    }

    [RelayCommand]
    private void GoBack() => GoBackRequested?.Invoke();
}
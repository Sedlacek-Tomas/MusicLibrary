using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicLibrary.Models;
using MusicLibrary.Repositories;

namespace MusicLibrary.ViewModels;

public partial class AlbumDetailViewModel : ViewModelBase
{
    private readonly ITrackRepository _trackRepo;

    [ObservableProperty]
    private Album? _album;

    public ObservableCollection<Track> Tracks { get; } = new();

    // Formulář pro novou / editovanou skladbu
    [ObservableProperty] private string _trackTitle = string.Empty;
    [ObservableProperty] private string _trackDuration = string.Empty;  // "3:45"
    [ObservableProperty] private string _trackNo = string.Empty;
    [ObservableProperty] private string _trackError = string.Empty;

    [ObservableProperty] private Track? _editingTrack;  // null = přidáváme novou

    public event Action? GoBackRequested;
    public event Action<Album>? OpenEditFormRequested;

    public AlbumDetailViewModel(ITrackRepository trackRepo)
    {
        _trackRepo = trackRepo;
    }

    public async Task LoadAsync(Album album)
    {
        Album = album;
        Tracks.Clear();
        foreach (var t in await _trackRepo.GetByAlbumIdAsync(album.Id))
            Tracks.Add(t);
    }

    [RelayCommand]
    private void GoBack() => GoBackRequested?.Invoke();

    [RelayCommand]
    private void EditAlbum()
    {
        if (Album is not null)
            OpenEditFormRequested?.Invoke(Album);
    }

    [RelayCommand]
    private void StartEditTrack(Track? track)
    {
        if (track is null) return;
        EditingTrack = track;
        TrackTitle    = track.Title;
        TrackDuration = track.Duration.HasValue
            ? $"{track.Duration.Value / 60}:{track.Duration.Value % 60:D2}"
            : string.Empty;
        TrackNo    = track.TrackNo?.ToString() ?? string.Empty;
        TrackError = string.Empty;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditingTrack  = null;
        TrackTitle    = string.Empty;
        TrackDuration = string.Empty;
        TrackNo       = string.Empty;
        TrackError    = string.Empty;
    }

    [RelayCommand]
    private async Task SaveTrack()
    {
        if (string.IsNullOrWhiteSpace(TrackTitle))
        {
            TrackError = "Název skladby je povinný.";
            return;
        }

        int? duration = null;
        if (!string.IsNullOrWhiteSpace(TrackDuration))
        {
            duration = ParseDuration(TrackDuration);
            if (duration is null)
            {
                TrackError = "Délka musí být ve formátu m:ss (např. 3:45).";
                return;
            }
        }

        int? trackNo = null;
        if (!string.IsNullOrWhiteSpace(TrackNo))
        {
            if (!int.TryParse(TrackNo, out var n) || n < 1)
            {
                TrackError = "Číslo stopy musí být kladné celé číslo.";
                return;
            }
            trackNo = n;
        }

        TrackError = string.Empty;

        if (EditingTrack is null)
        {
            // Přidáváme novou
            var newTrack = new Track
            {
                AlbumId  = Album!.Id,
                Title    = TrackTitle.Trim(),
                Duration = duration,
                TrackNo  = trackNo
            };
            await _trackRepo.AddAsync(newTrack);
            await LoadAsync(Album!);
        }
        else
        {
            // Upravujeme existující
            EditingTrack.Title    = TrackTitle.Trim();
            EditingTrack.Duration = duration;
            EditingTrack.TrackNo  = trackNo;
            await _trackRepo.UpdateAsync(EditingTrack);
            await LoadAsync(Album!);
        }

        CancelEdit();
    }

    [RelayCommand]
    private async Task DeleteTrack(Track? track)
    {
        if (track is null) return;
        await _trackRepo.DeleteAsync(track.Id);
        Tracks.Remove(track);
    }

    // Parsuje "3:45" → 225 sekund
    private static int? ParseDuration(string input)
    {
        var parts = input.Split(':');
        if (parts.Length != 2) return null;
        if (!int.TryParse(parts[0], out var min)) return null;
        if (!int.TryParse(parts[1], out var sec)) return null;
        if (sec < 0 || sec > 59) return null;
        return min * 60 + sec;
    }
}
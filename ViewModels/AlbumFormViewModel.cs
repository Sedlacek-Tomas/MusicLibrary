using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicLibrary.Models;
using MusicLibrary.Repositories;

namespace MusicLibrary.ViewModels;

public partial class AlbumFormViewModel : ViewModelBase
{
    private readonly IAlbumRepository _albumRepo;
    private readonly IGenreRepository _genreRepo;

    private int? _editingId;  // null = přidáváme nové album

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _artist = string.Empty;
    [ObservableProperty] private string _year = string.Empty;
    [ObservableProperty] private string _titleError = string.Empty;
    [ObservableProperty] private string _artistError = string.Empty;
    [ObservableProperty] private string _yearError = string.Empty;

    public ObservableCollection<Genre> Genres { get; } = new();

    [ObservableProperty]
    private Genre? _selectedGenre;

    public string WindowTitle => _editingId is null ? "Přidat album" : "Upravit album";

    public event Action? SaveCompleted;
    public event Action? Cancelled;

    public AlbumFormViewModel(IAlbumRepository albumRepo, IGenreRepository genreRepo)
    {
        _albumRepo = albumRepo;
        _genreRepo = genreRepo;
    }

    public async Task LoadAsync(Album? albumToEdit = null)
    {
        Genres.Clear();
        foreach (var g in await _genreRepo.GetAllAsync())
            Genres.Add(g);

        if (albumToEdit is not null)
        {
            _editingId     = albumToEdit.Id;
            Title          = albumToEdit.Title;
            Artist         = albumToEdit.Artist;
            Year           = albumToEdit.Year?.ToString() ?? string.Empty;
            SelectedGenre  = Genres.FirstOrDefault(g => g.Id == albumToEdit.GenreId);
        }
        else
        {
            _editingId    = null;
            Title         = string.Empty;
            Artist        = string.Empty;
            Year          = string.Empty;
            SelectedGenre = Genres.FirstOrDefault();
        }

        TitleError  = string.Empty;
        ArtistError = string.Empty;
        YearError   = string.Empty;
    }

    [RelayCommand]
    private async Task Save()
    {
        // Validace
        TitleError  = string.IsNullOrWhiteSpace(Title)  ? "Název alba je povinný."   : string.Empty;
        ArtistError = string.IsNullOrWhiteSpace(Artist) ? "Interpret je povinný."    : string.Empty;

        int? yearValue = null;
        if (!string.IsNullOrWhiteSpace(Year))
        {
            if (!int.TryParse(Year, out var y) || y < 1500 || y > 2100)
                YearError = "Rok musí být číslo mezi 1500 a 2100.";
            else
            {
                YearError  = string.Empty;
                yearValue = y;
            }
        }
        else YearError = string.Empty;

        if (TitleError != string.Empty || ArtistError != string.Empty || YearError != string.Empty)
            return;

        var album = new Album
        {
            Id      = _editingId ?? 0,
            Title   = Title.Trim(),
            Artist  = Artist.Trim(),
            Year    = yearValue,
            GenreId = SelectedGenre?.Id ?? Genres.First().Id
        };

        if (_editingId is null)
            await _albumRepo.AddAsync(album);
        else
            await _albumRepo.UpdateAsync(album);

        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}
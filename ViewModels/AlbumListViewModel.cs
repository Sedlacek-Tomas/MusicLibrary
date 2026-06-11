using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicLibrary.Models;
using MusicLibrary.Repositories;

namespace MusicLibrary.ViewModels;

public partial class AlbumListViewModel : ViewModelBase
{
    private readonly IAlbumRepository _albumRepo;
    private System.Collections.Generic.List<Album> _allAlbums = new();

    public ObservableCollection<Album> Albums { get; } = new();

    [ObservableProperty]
    private Album? _selectedAlbum;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _sortBy = "artist";

    public string[] SortOptions { get; } = { "artist", "title", "year" };

    public event Action<Album>? OpenDetailRequested;
    public event Action? OpenAddFormRequested;
    public event Action? OpenStatsRequested;

    public AlbumListViewModel(IAlbumRepository albumRepo)
    {
        _albumRepo = albumRepo;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        _allAlbums = (await _albumRepo.GetAllAsync()).ToList();
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSortByChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = _allAlbums.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(s) ||
                a.Artist.ToLower().Contains(s) ||
                a.GenreName.ToLower().Contains(s));
        }

        query = SortBy switch
        {
            "title" => query.OrderBy(a => a.Title),
            "year"  => query.OrderBy(a => a.Year),
            _       => query.OrderBy(a => a.Artist).ThenBy(a => a.Title),
        };

        Albums.Clear();
        foreach (var a in query)
            Albums.Add(a);
    }

    [RelayCommand]
    private void OpenDetail(Album? album)
    {
        if (album is not null)
            OpenDetailRequested?.Invoke(album);
    }

    [RelayCommand]
    private void OpenAddForm() => OpenAddFormRequested?.Invoke();

    [RelayCommand]
    private void OpenStats() => OpenStatsRequested?.Invoke();

    [RelayCommand]
    private async Task DeleteAlbum(Album? album)
    {
        if (album is null) return;
        await _albumRepo.DeleteAsync(album.Id);
        _allAlbums.Remove(album);
        ApplyFilter();
    }
}
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicLibrary.Models;
using MusicLibrary.Repositories;

namespace MusicLibrary.ViewModels;

public partial class AlbumListViewModel : ViewModelBase
{
    private readonly IAlbumRepository _albumRepo;

    public ObservableCollection<Album> Albums { get; } = new();

    [ObservableProperty]    
    private Album? _selectedAlbum;

    // Události pro navigaci — App.axaml.cs je napojí na konkrétní akce
    public event Action<Album>? OpenDetailRequested;
    public event Action? OpenAddFormRequested;

    public AlbumListViewModel(IAlbumRepository albumRepo)
    {
        _albumRepo = albumRepo;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Albums.Clear();
        foreach (var a in await _albumRepo.GetAllAsync())
            Albums.Add(a);
    }

    [RelayCommand]
    private void OpenDetail(Album? album)
    {
        if (album is not null)
            OpenDetailRequested?.Invoke(album);
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        OpenAddFormRequested?.Invoke();
    }

    [RelayCommand]
    private async Task DeleteAlbum(Album? album)
    {
        if (album is null) return;
        await _albumRepo.DeleteAsync(album.Id);
        Albums.Remove(album);
    }
}
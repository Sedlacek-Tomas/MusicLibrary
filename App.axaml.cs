using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MusicLibrary.Models;
using MusicLibrary.ViewModels;
using MusicLibrary.Views;
using System.Linq;

namespace MusicLibrary;

public partial class App : Application
{
    private static ServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Spustí DI a načte .env
        _services = Services.Configure();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var mainVm = _services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow { DataContext = mainVm };

            // Zobraz seznam alb jako první view
            ShowAlbumList(mainVm);
        }

        base.OnFrameworkInitializationCompleted();
    }

    // ── Navigace ────────────────────────────────────────────────────────────

    private async void ShowAlbumList(MainWindowViewModel mainVm)
    {
        var vm = _services!.GetRequiredService<AlbumListViewModel>();

        vm.OpenDetailRequested  += album => ShowAlbumDetail(mainVm, album);
        vm.OpenAddFormRequested += ()    => ShowAlbumForm(mainVm, null, () => ShowAlbumList(mainVm));
        
        vm.OpenStatsRequested += () => ShowStats(mainVm);
        await vm.LoadAsync();
        mainVm.CurrentViewModel = vm;
    }

    private async void ShowAlbumDetail(MainWindowViewModel mainVm, Album album)
    {
        var vm = _services!.GetRequiredService<AlbumDetailViewModel>();

        vm.GoBackRequested       += () => ShowAlbumList(mainVm);
        vm.OpenEditFormRequested += albumToEdit =>
            ShowAlbumForm(mainVm, albumToEdit, async () =>
            {
                var freshAlbum = await _services!
                    .GetRequiredService<Repositories.IAlbumRepository>()
                    .GetByIdAsync(albumToEdit.Id);
                if (freshAlbum is not null)
                    await vm.LoadAsync(freshAlbum);
                mainVm.CurrentViewModel = vm;
            });

        await vm.LoadAsync(album);
        mainVm.CurrentViewModel = vm;
    }

    private async void ShowAlbumForm(MainWindowViewModel mainVm, Album? albumToEdit, System.Action onSaved)
    {
        var vm = _services!.GetRequiredService<AlbumFormViewModel>();

        vm.SaveCompleted += onSaved;
        vm.Cancelled     += () => ShowAlbumList(mainVm);

        await vm.LoadAsync(albumToEdit);  // ← počkej než se žánry načtou
        mainVm.CurrentViewModel = vm;     // ← pak teprve zobraz formulář
    }
    
    private async void ShowStats(MainWindowViewModel mainVm)
    {
        var vm = _services!.GetRequiredService<StatsViewModel>();
        vm.GoBackRequested += () => ShowAlbumList(mainVm);
        await vm.LoadAsync();
        mainVm.CurrentViewModel = vm;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var toRemove = BindingPlugins.DataValidators
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();
        foreach (var plugin in toRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}
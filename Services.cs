using System;
using System.IO;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using MusicLibrary.Repositories;
using MusicLibrary.ViewModels;

namespace MusicLibrary;

public static class Services
{
    public static ServiceProvider Configure()
    {
        // Najde .env soubor i při spuštění z bin/Debug/
        var basePath = AppContext.BaseDirectory;
        var envPath = Path.Combine(basePath, ".env");

    // Pokud .env není vedle .exe, hledej ho výš (kořen projektu)
        if (!File.Exists(envPath))
        {
            var dir = new DirectoryInfo(basePath);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, ".env")))
                dir = dir.Parent;
            if (dir != null)
                envPath = Path.Combine(dir.FullName, ".env");
        }

        Env.Load(envPath);

        var host    = Environment.GetEnvironmentVariable("DB_HOST");
        var port    = Environment.GetEnvironmentVariable("DB_PORT");
        var user    = Environment.GetEnvironmentVariable("DB_USER");
        var pass    = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var db      = Environment.GetEnvironmentVariable("DB_NAME");
        var connStr = $"Host={host};Port={port};Username={user};Password={pass};Database={db}";

        var services = new ServiceCollection();

        // Repositories
        services.AddSingleton<IAlbumRepository>(_ => new AlbumRepository(connStr));
        services.AddSingleton<ITrackRepository>(_ => new TrackRepository(connStr));
        services.AddSingleton<IGenreRepository>(_ => new GenreRepository(connStr));

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<AlbumListViewModel>();
        services.AddTransient<AlbumDetailViewModel>();
        services.AddTransient<AlbumFormViewModel>();

        return services.BuildServiceProvider();
    }
}
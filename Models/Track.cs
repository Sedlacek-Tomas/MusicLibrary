namespace MusicLibrary.Models;

public class Track
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Duration { get; set; }
    public int? TrackNo { get; set; }

    // Pomocná vlastnost pro zobrazení délky ve formátu mm:ss
    public string DurationFormatted =>
        Duration.HasValue
            ? $"{Duration.Value / 60}:{Duration.Value % 60:D2}"
            : "-";
}
namespace MusicLibrary.Models;

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int? Year { get; set; }
    public int GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;
}
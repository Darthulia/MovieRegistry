namespace MovieRegistry.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public int GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;
}
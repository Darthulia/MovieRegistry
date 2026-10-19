namespace MovieRegistry.Data;

public class MovieRepository(string connectionString)
{
    public List<Movie> GetAllMovies()
    {
        const string sql = """
                           SELECT m.Id, m.Title, m.ReleaseYear, m.GenreId, g.Name AS GenreName
                           FROM Movie m
                           INNER JOIN Genre g ON g.Id = m.GenreId
                           ORDER BY m.Title
                           """;

        var movies = new List<Movie>();

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            movies.Add(MapMovie(reader));
        }

        return movies;
    }

    public List<Movie> GetMoviesByGenre(string genreName)
    {
        const string sql = """
                           SELECT m.Id, m.Title, m.ReleaseYear, m.GenreId, g.Name AS GenreName
                           FROM Movie m
                           INNER JOIN Genre g ON g.Id = m.GenreId
                           WHERE g.Name = @GenreName
                           ORDER BY m.Title
                           """;

        var movies = new List<Movie>();

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@GenreName", genreName);
        connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            movies.Add(MapMovie(reader));
        }

        return movies;
    }

    public List<Genre> GetGenres()
    {
        const string sql = "SELECT Id, Name FROM Genre ORDER BY Name";

        var genres = new List<Genre>();

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            genres.Add(new Genre
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            });
        }

        return genres;
    }

    public bool AddMovie(string title, int year, int genreId)
    {
        const string sql = """
                           INSERT INTO Movie (Title, ReleaseYear, GenreId)
                           VALUES (@Title, @Year, @GenreId)
                           """;

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Title", title);
        command.Parameters.AddWithValue("@Year", year);
        command.Parameters.AddWithValue("@GenreId", genreId);
        connection.Open();

        return command.ExecuteNonQuery() == 1;
    }

    public bool DeleteMovie(int movieId)
    {
        const string sql = "DELETE FROM Movie WHERE Id = @Id";

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", movieId);
        connection.Open();

        return command.ExecuteNonQuery() == 1;
    }

    private static Movie MapMovie(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Title = reader.GetString(reader.GetOrdinal("Title")),
        Year = reader.GetInt32(reader.GetOrdinal("ReleaseYear")),
        GenreId = reader.GetInt32(reader.GetOrdinal("GenreId")),
        GenreName = reader.GetString(reader.GetOrdinal("GenreName"))
    };
}
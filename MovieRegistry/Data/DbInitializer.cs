namespace MovieRegistry.Data;

public class DbInitializer(string connectionString)
{
    private readonly string _databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

    public void Initialize()
    {
        EnsureDatabaseExists();
        EnsureTablesExist();
        EnsureSeedData();
    }

    private void EnsureDatabaseExists()
    {
        var masterCs = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        using var connection = new SqlConnection(masterCs.ConnectionString);
        connection.Open();

        using var checkCommand = new SqlCommand("SELECT DB_ID(@Name)", connection);
        checkCommand.Parameters.AddWithValue("@Name", _databaseName);

        if (checkCommand.ExecuteScalar() != DBNull.Value) return;

        var safeName = _databaseName.Replace("]", "]]");
        using var createCommand = new SqlCommand($"CREATE DATABASE [{safeName}]", connection);
        createCommand.ExecuteNonQuery();
    }

    private void EnsureTablesExist()
    {
        const string sql = """
                           IF OBJECT_ID(N'dbo.Genre', N'U') IS NULL
                           BEGIN
                               CREATE TABLE Genre (
                                   Id   INT IDENTITY(1,1) PRIMARY KEY,
                                   Name NVARCHAR(50) NOT NULL UNIQUE
                               );
                           END;

                           IF OBJECT_ID(N'dbo.Movie', N'U') IS NULL
                           BEGIN
                               CREATE TABLE Movie (
                                   Id           INT IDENTITY(1,1) PRIMARY KEY,
                                   Title        NVARCHAR(100) NOT NULL,
                                   ReleaseYear  INT NOT NULL,
                                   GenreId      INT NOT NULL,
                                   CONSTRAINT FK_Movie_Genre
                                       FOREIGN KEY (GenreId) REFERENCES Genre(Id)
                               );
                           END;
                           """;

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private void EnsureSeedData()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        SeedGenres(connection);

        if (CountMovies(connection) > 0) return;

        SeedMovie(connection, "Alien", 1979, "Sci-Fi");
        SeedMovie(connection, "The Shining", 1980, "Horror");
        SeedMovie(connection, "Die Hard", 1988, "Action");
        SeedMovie(connection, "Blade Runner", 1982, "Sci-Fi");
        SeedMovie(connection, "The Thing", 1982, "Horror");
        SeedMovie(connection, "The Godfather", 1972, "Drama");
        SeedMovie(connection, "Airplane!", 1980, "Comedy");
    }

    private static void SeedGenres(SqlConnection connection)
    {
        string[] genres = ["Sci-Fi", "Horror", "Action", "Drama", "Comedy"];

        foreach (var genre in genres)
        {
            using var command = new SqlCommand(
                "IF NOT EXISTS (SELECT 1 FROM Genre WHERE Name = @Name) " +
                "INSERT INTO Genre (Name) VALUES (@Name)",
                connection);
            command.Parameters.AddWithValue("@Name", genre);
            command.ExecuteNonQuery();
        }
    }

    private static int CountMovies(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT COUNT(*) FROM Movie", connection);
        return (int)command.ExecuteScalar()!;
    }

    private static void SeedMovie(SqlConnection connection, string title, int year, string genreName)
    {
        const string sql = """
                           INSERT INTO Movie (Title, ReleaseYear, GenreId)
                           VALUES (@Title, @Year,
                                   (SELECT Id FROM Genre WHERE Name = @GenreName))
                           """;

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Title", title);
        command.Parameters.AddWithValue("@Year", year);
        command.Parameters.AddWithValue("@GenreName", genreName);
        command.ExecuteNonQuery();
    }
}
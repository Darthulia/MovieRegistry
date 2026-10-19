namespace MovieRegistry;

internal class Program
{
    static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("MovieDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'MovieDb' is missing from appsettings.json.");

        AnsiConsole.Status()
            .Start("Preparing database...", _ => new DbInitializer(connectionString).Initialize());

        new MovieApp(new MovieRepository(connectionString)).Run();
    }
}
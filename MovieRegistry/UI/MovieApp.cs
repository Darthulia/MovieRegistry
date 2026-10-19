namespace MovieRegistry.UI;

public class MovieApp(MovieRepository repository)
{
    public void Run() => MainMenu();

    private void MainMenu()
    {
        var exit = false;
        while (!exit)
        {
            var menuOptions = new (string Text, Action Action)[]
            {
                ("Show all movies", ShowAllMovies),
                ("Search movies by genre", SearchMoviesByGenre),
                ("Add movie", AddMovie),
                ("Delete movie", DeleteMovie),
                ("Exit", () => exit = true)
            };

            Gui.DisplayMenu(menuOptions, "Movie Registry").Invoke();

            if (!exit)
            {
                Pause();
            }
        }

        AnsiConsole.MarkupLine("[cyan]Goodbye![/]");
    }

    private void ShowAllMovies()
    {
        AnsiConsole.MarkupLine("[bold]All movies[/]");
        var movies = repository.GetAllMovies();
        RenderMovies(movies);
    }

    private void SearchMoviesByGenre()
    {
        var chosenGenre = AnsiConsole.Prompt(
            new SelectionPrompt<Genre>()
                .Title("Select a genre")
                .EnableSearch()
                .UseConverter(g => g.Name)
                .AddChoices(repository.GetGenres()));

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Movies in genre[/] [cyan]{Markup.Escape(chosenGenre.Name)}[/]");

        var movies = repository.GetMoviesByGenre(chosenGenre.Name);
        RenderMovies(movies);
    }

    private void AddMovie()
    {
        AnsiConsole.MarkupLine("[bold]Add a new movie[/]");

        var genres = repository.GetGenres();
        if (genres.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]There are no genres to choose from.[/]");
            return;
        }

        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Title[/]:")
                .Validate(value => value.Trim() switch
                {
                    "" => ValidationResult.Error("[red]Title cannot be empty.[/]"),
                    { Length: > 100 } => ValidationResult.Error("[red]Title can be at most 100 characters.[/]"),
                    _ => ValidationResult.Success()
                }));

        var year = AnsiConsole.Prompt(
            new TextPrompt<int>("[green]Year[/]:")
                .Validate(value => value is >= 1888 and <= 2100
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Enter a year between 1888 and 2100.[/]")));

        var genre = AnsiConsole.Prompt(
            new SelectionPrompt<Genre>()
                .Title("[green]Select an existing genre[/]:")
                .UseConverter(g => g.Name)
                .AddChoices(genres));

        title = title.Trim();
        if (repository.AddMovie(title, year, genre.Id))
        {
            AnsiConsole.MarkupLine(
                $"[green]Added[/] {Markup.Escape(title)} ({year}) - {Markup.Escape(genre.Name)}");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]The movie could not be added.[/]");
        }
    }

    private void DeleteMovie()
    {
        var movies = repository.GetAllMovies();
        if (movies.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]There are no movies to delete.[/]");
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Movie>()
                .Title("[green]Select the movie to delete[/]:")
                .EnableSearch()
                .UseConverter(m => $"{m.Title} ({m.Year}) - {m.GenreName}")
                .AddChoices(movies));

        if (!AnsiConsole.Confirm($"[yellow]Delete[/] {Markup.Escape(selected.Title)} ({selected.Year})?"))
        {
            AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
            return;
        }

        AnsiConsole.MarkupLine(repository.DeleteMovie(selected.Id)
            ? $"[green]Deleted[/] {Markup.Escape(selected.Title)} ({selected.Year})."
            : "[red]The movie was not found.[/]");
    }

    private static void RenderMovies(List<Movie> movies)
    {
        if (movies.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No movies found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Title[/]"))
            .AddColumn(new TableColumn("[bold]Year[/]").Centered())
            .AddColumn(new TableColumn("[bold]Genre[/]"));

        foreach (var movie in movies)
        {
            table.AddRow(
                Markup.Escape(movie.Title),
                movie.Year.ToString(),
                Markup.Escape(movie.GenreName));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]{movies.Count} movie(s).[/]");
    }

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
        Console.ReadLine();
    }
}
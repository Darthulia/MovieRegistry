namespace MovieRegistry.UI;

public static class Gui
{
    public static Action DisplayMenu(
        (string Text, Action Action)[] menuOptions,
        string menuName = "No Name Specified",
        Panel? panel = null)
    {
        if (!Console.IsOutputRedirected)
            AnsiConsole.Clear();

        AnsiConsole.Write(new Panel(new FigletText(menuName)
            .Color(Color.Green)
            .Centered()));
        AnsiConsole.WriteLine();

        if (panel != null)
            AnsiConsole.Write(panel);

        return AnsiConsole.Prompt(
            new SelectionPrompt<(string Text, Action Action)>()
                .UseConverter(t => t.Text)
                .WrapAround()
                .AddChoices(menuOptions)
                .HighlightStyle(Color.Cyan1)).Action;
    }
}
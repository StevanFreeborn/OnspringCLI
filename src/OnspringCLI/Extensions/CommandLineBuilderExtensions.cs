namespace OnspringCLI.Extensions;

public static class CommandLineBuilderExtensions
{
  public static CommandLineBuilder AddHelpFigletText(this CommandLineBuilder builder)
  {
    return builder
    .UseHelp(
      ctx =>
        ctx.HelpBuilder
        .CustomizeLayout(
          _ =>
            HelpBuilder.Default
            .GetLayout()
            .Prepend(
              _ =>
                AnsiConsole.Write(
                  new FigletText("OnspringCLI")
                  .Color(Color.Orange3)
                )
            )
        )
    );
  }
}
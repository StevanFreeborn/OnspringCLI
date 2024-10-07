namespace OnspringCLI.Extensions;

[ExcludeFromCodeCoverage]
public static class CommandLineBuilderExtensions
{
  public static CommandLineBuilder AddHost(this CommandLineBuilder builder, string[] args)
  {
    return builder
      .UseHost(
        _ =>
          Host.CreateDefaultBuilder(args),
          host =>
            host
              .AddServices()
              .AddSerilog()
              .AddCommandHandlers()
      );
  }

  public static CommandLineBuilder AddFiglet(
    this CommandLineBuilder builder,
    string text
  )
  {
    return builder
    .UseHelp(
      ctx =>
        ctx.HelpBuilder
          .CustomizeLayout(
            _ =>
              HelpBuilder.Default
                .GetLayout()
                .Prepend(_ => AnsiConsole.Write(new FigletText(text).Color(Color.Orange3)))
          )
    );
  }
}
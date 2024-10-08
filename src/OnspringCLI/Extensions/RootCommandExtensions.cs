namespace OnspringCLI.Extensions;

[ExcludeFromCodeCoverage]
static class RootCommandExtensions
{
  public static CommandLineBuilder CreateBuilder(this RootCommand root)
  {
    return new CommandLineBuilder(root);
  }

  public static RootCommand AddOptions(this RootCommand root)
  {
    root.AddGlobalOption(
      new Option<string>(
        aliases: ["--source-api-key", "-sk"],
        description: "The API key to use to authenticate with an Onspring source instance."
      )
      {
        IsRequired = true,
      }
    );

    root.AddGlobalOption(
      new Option<LogEventLevel>(
        aliases: ["--log-level", "-l"],
        description: "The minimum level of log events to be written to the log file.",
        getDefaultValue: () => LogEventLevel.Debug
      )
    );

    return root;
  }

  public static RootCommand AddSubCommands(this RootCommand root)
  {
    root.AddCommand(
      new AttachmentsCommand()
    );

    root.AddCommand(
      new RecordsCommand()
    );

    return root;
  }
}
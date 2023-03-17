namespace OnspringCLI.Extensions;

static class RootCommandExtensions
{
  public static CommandLineBuilder Create(this RootCommand root)
  {
    return new CommandLineBuilder(root);
  }

  public static RootCommand AddOptions(this RootCommand root)
  {
    root.AddGlobalOption(
      new Option<string>(
        aliases: new[] { "--api-key", "-k" },
        description: "The API key to use to authenticate with Onspring."
      )
      {
        IsRequired = true,
      }
    );

    root.AddGlobalOption(
      new Option<LogEventLevel>(
        aliases: new[] { "--log-level", "-l" },
        description: "The log level to use.",
        getDefaultValue: () => LogEventLevel.Information
      )
    );

    return root;
  }

  public static RootCommand AddSubCommands(this RootCommand root)
  {
    root.AddCommand(
      new AttachmentsCommand()
    );

    return root;
  }
}
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
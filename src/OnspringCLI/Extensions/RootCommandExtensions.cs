namespace OnspringCLI.Extensions;

static class RootCommandExtensions
{
  public static CommandLineBuilder Create(this RootCommand root)
  {
    return new CommandLineBuilder(root);
  }

  public static RootCommand AddSubCommands(this RootCommand root)
  {
    root.AddCommand(
      new AttachmentsCommand()
    );

    return root;
  }
}
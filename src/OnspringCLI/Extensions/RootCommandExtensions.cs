namespace OnspringCLI.Extensions;

static class RootCommandExtensions
{
  public static CommandLineBuilder Create(this RootCommand root)
  {
    root.Handler = CommandHandler.Create(
      () => root.Invoke("--help")
    );

    return new CommandLineBuilder(root);
  }
}
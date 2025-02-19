await new RootCommand("A command-line interface for interacting with an Onspring instance.")
  .AddOptions()
  .AddSubCommands()
  .CreateBuilder()
  .UseDefaults()
  .AddFiglet("OnspringCLI")
  .AddHost(args)
  .Build()
  .InvokeAsync(args);
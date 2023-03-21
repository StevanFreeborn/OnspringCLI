await new RootCommand(
  "A command-line interface for interacting with an Onspring instance."
)
.AddOptions()
.AddSubCommands()
.CreateBuilder()
.AddHost(args)
.UseDefaults()
.AddFiglet("OnspringCLI")
.Build()
.InvokeAsync(args);
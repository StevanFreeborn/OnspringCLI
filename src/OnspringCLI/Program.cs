await new RootCommand(
  "A command-line interface for interacting with an Onspring instance."
)
.AddOptions()
.AddSubCommands()
.Create()
.UseHost(
  _ =>
    Host.CreateDefaultBuilder(args),
    host =>
      host
      .AddServices()
      .AddSerilog()
      .AddCommandHandlers()
)
.UseDefaults()
.Build()
.InvokeAsync(args);
await new RootCommand(
  "A command-line interface for interacting with an Onspring instance."
)
.AddSubCommands()
.Create()
.UseHost(
  _ =>
    Host.CreateDefaultBuilder(args),
    host =>
      host
      .UseSerilog()
      .AddServices()
      .AddCommandHandlers()
)
.UseDefaults()
.Build()
.InvokeAsync(args);
new RootCommand(
  "A command-line interface for interacting with an Onspring instance."
)
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
.Invoke(args);
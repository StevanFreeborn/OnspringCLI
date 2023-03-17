new RootCommand(
  "A command-line interface for interacting with an Onspring instance."
)
.Create()
.UseDefaults()
.Build()
.InvokeAsync(args);
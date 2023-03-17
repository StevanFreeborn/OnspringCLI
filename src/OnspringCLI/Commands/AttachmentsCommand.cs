namespace OnspringCLI.Commands;

public class AttachmentsCommand : Command
{
  public AttachmentsCommand() : base("attachments", "Manage attachments")
  {
    AddOption(
      new Option<string>(
        new[] { "--name", "-n" },
        "name"
      )
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly IConsole _console;

    public string Name { get; set; } = string.Empty;

    public Handler(IConsole console)
    {
      _console = console;
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
      _console.WriteLine($"Hello {Name}!");
      return Task.FromResult(0);
    }
  }
}
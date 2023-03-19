namespace OnspringCLI.Commands.Attachments.Delete;

public class BulkCommand : Command
{
  public BulkCommand() : base("bulk", "Delete attachments in bulk")
  {

  }

  public new class Handler : ICommandHandler
  {
    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
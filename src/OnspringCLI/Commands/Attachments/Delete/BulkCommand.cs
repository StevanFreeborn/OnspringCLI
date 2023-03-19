namespace OnspringCLI.Commands.Attachments.Delete;

public class BulkCommand : Command
{
  public BulkCommand() : base("bulk", "Delete attachments in bulk")
  {
    AddOption(
      new Option<int>(
        aliases: new[] { "--app-id", "-a" },
        description: "The app id where the attachments are held that will be deleted."
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--field-filter", "-ff" },
        description: "A comma separated list of field ids to whose attachments will be deleted.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--records-filter", "-rf" },
        description: "A comma separated list of record ids whose attachments will be deleted.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    AddOption(
      new Option<int>(
        aliases: new[] { "--report-filter", "-rpf" },
        description: "The id of the report whose records' attachments will be deleted."
      )
    );
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
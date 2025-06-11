namespace OnspringCLI.Commands.Records.Update;

public class BulkCommand : Command
{
  public BulkCommand() : base("bulk", "Update records in bulk")
  {
    AddCommand(new YearCommand());
  }
}
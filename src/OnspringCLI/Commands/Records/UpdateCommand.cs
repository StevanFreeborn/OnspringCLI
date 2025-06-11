namespace OnspringCLI.Commands.Records;

public class UpdateCommand : Command
{
  public UpdateCommand() : base("update", "Update records")
  {
    AddCommand(new BulkCommand());
  }
}
namespace OnspringCLI.Commands.Attachments;

public class DeleteCommand : Command
{
  public DeleteCommand() : base("download", "Download attachments")
  {
    AddCommand(
      new Delete.BulkCommand()
    );
  }
}
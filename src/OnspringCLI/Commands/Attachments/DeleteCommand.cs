namespace OnspringCLI.Commands.Attachments;

public class DeleteCommand : Command
{
  public DeleteCommand() : base("delete", "Delete attachments")
  {
    AddCommand(
      new Delete.BulkCommand()
    );
  }
}
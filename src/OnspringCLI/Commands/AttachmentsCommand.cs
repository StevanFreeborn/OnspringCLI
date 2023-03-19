namespace OnspringCLI.Commands;

public class AttachmentsCommand : Command
{
  public AttachmentsCommand() : base("attachments", "Manage attachments")
  {
    AddCommand(
      new ReportCommand()
    );

    AddCommand(
      new DownloadCommand()
    );

    AddCommand(
      new DeleteCommand()
    );
  }
}
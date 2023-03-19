namespace OnspringCLI.Commands.Attachments;

public class DownloadCommand : Command
{
  public DownloadCommand() : base("download", "Download attachments")
  {
    AddCommand(
      new BulkDownloadCommand()
    );
  }
}
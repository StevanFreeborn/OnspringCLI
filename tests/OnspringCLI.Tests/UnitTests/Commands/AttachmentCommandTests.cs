namespace OnspringCLI.Tests.UnitTests.Commands;

public class AttachmentCommandTests
{
  [Fact]
  public void AttachmentCommand_WhenCalled_ReturnsNewInstance()
  {
    var attachmentCommand = new AttachmentsCommand();

    attachmentCommand.Should().NotBeNull();
    attachmentCommand.Name.Should().Be("attachments");
    attachmentCommand.Description.Should().Be("Manage attachments");
  }

  [Fact]
  public void AttachmentCommand_WhenCalled_ItShouldHaveAReportCommand()
  {
    var attachmentCommand = new AttachmentsCommand();

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "report"
    ).Should().NotBeNull().And.BeOfType<ReportCommand>();
  }

  [Fact]
  public void AttachmentCommand_WhenCalled_ItShouldHaveADeleteCommand()
  {
    var attachmentCommand = new AttachmentsCommand();

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "delete"
    ).Should().NotBeNull().And.BeOfType<DeleteCommand>();
  }

  [Fact]
  public void AttachmentCommand_WhenCalled_ItShouldHaveADownloadCommand()
  {
    var attachmentCommand = new AttachmentsCommand();

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "download"
    ).Should().NotBeNull().And.BeOfType<DownloadCommand>();
  }

  [Fact]
  public void AttachmentCommand_WhenCalled_ItShouldHaveATransferCommand()
  {
    var attachmentCommand = new AttachmentsCommand();

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "transfer"
    ).Should().NotBeNull().And.BeOfType<TransferCommand>();
  }
}
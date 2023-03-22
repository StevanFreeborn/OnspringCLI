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
    attachmentCommand.Subcommands.Should().NotBeEmpty();
    attachmentCommand.Subcommands.Should().HaveCount(4);

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "report"
    ).Should().NotBeNull();

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "download"
    ).Should().NotBeNull().And.BeOfType<DownloadCommand>();

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "delete"
    ).Should().NotBeNull().And.BeOfType<DeleteCommand>();

    attachmentCommand.Subcommands.FirstOrDefault(
      x => x.Name == "transfer"
    ).Should().NotBeNull().And.BeOfType<TransferCommand>();
  }
}
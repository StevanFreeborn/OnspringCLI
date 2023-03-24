using BulkCommand = OnspringCLI.Commands.Attachments.Download.BulkCommand;

namespace OnspringCLI.Tests.UnitTests.Commands.Attachments;

public class DownloadCommandTests
{
  [Fact]
  public void DownloadCommand_WhenCalled_ReturnsNewInstance()
  {
    var downloadCommand = new DownloadCommand();

    downloadCommand.Should().NotBeNull();
    downloadCommand.Name.Should().Be("download");
    downloadCommand.Description.Should().Be("Download attachments");
  }

  [Fact]
  public void DownloadCommand_WhenCalled_ItShouldHaveABulkCommand()
  {
    var downloadCommand = new DownloadCommand();

    downloadCommand.Subcommands.FirstOrDefault(
      x => x.Name == "bulk"
    ).Should().NotBeNull().And.BeOfType<BulkCommand>();
  }
}
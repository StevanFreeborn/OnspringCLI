using BulkCommand = OnspringCLI.Commands.Attachments.Download.BulkCommand;

namespace OnspringCLI.Tests.UnitTests.Commands.Download;

public class DownloadCommandTests
{
  [Fact]
  public void DownloadCommand_WhenCalled_ReturnsNewInstance()
  {
    var deleteCommand = new DownloadCommand();

    deleteCommand.Should().NotBeNull();
    deleteCommand.Name.Should().Be("download");
    deleteCommand.Description.Should().Be("Download attachments");
    deleteCommand.Subcommands.FirstOrDefault(
      x => x.Name == "bulk"
    ).Should().NotBeNull().And.BeOfType<BulkCommand>();
  }
}
namespace OnspringCLI.Tests.UnitTests.Commands;

public class RecordsCommandTests
{
  [Fact]
  public void RecordsCommand_WhenCalled_ReturnsNewInstance()
  {
    var recordsCommand = new RecordsCommand();

    recordsCommand.Should().NotBeNull();
    recordsCommand.Name.Should().Be("records");
    recordsCommand.Description.Should().Be("Manage records");
  }
}
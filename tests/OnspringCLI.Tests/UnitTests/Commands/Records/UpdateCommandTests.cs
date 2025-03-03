namespace OnspringCLI.Tests.UnitTests.Commands.Records;

public class UpdateCommandTests
{
  [Fact]
  public void UpdateCommand_WhenCalled_ReturnsNewInstance()
  {
    var updateCommand = new UpdateCommand();

    updateCommand.Should().NotBeNull();
    updateCommand.Name.Should().Be("update");
    updateCommand.Description.Should().Be("Update records");
  }

  [Fact]
  public void UpdateCommand_WhenCalled_ItShouldHaveABulkCommand()
  {
    var updateCommand = new UpdateCommand();

    updateCommand.Subcommands
      .FirstOrDefault(static x => x.Name == "bulk")
      .Should()
      .NotBeNull().And.BeOfType<BulkCommand>();
  }
}
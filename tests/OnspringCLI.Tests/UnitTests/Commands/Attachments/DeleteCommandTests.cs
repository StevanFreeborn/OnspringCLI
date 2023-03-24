using BulkCommand = OnspringCLI.Commands.Attachments.Delete.BulkCommand;

namespace OnspringCLI.Tests.UnitTests.Commands.Attachments;

public class DeleteCommandTests
{
  [Fact]
  public void DeleteCommand_WhenCalled_ReturnsNewInstance()
  {
    var deleteCommand = new DeleteCommand();

    deleteCommand.Should().NotBeNull();
    deleteCommand.Name.Should().Be("delete");
    deleteCommand.Description.Should().Be("Delete attachments");
  }

  [Fact]
  public void DeleteCommand_WhenCalled_ItShouldHaveABulkCommand()
  {
    var deleteCommand = new DeleteCommand();

    deleteCommand.Subcommands.FirstOrDefault(
      x => x.Name == "bulk"
    ).Should().NotBeNull().And.BeOfType<BulkCommand>();
  }
}
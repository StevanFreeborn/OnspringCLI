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

  [Fact]
  public void RecordsCommand_WhenCalled_ItShouldHaveAFindCommand()
  {
    var recordsCommand = new RecordsCommand();

    recordsCommand.Subcommands
      .FirstOrDefault(static x => x.Name == "find")
      .Should()
      .NotBeNull().And.BeOfType<FindCommand>();
  }

  [Fact]
  public void RecordsCommand_WhenCalled_ItShouldHaveAnUpdateCommand()
  {
    var recordsCommand = new RecordsCommand();

    recordsCommand.Subcommands
      .FirstOrDefault(static x => x.Name == "update")
      .Should()
      .NotBeNull().And.BeOfType<UpdateCommand>();
  }
}
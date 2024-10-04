namespace OnspringCLI.Tests.UnitTests.Commands.Records;

public class FindCommandTests
{
  [Fact]
  public void FindCommand_WhenCalled_ReturnsNewInstance()
  {
    var findCommand = new FindCommand();

    findCommand.Should().NotBeNull();
    findCommand.Name.Should().Be("find");
    findCommand.Description.Should().Be("Find records");
  }

  [Fact]
  public void FindCommand_WhenCalled_ItShouldHaveAReferencesCommand()
  {
    var findCommand = new FindCommand();

    findCommand.Subcommands
      .FirstOrDefault(x => x.Name == "references")
      .Should()
      .NotBeNull().And.BeOfType<ReferencesCommand>();
  }
}
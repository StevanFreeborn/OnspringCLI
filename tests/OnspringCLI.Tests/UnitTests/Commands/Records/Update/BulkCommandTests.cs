namespace OnspringCLI.Tests.UnitTests.Commands.Records.Update;

public class BulkCommandTests
{
  [Fact]
  public void BulkCommand_WhenCalled_ReturnsNewInstance()
  {
    var bulkCommand = new BulkCommand();

    bulkCommand.Should().NotBeNull();
    bulkCommand.Name.Should().Be("bulk");
    bulkCommand.Description.Should().Be("Update records in bulk");
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveAYearCommand()
  {
    var bulkCommand = new BulkCommand();

    bulkCommand.Subcommands
      .FirstOrDefault(static x => x.Name == "year")
      .Should()
      .NotBeNull().And.BeOfType<YearCommand>();
  }
}
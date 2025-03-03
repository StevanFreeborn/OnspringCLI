namespace OnspringCLI.Tests.UnitTests.Commands.Records.Update.Bulk;

public class YearCommandTests
{
  [Fact]
  public void YearCommand_WhenCalled_ReturnsNewInstance()
  {
    var yearCommand = new YearCommand();

    yearCommand.Should().NotBeNull();
    yearCommand.Name.Should().Be("year");
    yearCommand.Description.Should().Be("Adjusts the value of a list and/or date field by given years");
  }
}
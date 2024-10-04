namespace OnspringCLI.Tests.UnitTests.Commands.Records.Find;

public class ReferencesCommandTests
{
  [Fact]
  public void ReferencesCommand_WhenCalled_ReturnsNewInstance()
  {
    var referencesCommand = new ReferencesCommand();

    referencesCommand.Should().NotBeNull();
    referencesCommand.Name.Should().Be("references");
    referencesCommand.Description.Should().Be("Locate references to records");
  }
}
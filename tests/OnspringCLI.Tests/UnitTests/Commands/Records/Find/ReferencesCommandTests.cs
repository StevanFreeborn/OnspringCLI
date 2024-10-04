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

  [Fact]
  public void ReferencesCommand_WhenCalled_ItShouldHaveARequiredAppIdOption()
  {
    var referencesCommand = new ReferencesCommand();

    referencesCommand.Options
      .FirstOrDefault(x => x.Name == "app-id")
      .Should()
      .NotBeNull().And.BeOfType<Option<int>>();
  }

  [Fact]
  public void ReferencesCommand_WhenCalled_ItShouldHaveARequiredRecordIdsOption()
  {
    var referencesCommand = new ReferencesCommand();

    referencesCommand.Options
      .FirstOrDefault(x => x.Name == "record-ids")
      .Should()
      .NotBeNull().And.BeOfType<Option<List<int>>>();
  }

  [Fact]
  public void ReferencesCommand_WhenCalled_ItShouldHaveAnOptionalOutputDirectoryOption()
  {
    var referencesCommand = new ReferencesCommand();

    var outputDirectoryOption = referencesCommand.Options.FirstOrDefault(o => o.Name == "output-directory");

    outputDirectoryOption.Should().NotBeNull();
    outputDirectoryOption.Should().BeOfType<Option<string>>();
    outputDirectoryOption!.IsRequired.Should().BeFalse();
  }
}
namespace OnspringCLI.Tests.UnitTests.Models;

public class AttachmentTransferSettingsTests
{
  [Fact]
  public void AttachmentTransferSettings_WhenCalled_ReturnsNewInstance()
  {
    var settings = new AttachmentTransferSettings();

    settings.Should().NotBeNull();
  }

  [Fact]
  public void AttachmentTransferSettings_WhenInitialized_ItShouldBeAbleToSetProperties()
  {
    var settings = new AttachmentTransferSettings
    {
      SourceAppId = 1,
      TargetAppId = 2,
      SourceMatchFieldId = 3,
      TargetMatchFieldId = 4,
      AttachmentFieldMappings = new Dictionary<string, int>
      {
        { "1", 2 },
        { "3", 4 },
        { "invalid", 6 },
      },
      ProcessFlagFieldId = 5,
      ProcessFlagValue = "6",
      ProcessedFlagValue = "7",
      ProcessFlagListValueId = Guid.NewGuid(),
      ProcessedFlagListValueId = Guid.NewGuid(),
    };

    settings.SourceAppId.Should().Be(1);
    settings.TargetAppId.Should().Be(2);
    settings.SourceMatchFieldId.Should().Be(3);
    settings.TargetMatchFieldId.Should().Be(4);
    settings.AttachmentFieldMappings.Should().BeEquivalentTo(
      new Dictionary<string, int>
      {
        { "1", 2 },
        { "3", 4 },
        { "invalid", 6 },
      }
    );
    settings.AttachmentFieldIdMappings.Should().BeEquivalentTo(
      new Dictionary<int, int>
      {
        { 1, 2 },
        { 3, 4 },
        { 0, 6 },
      }
    );
    settings.ProcessFlagFieldId.Should().Be(5);
    settings.ProcessFlagValue.Should().Be("6");
    settings.ProcessedFlagValue.Should().Be("7");
    settings.ProcessFlagListValueId.Should().NotBe(Guid.Empty);
    settings.ProcessedFlagListValueId.Should().NotBe(Guid.Empty);
  }
}
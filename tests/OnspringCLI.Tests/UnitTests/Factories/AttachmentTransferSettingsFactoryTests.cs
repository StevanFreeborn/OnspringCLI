namespace OnspringCLI.Tests.UnitTests.Factories;

public class AttachmentTransferSettingsFactoryTests
{
  [Fact]
  public void AttachmentTransferSettingsFactory_WhenCalled_ReturnsNewInstance()
  {
    var attachmentTransferSettingsFactory = new AttachmentTransferSettingsFactory();

    attachmentTransferSettingsFactory.Should().NotBeNull();
  }

  [Fact]
  public void Create_WhenCalled_ReturnsNewInstance()
  {
    var settingsFilePath = TestSettingsFileFactory.GetTestTransferSettingsFilePath();
    var settingsFile = new FileInfo(settingsFilePath);

    var attachmentTransferSettingsFactory = new AttachmentTransferSettingsFactory();
    var attachmentTransferSettings = attachmentTransferSettingsFactory.Create(settingsFile);

    attachmentTransferSettings.Should().NotBeNull();
    attachmentTransferSettings.SourceAppId.Should().Be(1);
    attachmentTransferSettings.TargetAppId.Should().Be(1);
    attachmentTransferSettings.SourceMatchFieldId.Should().Be(1);
    attachmentTransferSettings.TargetMatchFieldId.Should().Be(1);
    attachmentTransferSettings.ProcessFlagFieldId.Should().Be(1);
    attachmentTransferSettings.ProcessFlagValue.Should().Be("no");
    attachmentTransferSettings.ProcessedFlagValue.Should().Be("yes");
    attachmentTransferSettings.AttachmentFieldMappings.Should().BeEquivalentTo(
      new Dictionary<string, int>
      {
        { "1", 1 },
        { "2", 1 },
      }
    );
    attachmentTransferSettings.AttachmentFieldIdMappings.Should().BeEquivalentTo(
      new Dictionary<int, int>
      {
        { 1, 1 },
        { 2, 1 },
      }
    );
  }
}
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

  public class HandlerTests
  {
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IRecordsProcessor> _processorMock;
    private readonly ReferencesCommand.Handler _handler;
    private readonly ReferencesCommand _command;

    public HandlerTests()
    {
      _loggerMock = new Mock<ILogger>();
      _processorMock = new Mock<IRecordsProcessor>();

      _loggerMock
        .Setup(x => x.ForContext<It.IsAnyType>())
        .Returns(_loggerMock.Object);

      _handler = new ReferencesCommand.Handler(
        _loggerMock.Object,
        _processorMock.Object
      );

      _command = [];
      _command.SetHandler(_handler.InvokeAsync);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoAppsAreFound_ItShouldReturnNonZeroValue()
    {
      _processorMock
        .Setup(x => x.GetApps())
        .ReturnsAsync([]);

      var options = OptionsFactory.RequiredReferencesOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().NotBe(0);

      _processorMock.Verify(x => x.GetApps(), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoReferenceFieldsAreFound_ItShouldReturnNonZeroValue()
    {
      var apps = new List<App>()
      {
        new() { Id = 1, Name = "App 1" },
        new() { Id = 2, Name = "App 2" }
      };

      _processorMock
        .Setup(x => x.GetApps())
        .ReturnsAsync(apps);

      _processorMock
        .Setup(x => x.GetReferenceFields(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync([]);

      var options = OptionsFactory.RequiredReferencesOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().NotBe(0);

      _processorMock.Verify(x => x.GetApps(), Times.Once);
      _processorMock.Verify(x => x.GetReferenceFields(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
      _processorMock.Verify(x => x.GetReferences(It.IsAny<App>(), It.IsAny<List<ReferenceField>>(), It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoReferencesAreFound_ItShouldReturnNonZeroValue()
    {
      var apps = new List<App>()
      {
        new() { Id = 1, Name = "App 1" },
        new() { Id = 2, Name = "App 2" }
      };

      var referenceFields = new List<ReferenceField>()
      {
        new() { Id = 1, Name = "Reference Field 1" },
        new() { Id = 2, Name = "Reference Field 2" }
      };

      _processorMock
        .Setup(x => x.GetApps())
        .ReturnsAsync(apps);

      _processorMock
        .Setup(x => x.GetReferenceFields(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(referenceFields);

      _processorMock
        .Setup(x => x.GetReferences(It.IsAny<App>(), It.IsAny<List<ReferenceField>>(), It.IsAny<List<int>>()))
        .ReturnsAsync([]);

      var options = OptionsFactory.RequiredReferencesOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().NotBe(0);

      _processorMock.Verify(x => x.GetApps(), Times.Once);
      _processorMock.Verify(x => x.GetReferenceFields(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));

      _processorMock.Verify(x => x.GetReferences(It.IsAny<App>(), It.IsAny<List<ReferenceField>>(), It.IsAny<List<int>>()), Times.Exactly(2));

      _processorMock.Verify(x => x.WriteReferencesReport(It.IsAny<List<RecordReference>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndReferencesAreFound_ItShouldReturnZeroValueAndWriteReport()
    {
      var apps = new List<App>()
      {
        new() { Id = 1, Name = "App 1" },
        new() { Id = 2, Name = "App 2" }
      };

      var referenceFields = new List<ReferenceField>()
      {
        new() { Id = 1, Name = "Reference Field 1" },
        new() { Id = 2, Name = "Reference Field 2" }
      };

      var references = new List<RecordReference>()
      {
        new(1, 1, 1, "App 1", 1, "Reference Field 1", 1),
      };

      _processorMock
        .Setup(x => x.GetApps())
        .ReturnsAsync(apps);

      _processorMock
        .Setup(x => x.GetReferenceFields(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(referenceFields);

      _processorMock
        .Setup(x => x.GetReferences(It.IsAny<App>(), It.IsAny<List<ReferenceField>>(), It.IsAny<List<int>>()))
        .ReturnsAsync(references);

      var options = OptionsFactory.RequiredReferencesOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);

      _processorMock.Verify(x => x.GetApps(), Times.Once);
      _processorMock.Verify(x => x.GetReferenceFields(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));

      _processorMock.Verify(x => x.GetReferences(It.IsAny<App>(), It.IsAny<List<ReferenceField>>(), It.IsAny<List<int>>()), Times.Exactly(2));

      _processorMock.Verify(x => x.WriteReferencesReport(It.IsAny<List<RecordReference>>(), It.IsAny<string>()), Times.Once);
    }
  }
}
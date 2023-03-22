namespace OnspringCLI.Tests.UnitTests.Commands;

public class TransferCommandTests
{
  [Fact]
  public void TransferCommand_WhenCalled_ReturnsNewInstance()
  {
    var transferCommand = new TransferCommand();

    transferCommand.Should().NotBeNull();
    transferCommand.Name.Should().Be("transfer");
    transferCommand.Description.Should().Be("Transfer attachments");
  }

  [Fact]
  public void TransferCommand_WhenCalled_ItShouldHaveARequiredTargetApiKeyOption()
  {
    var transferCommand = new TransferCommand();
    var targetApiKeyOption = transferCommand
    .Options
    .FirstOrDefault(
      x => x.Name == "target-api-key"
    );

    targetApiKeyOption.Should().NotBeNull();
    targetApiKeyOption.Should().BeOfType<Option<string>>();
    targetApiKeyOption!.IsRequired.Should().BeTrue();
  }

  [Fact]
  public void TransferCommand_WhenCalled_ItShouldHaveARequiredSettingsFileOption()
  {
    var transferCommand = new TransferCommand();
    var settingsOption = transferCommand
    .Options
    .FirstOrDefault(
      x => x.Name == "settings-file"
    );

    settingsOption.Should().NotBeNull();
    settingsOption.Should().BeOfType<Option<FileInfo>>();
    settingsOption!.IsRequired.Should().BeTrue();
  }

  [Fact]
  public void TransferCommand_WhenCalled_ItShouldHaveARecordsFilterOption()
  {
    var transferCommand = new TransferCommand();
    var recordsFilterOption = transferCommand
    .Options
    .FirstOrDefault(
      x => x.Name == "records-filter"
    );

    recordsFilterOption.Should().NotBeNull();
    recordsFilterOption.Should().BeOfType<Option<List<int>>>();
  }

  [Fact]
  public void TransferCommand_WhenCalled_ItShouldHaveAReportFilterOption()
  {
    var transferCommand = new TransferCommand();
    var reportFilterOption = transferCommand
    .Options
    .FirstOrDefault(
      x => x.Name == "report-filter"
    );

    reportFilterOption.Should().NotBeNull();
    reportFilterOption.Should().BeOfType<Option<int>>();
  }

  public class HandlerTests
  {
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IAttachmentTransferSettingsFactory> _settingsFactoryMock;
    private readonly Mock<IAttachmentsProcessor> _processorMock;
    private readonly TransferCommand.Handler _handler;
    private readonly TransferCommand _command;

    public HandlerTests()
    {
      _loggerMock = new Mock<ILogger>();
      _settingsFactoryMock = new Mock<IAttachmentTransferSettingsFactory>();
      _processorMock = new Mock<IAttachmentsProcessor>();

      _loggerMock
      .Setup(
        m => m.ForContext<It.IsAnyType>()
      )
      .Returns(
        _loggerMock.Object
      );

      _handler = new TransferCommand.Handler(
        _loggerMock.Object,
        _settingsFactoryMock.Object,
        _processorMock.Object
      );

      _command = new TransferCommand();
      _command.SetHandler(_handler.InvokeAsync);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndSettingsFileDoesNotExist_ItShouldReturnNonZeroValue()
    {
      var options = OptionsFactory.RequiredTransferOptionsWithInvalidSettingsFile;

      var result = await _command.InvokeAsync(options);

      result.Should().NotBe(0);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndMatchFieldsAreInvalid_ItShouldReturnNonZeroValue()
    {
      _processorMock
      .Setup(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(false);

      var options = OptionsFactory.RequiredTransferOptionsWithValidSettingsFile;

      var result = await _command.InvokeAsync(options);

      result.Should().Be(1);

      _processorMock
      .Verify(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndFlagFieldOrFlagValuesAreInvalid_ItShouldReturnNonZeroValue()
    {
      _processorMock
      .Setup(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(false);

      var options = OptionsFactory.RequiredTransferOptionsWithValidSettingsFile;

      var result = await _command.InvokeAsync(options);

      result.Should().Be(2);

      _processorMock
      .Verify(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoSourceRecordsAreFound_ItShouldReturnNonZeroValue()
    {
      _processorMock
      .Setup(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<ResultRecord>()
      );

      var options = OptionsFactory.RequiredTransferOptionsWithValidSettingsFile;

      var result = await _command.InvokeAsync(options);

      result.Should().Be(3);

      _processorMock
      .Verify(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndSourceRecordsAreFound_ItShouldProcessRecordsAndReturnZeroValue()
    {
      _processorMock
      .Setup(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<ResultRecord>()
        {
          new ResultRecord()
        }
      );

      _processorMock
      .Setup(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        )
      );

      var options = OptionsFactory.RequiredTransferOptionsWithValidSettingsFile;

      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);

      _processorMock
      .Verify(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        ),
        Times.Once
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndReportFilterProvided_ItShouldRetrieveReportAndAddRecordIdsToRecordFilter()
    {
      _processorMock
      .Setup(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(
        new List<int>()
        {
          1
        }
      );

      _processorMock
      .Setup(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<ResultRecord>()
        {
          new ResultRecord()
        }
      );

      _processorMock
      .Setup(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        )
      );

      _handler.ReportFilter = 1;

      var options = OptionsFactory
      .RequiredTransferOptionsWithValidSettingsFile;

      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);
      _handler.RecordsFilter.Should().BeEquivalentTo(new List<int>() { 1 });

      _processorMock
      .Verify(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        ),
        Times.Once
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledWithAllOptions_ItShouldReturnZeroValue()
    {
      _processorMock
      .Setup(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        )
      )
      .ReturnsAsync(true);

      _processorMock
      .Setup(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(
        new List<int>()
        {
          3
        }
      );

      _processorMock
      .Setup(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<ResultRecord>()
        {
          new ResultRecord()
        }
      );

      _processorMock
      .Setup(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        )
      );

      _handler.ReportFilter = 1;
      _handler.RecordsFilter = new List<int>() { 1, 2 };

      var options = OptionsFactory.AllTransferOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);
      _handler.RecordsFilter.Should().BeEquivalentTo(new List<int>() { 1, 2, 3 });

      _processorMock
      .Verify(
        x => x.ValidateMatchFields(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.ValidateFlagFieldIdAndValues(
          It.IsAny<AttachmentTransferSettings>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.GetSourceRecordsToProcess(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        x => x.TransferAttachments(
          It.IsAny<AttachmentTransferSettings>(),
          It.IsAny<ResultRecord>()
        ),
        Times.Once
      );
    }

    [Fact]
    public void Invoke_WhenCalled_ItShouldThrowAnException()
    {
      var action = () => _handler.Invoke(
        It.IsAny<InvocationContext>()
      );

      action.Should().Throw<NotImplementedException>();
    }

  }
}
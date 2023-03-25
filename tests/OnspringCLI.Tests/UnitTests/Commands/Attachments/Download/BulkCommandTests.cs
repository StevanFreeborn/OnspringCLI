using BulkCommand = OnspringCLI.Commands.Attachments.Download.BulkCommand;

namespace OnspringCLI.Tests.UnitTests.Commands.Attachments.Download;

public class BulkCommandTests
{
  [Fact]
  public void BulkCommand_WhenCalled_ItShouldCreateANewInstance()
  {
    var bulkCommand = new BulkCommand();
    bulkCommand.Should().NotBeNull();
    bulkCommand.Name.Should().Be("bulk");
    bulkCommand.Description.Should().Be("Download attachments in bulk");
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveARequiredAppIdOption()
  {
    var bulkCommand = new BulkCommand();
    var appIdOption = bulkCommand
    .Options
    .FirstOrDefault(
      o => o.Name == "app-id"
    );

    appIdOption.Should().NotBeNull();
    appIdOption.Should().BeOfType<Option<int>>();
    appIdOption!.IsRequired.Should().BeTrue();
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveAnOutputDirectoryOption()
  {
    var bulkCommand = new BulkCommand();
    var outputDirectoryOption = bulkCommand
    .Options
    .FirstOrDefault(
      o => o.Name == "output-directory"
    );

    outputDirectoryOption.Should().NotBeNull();
    outputDirectoryOption.Should().BeOfType<Option<string>>();
    outputDirectoryOption!.IsRequired.Should().BeFalse();
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveAFieldFilterOption()
  {
    var bulkCommand = new BulkCommand();
    var fieldFilterOption = bulkCommand
    .Options
    .FirstOrDefault(
      o => o.Name == "fields-filter"
    );

    fieldFilterOption.Should().NotBeNull();
    fieldFilterOption.Should().BeOfType<Option<List<int>>>();
    fieldFilterOption!.IsRequired.Should().BeFalse();
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveARecordsFilterOption()
  {
    var bulkCommand = new BulkCommand();
    var recordsFilterOption = bulkCommand
    .Options
    .FirstOrDefault(
      o => o.Name == "records-filter"
    );

    recordsFilterOption.Should().NotBeNull();
    recordsFilterOption.Should().BeOfType<Option<List<int>>>();
    recordsFilterOption!.IsRequired.Should().BeFalse();
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveAReportFilterOption()
  {
    var bulkCommand = new BulkCommand();
    var reportFilterOption = bulkCommand
    .Options
    .FirstOrDefault(
      o => o.Name == "report-filter"
    );

    reportFilterOption.Should().NotBeNull();
    reportFilterOption.Should().BeOfType<Option<int>>();
    reportFilterOption!.IsRequired.Should().BeFalse();
  }

  public class HandlerTests
  {
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IAttachmentsProcessor> _processorMock;
    private readonly BulkCommand.Handler _handler;
    private readonly BulkCommand _command;

    public HandlerTests()
    {
      _loggerMock = new Mock<ILogger>();
      _processorMock = new Mock<IAttachmentsProcessor>();

      _loggerMock
      .Setup(
        m => m.ForContext<It.IsAnyType>()
      )
      .Returns(
        _loggerMock.Object
      );

      _handler = new BulkCommand.Handler(
        _loggerMock.Object,
        _processorMock.Object
      );

      _command = new BulkCommand();
      _command.SetHandler(_handler.InvokeAsync);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoFileFilesAreFound_ItShouldReturnNonZeroValue()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<Field>()
      );

      var options = OptionsFactory.RequiredBulkDownloadOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(1);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.WriteFileRequestErrorReport(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoFileRequestsAreFound_ItShouldReturnNonZeroValue()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<Field>
        {
          new Field()
        }
      );

      _processorMock
      .Setup(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>()
      );

      var options = OptionsFactory.RequiredBulkDownloadOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(2);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.WriteFileRequestErrorReport(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndFileRequestsAreFound_ItShouldReturnZeroValue()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<Field>
        {
          new Field()
        }
      );

      _processorMock
      .Setup(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>
        {
          new OnspringFileRequest()
        }
      );

      _processorMock
      .Setup(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>()
      );

      var options = OptionsFactory.RequiredBulkDownloadOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.WriteFileRequestErrorReport(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndFileRequestsResultInError_ItShouldWriteFileRequestErrorReport()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<Field>
        {
          new Field()
        }
      );

      _processorMock
      .Setup(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>
        {
          new OnspringFileRequest()
        }
      );

      _processorMock
      .Setup(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>
        {
          new OnspringFileRequest()
        }
      );


      var options = OptionsFactory.RequiredBulkDownloadOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.WriteFileRequestErrorReport(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Once
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndReportFilterOptionIsGiven_ItShouldAddReportRecordIdsToRecordFilter()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<Field>
        {
          new Field()
        }
      );

      _processorMock
      .Setup(
        m => m.GetRecordIdsFromReport(
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(
        new List<int>
        {
          1
        }
      );

      _processorMock
      .Setup(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>
        {
          new OnspringFileRequest()
        }
      );

      _processorMock
      .Setup(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>()
      );

      _handler.ReportFilter = 1;
      var options = OptionsFactory.AllBulkDownloadOptions;
      var result = await _command.InvokeAsync(options);
      _handler.RecordsFilter.Should().BeEquivalentTo(new List<int> { 1 });

      result.Should().Be(0);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetRecordIdsFromReport(
          It.IsAny<int>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          null,
          It.IsAny<List<int>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.TryDownloadFiles(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.WriteFileRequestErrorReport(
          It.IsAny<List<OnspringFileRequest>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );
    }

    [Fact]
    public void Invoke_WhenCalled_ItShouldThrowException()
    {
      var action = () => _handler.Invoke(
        It.IsAny<InvocationContext>()
      );

      action.Should().Throw<NotImplementedException>();
    }

  }
}
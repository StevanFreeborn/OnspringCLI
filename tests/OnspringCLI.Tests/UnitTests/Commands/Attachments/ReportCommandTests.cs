namespace OnspringCLI.Tests.UnitTests.Commands.Attachments;

public class ReportCommandTests
{
  [Fact]
  public void ReportCommand_WhenCalled_ItShouldCreateANewInstance()
  {
    var command = new ReportCommand();

    command.Should().NotBeNull();
    command.Name.Should().Be("report");
    command.Description.Should().Be("Report on the attachments in an Onspring app.");
  }

  [Fact]
  public void ReportCommand_WhenCalled_ItShouldHaveARequiredAppIdOption()
  {
    var command = new ReportCommand();

    var appIdOption = command
    .Options
    .FirstOrDefault(
      o => o.Name == "app-id"
    );

    appIdOption.Should().NotBeNull();
    appIdOption.Should().BeOfType<Option<int>>();
    appIdOption!.IsRequired.Should().BeTrue();
  }

  [Fact]
  public void ReportCommand_WhenCalled_ItShouldHaveAnOutputDirectoryOption()
  {
    var command = new ReportCommand();

    var outputDirectoryOption = command
    .Options
    .FirstOrDefault(
      o => o.Name == "output-directory"
    );

    outputDirectoryOption.Should().NotBeNull();
    outputDirectoryOption.Should().BeOfType<Option<string>>();
    outputDirectoryOption!.IsRequired.Should().BeFalse();
  }

  [Fact]
  public void ReportCommand_WhenCalled_ItShouldHaveAFilesFilterOption()
  {
    var command = new ReportCommand();

    var filesFilterOption = command
    .Options
    .FirstOrDefault(
      o => o.Name == "files-filter"
    );

    filesFilterOption.Should().NotBeNull();
    filesFilterOption.Should().BeOfType<Option<List<int>>>();
    filesFilterOption!.IsRequired.Should().BeFalse();
  }

  [Fact]
  public void ReportCommand_WhenCalled_ItShouldHaveAFilesFilterCsvOption()
  {
    var command = new ReportCommand();

    var filesFilterCsvOption = command
    .Options
    .FirstOrDefault(
      o => o.Name == "files-filter-csv"
    );

    filesFilterCsvOption.Should().NotBeNull();
    filesFilterCsvOption.Should().BeOfType<Option<FileInfo>>();
    filesFilterCsvOption!.IsRequired.Should().BeFalse();
  }

  public class HandlerTests
  {
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IAttachmentsProcessor> _processorMock;
    private readonly ReportCommand.Handler _handler;
    private readonly ReportCommand _command;

    public HandlerTests()
    {
      _loggerMock = new Mock<ILogger>();
      _processorMock = new Mock<IAttachmentsProcessor>();

      _loggerMock
      .Setup(
        l => l.ForContext<It.IsAnyType>()
      )
      .Returns(
        _loggerMock.Object
      );

      _handler = new ReportCommand.Handler(
        _loggerMock.Object,
        _processorMock.Object
      );

      _command = new ReportCommand();
      _command.SetHandler(_handler.InvokeAsync);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoFileFieldsAreFound_ItShouldReturnNonZeroValue()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
        )
      )
      .ReturnsAsync(
        new List<Field>()
      );

      var options = OptionsFactory.AllReportOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(1);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
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
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.WriteFileInfoReport(
          It.IsAny<List<OnspringFileInfoResult>>(),
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
          null
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
          It.IsAny<List<int>>(),
          null
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>()
      );

      var options = OptionsFactory.AllReportOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(2);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          It.IsAny<List<int>>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        ),
        Times.Never
      );

      _processorMock
      .Verify(
        m => m.WriteFileInfoReport(
          It.IsAny<List<OnspringFileInfoResult>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoFileInfosAreFound_ItShouldReturnNonZeroValue()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
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
          It.IsAny<List<int>>(),
          null
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
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileInfoResult>()
      );

      var options = OptionsFactory.AllReportOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(3);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          It.IsAny<List<int>>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.WriteFileInfoReport(
          It.IsAny<List<OnspringFileInfoResult>>(),
          It.IsAny<string>()
        ),
        Times.Never
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndFileInfosAreFound_ItShouldReturnZeroValue()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
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
          It.IsAny<List<int>>(),
          null
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
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileInfoResult>
        {
          new OnspringFileInfoResult()
        }
      );

      var options = OptionsFactory.AllReportOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          It.IsAny<List<int>>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.WriteFileInfoReport(
          It.IsAny<List<OnspringFileInfoResult>>(),
          It.IsAny<string>()
        ),
        Times.Once
      );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndFilesFilterCsvOptionIsGiven_ItShouldAddFileIdsFromCsvToFilesFilter()
    {
      _processorMock
      .Setup(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
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
          It.IsAny<List<int>>(),
          null
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
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileInfoResult>
        {
          new OnspringFileInfoResult()
        }
      );

      var filesFilterCsvPath = TestFilesPathFactory.GetTestFilesFilterCsvPath();
      _handler.FilesFilterCsv = new FileInfo(filesFilterCsvPath);

      var options = OptionsFactory.AllReportOptions;
      var result = await _command.InvokeAsync(options);

      result.Should().Be(0);
      _handler.FilesFilterList.Should().NotBeEmpty();
      _handler.FilesFilterList.Should().HaveCount(3);
      _handler.FilesFilterList.Should().BeEquivalentTo(new List<int> { 1, 2, 3 });

      _processorMock
      .Verify(
        m => m.GetFileFields(
          It.IsAny<int>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileRequests(
          It.IsAny<int>(),
          It.IsAny<List<Field>>(),
          It.IsAny<List<int>>(),
          null
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.GetFileInfos(
          It.IsAny<List<OnspringFileRequest>>()
        ),
        Times.Once
      );

      _processorMock
      .Verify(
        m => m.WriteFileInfoReport(
          It.IsAny<List<OnspringFileInfoResult>>(),
          It.IsAny<string>()
        ),
        Times.Once
      );
    }
  }
}
using BulkCommand = OnspringCLI.Commands.Attachments.Delete.BulkCommand;

namespace OnspringCLI.Tests.UnitTests.Commands.Attachments.Delete;

public class BulkCommandTests
{
  [Fact]
  public void BulkCommand_WhenCalled_ItShouldCreateANewInstance()
  {
    var command = new BulkCommand();

    command.Should().NotBeNull();
    command.Name.Should().Be("bulk");
    command.Description.Should().Be("Delete attachments in bulk");
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveRequiredAppIdOption()
  {
    var command = new BulkCommand();

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
  public void BulkCommand_WhenCalled_ItShouldHaveFieldFilterOption()
  {
    var command = new BulkCommand();

    var fieldFilterOption = command
    .Options
    .FirstOrDefault(
      o => o.Name == "field-filter"
    );

    fieldFilterOption.Should().NotBeNull();
    fieldFilterOption.Should().BeOfType<Option<List<int>>>();
    fieldFilterOption!.IsRequired.Should().BeFalse();
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveRecordsFilterOption()
  {
    var command = new BulkCommand();

    var recordsFilterOption = command
    .Options
    .FirstOrDefault(
      o => o.Name == "records-filter"
    );

    recordsFilterOption.Should().NotBeNull();
    recordsFilterOption.Should().BeOfType<Option<List<int>>>();
    recordsFilterOption!.IsRequired.Should().BeFalse();
  }

  [Fact]
  public void BulkCommand_WhenCalled_ItShouldHaveReportFilterOption()
  {

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
    public async Task InvokeAsync_WhenCalledAndNoFileFieldsAreFound_ItShouldReturnNonZeroValue()
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

      _handler.AppId = 1;
      _handler.FieldFilter = new List<int> { 1, 2 };
      _handler.RecordsFilter = new List<int> { 1, 2 };
      _handler.ReportFilter = 1;

      var options = OptionsFactory.AllBulkDeleteOptions;
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
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

      var options = OptionsFactory.AllBulkDeleteOptions;
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
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
    public async Task InvokeAsync_WhenCalledAndFileRequestsAreFound_ItShouldReturnZero()
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>()
      );

      var options = OptionsFactory.AllBulkDeleteOptions;
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
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
    public async Task InvokeAsync_WhenCalledAndFileRequestsAreFoundAndSomeRequestsError_ItShouldReturnZeroAndWriteErrorReport()
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>
        {
          new OnspringFileRequest()
        }
      );

      var options = OptionsFactory.AllBulkDeleteOptions;
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
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
    public async Task InvokeAsync_WhenCalledAndReportFilterIsGiven_ItShouldAddReportRecordIdsToRecordsFilter()
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
        )
      )
      .ReturnsAsync(
        new List<OnspringFileRequest>()
      );

      _handler.ReportFilter = 1;
      var options = OptionsFactory.AllBulkDeleteOptions;
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
        m => m.TryDeleteFiles(
          It.IsAny<List<OnspringFileRequest>>()
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
    public void Invoke_WhenCalled_ItShouldThrowAnException()
    {
      var action = () => _handler.Invoke(
        It.IsAny<InvocationContext>()
      );

      action.Should().Throw<NotImplementedException>();
    }
  }
}
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
  }
}
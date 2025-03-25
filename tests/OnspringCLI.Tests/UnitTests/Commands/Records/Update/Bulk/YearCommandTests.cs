namespace OnspringCLI.Tests.UnitTests.Commands.Records.Update.Bulk;

public class YearCommandTests
{
  [Fact]
  public void YearCommand_WhenCalled_ReturnsNewInstance()
  {
    var yearCommand = new YearCommand();

    yearCommand.Should().NotBeNull();
    yearCommand.Name.Should().Be("year");
    yearCommand.Description.Should().Be("Adjusts the value of a list and/or date field by given years");
  }

  public class HandlerTests
  {
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IRecordsProcessor> _processorMock;
    private readonly YearCommand.Handler _handler;
    private readonly YearCommand _command;

    public HandlerTests()
    {
      _loggerMock = new Mock<ILogger>();
      _processorMock = new Mock<IRecordsProcessor>();

      _loggerMock
        .Setup(static x => x.ForContext<It.IsAnyType>())
        .Returns(_loggerMock.Object);

      _handler = new YearCommand.Handler(
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
        .Setup(static x => x.GetApps())
        .ReturnsAsync([]);

      var result = await _handler.InvokeAsync(context);

      result.Should().Be(1);
    }
  }
}
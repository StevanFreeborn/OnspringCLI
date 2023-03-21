namespace OnspringCLI.Tests.UnitTests.Services;

public class OnspringServiceTests
{
  private readonly Mock<ILogger> _loggerMock;
  private readonly Mock<IOnspringClientFactory> _clientFactoryMock;
  private readonly OnspringService _onspringService;
  public OnspringServiceTests()
  {
    _loggerMock = new Mock<ILogger>();
    _clientFactoryMock = new Mock<IOnspringClientFactory>();

    _onspringService = new OnspringService(
      _loggerMock.Object,
      _clientFactoryMock.Object
    );
  }
}
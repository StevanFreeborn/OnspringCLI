namespace OnspringCLI.Tests.UnitTests.Factories;

public class OnspringClientFactoryTests
{
  [Fact]
  public void OnspringClientFactory_WhenCalled_ReturnsNewInstance()
  {
    var onspringClientFactory = new OnspringClientFactory();

    onspringClientFactory.Should().NotBeNull();
  }

  [Fact]
  public void Create_WhenCalled_ReturnsNewInstance()
  {
    var onspringClientFactory = new OnspringClientFactory();
    var onspringClient = onspringClientFactory.Create("apiKey");

    onspringClient.Should().NotBeNull();
  }
}
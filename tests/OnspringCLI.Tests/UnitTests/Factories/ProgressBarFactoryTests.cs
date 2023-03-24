namespace OnspringCLI.Tests.UnitTests.Factories;

public class ProgressBarFactoryTests
{
  [Fact]
  public void ProgressBarFactory_WhenCalled_ReturnsNewInstance()
  {
    var progressBarFactory = new ProgressBarFactory();

    progressBarFactory.Should().NotBeNull();
  }

  [Fact]
  public void Create_WhenCalled_ReturnsNewInstance()
  {
    var progressBarFactory = new ProgressBarFactory();
    var progressBar = progressBarFactory.Create(
      10,
      "Test"
    );

    progressBar.Should().NotBeNull();
    progressBar.MaxTicks.Should().Be(10);
    progressBar.Message.Should().Be("Test");
  }
}
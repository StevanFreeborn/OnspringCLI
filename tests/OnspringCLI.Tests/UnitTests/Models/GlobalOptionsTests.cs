namespace OnspringCLI.Tests.UnitTests.Models;

public class GlobalOptionsTests
{
  [Fact]
  public void GlobalOptions_WhenCalled_ReturnsNewInstance()
  {
    var options = new GlobalOptions();

    options.Should().NotBeNull();
  }

  [Fact]
  public void GlobalOptions_WhenInitialized_ShouldBeAbleToSetItsProperties()
  {
    var sourceApiKey = "SourceKey";
    var targetApiKey = "TargetKey";
    var logLevel = LogEventLevel.Debug;

    var options = new GlobalOptions
    {
      SourceApiKey = sourceApiKey,
      TargetApiKey = targetApiKey,
      LogLevel = logLevel,
    };

    options.SourceApiKey.Should().Be(sourceApiKey);
    options.TargetApiKey.Should().Be(targetApiKey);
    options.LogLevel.Should().Be(logLevel);
  }
}
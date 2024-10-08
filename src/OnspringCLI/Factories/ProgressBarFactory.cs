namespace OnspringCLI.Factories;

public class ProgressBarFactory : IProgressBarFactory
{
  public IProgressBar Create(int maxTicks, string initialMessage)
  {
    var options = new ProgressBarOptions
    {
      ForegroundColor = ConsoleColor.DarkCyan,
      ProgressCharacter = '\u2593',
      ShowEstimatedDuration = false,
    };

    return new ProgressBar(maxTicks, initialMessage, options);
  }
}
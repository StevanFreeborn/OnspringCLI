namespace OnspringCLI.Factories;

public interface IProgressBarFactory
{
  IProgressBar Create(int maxTicks, string initialMessage);
}
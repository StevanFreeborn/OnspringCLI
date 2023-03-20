namespace OnspringCLI.Models;

public class GlobalOptions
{
  public string SourceApiKey { get; set; } = string.Empty;
  public string TargetApiKey { get; set; } = string.Empty;
  public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;
}
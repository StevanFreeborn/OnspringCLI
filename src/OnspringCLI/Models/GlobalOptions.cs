namespace OnspringCLI.Models;

public class GlobalOptions
{
  public string BaseUrl { get; } = "https://api.onspring.com";
  public string ApiKey { get; set; } = string.Empty;
  public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;
}
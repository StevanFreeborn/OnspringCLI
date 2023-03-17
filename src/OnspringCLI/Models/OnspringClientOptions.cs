namespace OnspringCLI.Models;

public class OnspringClientOptions
{
  public string BaseUrl { get; } = "https://api.onspring.com";
  public string ApiKey { get; set; } = string.Empty;
}
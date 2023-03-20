namespace OnspringCLI.Interfaces;

public interface IOnspringClientFactory
{
  IOnspringClient Create(string apiKey);
}
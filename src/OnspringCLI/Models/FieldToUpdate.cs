namespace OnspringCLI.Models;

internal record FieldToUpdate
{
  public string AppName { get; init; } = string.Empty;
  public string FieldName { get; init; } = string.Empty;

  internal FieldToUpdate()
  {
  }
}
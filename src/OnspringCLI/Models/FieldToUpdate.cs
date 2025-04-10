namespace OnspringCLI.Models;

internal record FieldToUpdate
{
  public string AppName { get; init; } = string.Empty;
  public string FieldName { get; init; } = string.Empty;

  internal FieldToUpdate()
  {
  }

  internal FieldToUpdate(string appName, string fieldName)
  {
    AppName = appName;
    FieldName = fieldName;
  }
}
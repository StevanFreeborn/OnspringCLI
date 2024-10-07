namespace OnspringCLI.Logging;

[ExcludeFromCodeCoverage]
public class OnspringFileRequestDestructuringPolicy : IDestructuringPolicy
{
  public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
  {
    if (value is not OnspringFileRequest fileRequest)
    {
      result = null;
      return false;
    }

    result = new StructureValue(
      [
        new LogEventProperty(nameof(fileRequest.RecordId), new ScalarValue(fileRequest.RecordId)),
        new LogEventProperty(nameof(fileRequest.FieldId), new ScalarValue(fileRequest.FieldId)),
        new LogEventProperty(nameof(fileRequest.FileId), new ScalarValue(fileRequest.FileId)),
        new LogEventProperty(nameof(fileRequest.FieldName), new ScalarValue(fileRequest.FieldName)),
      ]
    );

    return true;
  }
}
namespace OnspringCLI.Logging;

[ExcludeFromCodeCoverage]
public class OnspringSaveFileRequestDestructuringPolicy : IDestructuringPolicy
{
  public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
  {
    if (value is not SaveFileRequest saveFileRequest)
    {
      result = null;
      return false;
    }

    result = new StructureValue(
      [
        new LogEventProperty(nameof(saveFileRequest.RecordId), new ScalarValue(saveFileRequest.RecordId)),
        new LogEventProperty(nameof(saveFileRequest.FieldId), new ScalarValue(saveFileRequest.FieldId)),
        new LogEventProperty(nameof(saveFileRequest.ContentType), new ScalarValue(saveFileRequest.ContentType)),
        new LogEventProperty(nameof(saveFileRequest.FileName), new ScalarValue(saveFileRequest.FileName)),
        new LogEventProperty(nameof(saveFileRequest.Notes), new ScalarValue(saveFileRequest.Notes)),
        new LogEventProperty(nameof(saveFileRequest.ModifiedDate), new ScalarValue(saveFileRequest.ModifiedDate)),
      ]
    );

    return true;
  }
}
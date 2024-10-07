namespace OnspringCLI.Models;

public class RecordReference
{
  public int TargetAppId { get; init; }
  public int TargetRecordId { get; init; }
  public int SourceAppId { get; init; }
  public string SourceAppName { get; init; } = string.Empty;
  public int SourceFieldId { get; init; }
  public string SourceFieldName { get; init; } = string.Empty;
  public int SourceRecordId { get; init; }

  public RecordReference()
  {
  }

  public RecordReference(
    int targetAppId,
    int targetRecordId,
    int sourceAppId,
    string sourceAppName,
    int sourceFieldId,
    string sourceFieldName,
    int sourceRecordId
  )
  {
    TargetAppId = targetAppId;
    TargetRecordId = targetRecordId;
    SourceAppId = sourceAppId;
    SourceAppName = sourceAppName;
    SourceFieldId = sourceFieldId;
    SourceFieldName = sourceFieldName;
    SourceRecordId = sourceRecordId;
  }
}
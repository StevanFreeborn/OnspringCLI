namespace OnspringCLI.Maps;

[ExcludeFromCodeCoverage]
public class RecordReferenceMap : ClassMap<RecordReference>
{
  public RecordReferenceMap()
  {
    Map(m => m.TargetAppId).Index(0).Name("Target App Id");
    Map(m => m.TargetRecordId).Index(2).Name("Target Record Id");
    Map(m => m.SourceAppId).Index(3).Name("Source App Id");
    Map(m => m.SourceAppName).Index(4).Name("Source App Name");
    Map(m => m.SourceFieldId).Index(5).Name("Source Field Id");
    Map(m => m.SourceFieldName).Index(6).Name("Source Field Name");
    Map(m => m.SourceRecordId).Index(7).Name("Source Record Id");
  }
}
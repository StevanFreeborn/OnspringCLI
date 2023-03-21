namespace OnspringCLI.Maps;

[ExcludeFromCodeCoverage]
public class OnspringFileRequestMap : ClassMap<OnspringFileRequest>
{
  public OnspringFileRequestMap()
  {
    Map(m => m.RecordId).Index(0).Name("Record Id");
    Map(m => m.FieldId).Index(1).Name("Field Id");
    Map(m => m.FieldName).Index(2).Name("Field Name");
    Map(m => m.FileId).Index(3).Name("File Id");
  }
}
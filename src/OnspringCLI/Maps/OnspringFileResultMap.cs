namespace OnspringCLI.Maps;

[ExcludeFromCodeCoverage]
public class OnspringFileResultMap : ClassMap<OnspringFileResult>
{
  public OnspringFileResultMap()
  {
    Map(m => m.RecordId).Index(0).Name("Record Id");
    Map(m => m.FieldId).Index(1).Name("Field Id");
    Map(m => m.FieldName).Index(2).Name("Field Name");
    Map(m => m.FileId).Index(3).Name("File Id");
    Map(m => m.FileName).Index(4).Name("File Name");
    Map(m => m.FilePath).Index(5).Name("File Path");
  }
}
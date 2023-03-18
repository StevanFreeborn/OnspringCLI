namespace OnspringCLI.Maps;

public class OnspringFileInfoResultMap : ClassMap<OnspringFileInfoResult>
{
  public OnspringFileInfoResultMap()
  {
    // Map(m => m.Id).Index(0).Name("id");
    // Map(m => m.Name).Index(1).Name("name");
    Map(m => m.RecordId).Index(0).Name("Record Id");
    Map(m => m.FieldId).Index(1).Name("Field Id");
    Map(m => m.FieldName).Index(2).Name("Field Name");
    Map(m => m.FileId).Index(3).Name("File Id");
    Map(m => m.FileName).Index(4).Name("File Name");
    Map(m => m.FileSizeInBytes).Index(5).Name("File Size (Bytes)");
    Map(m => m.FileSizeInKB).Index(6).Name("File Size (KB)");
    Map(m => m.FileSizeInKiB).Index(7).Name("File Size (KiB)");
    Map(m => m.FileSizeInMB).Index(8).Name("File Size (MB)");
    Map(m => m.FileSizeInMiB).Index(9).Name("File Size (MiB)");
    Map(m => m.FileSizeInGB).Index(10).Name("File Size (GB)");
    Map(m => m.FileSizeInGiB).Index(11).Name("File Size (GiB)");
  }
}
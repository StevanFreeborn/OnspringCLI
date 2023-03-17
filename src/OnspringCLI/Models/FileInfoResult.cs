namespace OnspringCLI.Models;

public class FileInfoResult
{
  public int RecordId { get; set; }
  public int FieldId { get; set; }
  public string FieldName { get; set; }
  public int FileId { get; set; }
  public string? FileName { get; set; }
  public decimal FileSizeInBytes { get; set; }
  public decimal FileSizeInKB => Math.Round(FileSizeInBytes / 1000, 4);
  public decimal FileSizeInKiB => Math.Round(FileSizeInBytes / 1024, 4);
  public decimal FileSizeInMB => Math.Round(FileSizeInBytes / 1000000, 4);
  public decimal FileSizeInMiB => Math.Round(FileSizeInBytes / 1048576, 4);
  public decimal FileSizeInGB => Math.Round(FileSizeInBytes / 1000000000, 4);
  public decimal FileSizeInGiB => Math.Round(FileSizeInBytes / 1073741824, 4);

  public FileInfoResult(
    int recordId,
    int fieldId,
    string fieldName,
    int fileId,
    string? fileName,
    decimal fileSizeInBytes
  )
  {
    RecordId = recordId;
    FieldId = fieldId;
    FieldName = fieldName;
    FileId = fileId;
    FileName = fileName;
    FileSizeInBytes = fileSizeInBytes;
  }
}
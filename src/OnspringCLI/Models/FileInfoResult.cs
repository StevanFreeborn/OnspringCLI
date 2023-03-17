namespace OnspringCLI.Models;

public class FileInfoResult
{
  [Name("Record ID")]
  public int RecordId { get; set; }

  [Name("Field ID")]
  public int FieldId { get; set; }

  [Name("Field Name")]
  public string FieldName { get; set; }

  [Name("File ID")]
  public int FileId { get; set; }

  [Name("File Name")]
  public string? FileName { get; set; }

  [Name("File Size (Bytes)")]
  public decimal FileSizeInBytes { get; set; }

  [Name("File Size (KB)")]
  public decimal FileSizeInKB => Math.Round(FileSizeInBytes / 1000, 4);

  [Name("File Size (KiB)")]
  public decimal FileSizeInKiB => Math.Round(FileSizeInBytes / 1024, 4);

  [Name("File Size (MB)")]
  public decimal FileSizeInMB => Math.Round(FileSizeInBytes / 1000000, 4);

  [Name("File Size (MiB)")]
  public decimal FileSizeInMiB => Math.Round(FileSizeInBytes / 1048576, 4);

  [Name("File Size (GB)")]
  public decimal FileSizeInGB => Math.Round(FileSizeInBytes / 1000000000, 4);

  [Name("File Size (GiB)")]
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
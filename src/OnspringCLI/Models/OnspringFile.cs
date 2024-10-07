namespace OnspringCLI.Models;

public class OnspringFile
{
  public int RecordId { get; set; }
  public int FieldId { get; set; }
  public string? FieldName { get; set; }
  public int FileId { get; set; }
  public string? FileName { get; set; }

  public OnspringFile()
  {
  }

  public OnspringFile(int recordId, int fieldId, string fieldName, int fileId, string? fileName)
  {
    RecordId = recordId;
    FieldId = fieldId;
    FieldName = fieldName;
    FileId = fileId;
    FileName = fileName;
  }
}
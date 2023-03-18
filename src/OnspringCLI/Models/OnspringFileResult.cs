namespace OnspringCLI.Models;

public class OnspringFileResult : OnspringFile
{
  public string? FilePath { get; set; }
  public Stream Stream { get; set; }

  public OnspringFileResult(
      int recordId,
      int fieldId,
      string fieldName,
      int fileId,
      string? fileName,
      string? filePath,
      Stream fileStream

  ) : base(recordId, fieldId, fieldName, fileId, fileName)
  {
    Stream = fileStream;
    FilePath = filePath;
  }
}
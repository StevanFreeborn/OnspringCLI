namespace OnspringCLI.Tests.TestData;

public class FileDataFactory
{
  public static IEnumerable<object[]> GetFileResponse =>
  new List<object[]>
  {
    new object[]
    {
      FileResponse,
    },
  };

  public static IEnumerable<object[]> GetFileInfoResponse =>
  new List<object[]>
  {
    new object[]
    {
      FileInfoResponse,
    },
  };

  public static GetFileResponse FileResponse =>
  new()
  {
    FileName = "test.txt",
    ContentLength = 10,
    ContentType = "text/plain",
    Stream = new MemoryStream(),
  };

  public static GetFileInfoResponse FileInfoResponse =>
  new()
  {
    Name = "test.txt",
    Notes = "test notes",
    ContentType = "text/plain",
    Type = FieldType.Attachment,
    CreatedDate = DateTime.Now,
    ModifiedDate = DateTime.Now,
    Owner = "test owner",
    FileHref = "https://test.com",
  };
}
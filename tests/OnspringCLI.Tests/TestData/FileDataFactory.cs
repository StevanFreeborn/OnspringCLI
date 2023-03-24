namespace OnspringCLI.Tests.TestData;

public class FileDataFactory
{
  public static IEnumerable<object[]> InvalidOnspringFileResults =>
    new List<object[]>
    {
      new object[]
      {
        new OnspringFileResult(
          1,
          1,
          "test",
          1,
          "test",
          null,
          new MemoryStream()
        ),
      },
      new object[]
      {
        new OnspringFileResult(
          1,
          1,
          "test",
          1,
          "test",
          "/",
          new MemoryStream()
        ),
      },
    };

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

  public static IEnumerable<object[]> GetCreatedWithIdResponse =>
    new List<object[]>
    {
      new object[]
      {
        CreatedWithIdResponse,
      },
    };

  public static IEnumerable<object[]> FileRequests =>
    new List<object[]>
    {
      new object[]
      {
        new List<OnspringFileRequest>
        {
          new OnspringFileRequest(1, 1, "test", 1),
          new OnspringFileRequest(1, 1, "test", 2),
          new OnspringFileRequest(1, 2, "test", 3),
          new OnspringFileRequest(1, 2, "test", 4),
        },
      },
    };

  public static IEnumerable<object[]> FileRequestsWithResponses =>
    new List<object[]>
    {
      new object[]
      {
        new List<OnspringFileRequest>
        {
          new OnspringFileRequest(1, 1, "test", 1),
          new OnspringFileRequest(1, 1, "test", 2),
          new OnspringFileRequest(1, 2, "test", 3),
          new OnspringFileRequest(1, 2, "test", 4),
        },
        new List<GetFileResponse?>
        {
          new GetFileResponse
          {
            Stream = new MemoryStream(),
            FileName = "test1",
            ContentType = "text/plain",
            ContentLength = 100,
          },
          new GetFileResponse
          {
            Stream = new MemoryStream(),
            FileName = "test1",
            ContentType = "text/plain",
            ContentLength = 100,
          },
          new GetFileResponse
          {
            Stream = new MemoryStream(),
            FileName = "test1",
            ContentType = "text/plain",
            ContentLength = 100,
          },
          new GetFileResponse
          {
            Stream = new MemoryStream(),
            FileName = "test1",
            ContentType = "text/plain",
            ContentLength = 100,
          },
        },
      },
    };

  private static GetFileResponse FileResponse =>
    new()
    {
      FileName = "test.txt",
      ContentLength = 10,
      ContentType = "text/plain",
      Stream = new MemoryStream(),
    };

  private static GetFileInfoResponse FileInfoResponse =>
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

  private static CreatedWithIdResponse<int> CreatedWithIdResponse =>
    new()
    {
      Id = 1,
    };
}
namespace OnspringCLI.Tests.TestData;

public static class FieldDataFactory
{
  public static ApiResponse<GetPagedFieldsResponse> GetSuccessfulPagedFieldResponse(
    List<Field> fields,
    int totalPages,
    int totalRecords,
    int pageNumber
  )
  {
    return new ApiResponse<GetPagedFieldsResponse>
    {
      StatusCode = HttpStatusCode.OK,
      Message = "OK",
      Value = new GetPagedFieldsResponse
      {
        Items = fields,
        TotalPages = totalPages,
        TotalRecords = totalRecords,
        PageNumber = pageNumber,
      }
    };
  }

  public static ApiResponse<GetPagedFieldsResponse> GetFailedPagedFieldResponse(
    HttpStatusCode statusCode,
    string message
  )
  {
    return new ApiResponse<GetPagedFieldsResponse>
    {
      StatusCode = statusCode,
      Message = message,
    };
  }

  public static IEnumerable<object[]> GetOnePageOfFields =>
  new List<object[]>
  {
    new object[]
    {
      PageOneFields,
    },
  };

  public static IEnumerable<object[]> GetTwoPagesOfFields =>
  new List<object[]>
  {
    new object[]
    {
      PageOneFields,
      PageTwoFields,
    },
  };

  public static List<Field> PageOneFields =>
  new()
  {
    new Field
    {
      Id = 1,
      Name = "Field 1",
      Type = FieldType.Attachment,
    },
    new Field
    {
      Id = 2,
      Name = "Field 2",
      Type = FieldType.Attachment,
    },
    new Field
    {
      Id = 3,
      Name = "Field 3",
      Type = FieldType.Attachment,
    },
  };

  public static List<Field> PageTwoFields =>
  new()
  {
    new Field
    {
      Id = 4,
      Name = "Field 4",
      Type = FieldType.Attachment,
    },
    new Field
    {
      Id = 5,
      Name = "Field 5",
      Type = FieldType.Attachment,
    },
    new Field
    {
      Id = 6,
      Name = "Field 6",
      Type = FieldType.Attachment,
    },
  };
}
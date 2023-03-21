namespace OnspringCLI.Tests.TestData;

public static class RecordDataFactory
{
  public static ApiResponse<GetPagedRecordsResponse> GetSuccessfulPagedRecordResponse(
    List<ResultRecord> records,
    int totalPages,
    int totalRecords,
    int pageNumber
  )
  {
    return new ApiResponse<GetPagedRecordsResponse>
    {
      StatusCode = HttpStatusCode.OK,
      Message = "OK",
      Value = new GetPagedRecordsResponse
      {
        Items = records,
        TotalPages = totalPages,
        TotalRecords = totalRecords,
        PageNumber = pageNumber,
      }
    };
  }

  public static ApiResponse<GetPagedRecordsResponse> GetFailedPagedRecordResponse(
    HttpStatusCode statusCode,
    string message
  )
  {
    return new ApiResponse<GetPagedRecordsResponse>
    {
      StatusCode = statusCode,
      Message = message,
    };
  }

  public static IEnumerable<object[]> GetOnePageOfRecords =>
  new List<object[]>
  {
    new object[]
    {
      PageOneRecords,
    },
  };

  public static List<ResultRecord> PageOneRecords =>
  new()
  {
    new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>
      {
        new StringFieldValue
        {
          FieldId = 1,
          Value = "Field 1",
        },
      },
    },
    new ResultRecord
    {
      AppId = 1,
      RecordId = 2,
      FieldData = new List<RecordFieldValue>
      {
        new StringFieldValue
        {
          FieldId = 1,
          Value = "Field 1",
        },
      },
    },
  };
}
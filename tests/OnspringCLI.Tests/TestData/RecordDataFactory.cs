namespace OnspringCLI.Tests.TestData;

public static class RecordDataFactory
{
  public static IEnumerable<object[]> GetOnePageOfRecords =>
  new List<object[]>
  {
    new object[]
    {
      PageOfRecords,
    },
  };

  public static IEnumerable<object[]> GetEmptyPageOfRecords =>
  new List<object[]>
  {
    new object[]
    {
      EmptyPageOfRecords,
    },
  };

  public static IEnumerable<object[]> GetSaveRecordResponse =>
  new List<object[]>
  {
    new object[]
    {
      SaveRecordResponse,
    },
  };

  private static GetPagedRecordsResponse EmptyPageOfRecords =>
  new()
  {
    PageNumber = 1,
    TotalPages = 1,
    TotalRecords = 0,
    Items = new List<ResultRecord>(),
  };

  private static GetPagedRecordsResponse PageOfRecords =>
  new()
  {
    PageNumber = 1,
    TotalPages = 1,
    TotalRecords = 3,
    Items = PageOneRecords,
  };

  private static List<ResultRecord> PageOneRecords =>
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

  private static SaveRecordResponse SaveRecordResponse =>
  new()
  {
    Id = 1,
  };
}
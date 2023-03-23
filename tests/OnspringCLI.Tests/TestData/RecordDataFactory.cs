namespace OnspringCLI.Tests.TestData;

public static class RecordDataFactory
{
  public static List<ResultRecord> GetPageOfFileFieldValues(
    Field attachmentField,
    Field imageField
  ) =>
    new()
    {
      new ResultRecord
      {
        AppId = 1,
        RecordId = 1,
        FieldData = new List<RecordFieldValue>
        {
          new AttachmentListFieldValue
          {
            FieldId = attachmentField.Id,
            Value = new List<AttachmentFile>
            {
              new AttachmentFile
              {
                FileId = 1,
                FileName = "attachment1",
                Notes = "notes1",
                StorageLocation = FileStorageSite.Internal,
                DownloadLink = "downloadLink1",
                QuickEditLink = "quickEditLink1",
              },
              new AttachmentFile
              {
                FileId = 2,
                FileName = "attachment1",
                Notes = "notes1",
                StorageLocation = FileStorageSite.Internal,
                DownloadLink = "downloadLink1",
                QuickEditLink = "quickEditLink1",
              }
            }
          },
          new FileListFieldValue
          {
            FieldId = imageField.Id,
            Value = new List<int> { 3, 4, }
          }
        }
      },
      new ResultRecord
      {
        AppId = 1,
        RecordId = 2,
        FieldData = new List<RecordFieldValue>
        {
          new AttachmentListFieldValue
          {
            FieldId = attachmentField.Id,
            Value = new List<AttachmentFile>
            {
              new AttachmentFile
              {
                FileId = 5,
                FileName = "attachment1",
                Notes = "notes1",
                StorageLocation = FileStorageSite.Internal,
                DownloadLink = "downloadLink1",
                QuickEditLink = "quickEditLink1",
              },
              new AttachmentFile
              {
                FileId = 6,
                FileName = "attachment1",
                Notes = "notes1",
                StorageLocation = FileStorageSite.Internal,
                DownloadLink = "downloadLink1",
                QuickEditLink = "quickEditLink1",
              }
            }
          },
          new FileListFieldValue
          {
            FieldId = imageField.Id,
            Value = new List<int> { 7, 8, }
          }
        }
      }
    };

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
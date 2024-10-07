namespace OnspringCLI.Tests.TestData;

public static class RecordDataFactory
{
  public static IEnumerable<object[]> GetRecordWithFileToBeRequested =>
    new List<object[]>
    {
      new object[]
      {
        new ResultRecord
        {
          AppId = 1,
          RecordId = 1,
          FieldData =
          [
            new StringFieldValue(1, "Test"),
            new AttachmentListFieldValue(
              2,
              [
                new AttachmentFile
                {
                  FileId = 1,
                  FileName = "Test",
                  Notes = "Test",
                  StorageLocation = FileStorageSite.Internal,
                  DownloadLink = "Test",
                  QuickEditLink = "Test",
                },
                new AttachmentFile
                {
                  FileId = 2,
                  FileName = "Test",
                  Notes = "Test",
                  StorageLocation = FileStorageSite.GoogleDrive,
                  DownloadLink = "Test",
                  QuickEditLink = "Test",
                },
                new AttachmentFile
                {
                  FileId = 3,
                  FileName = "Test",
                  Notes = "Test",
                  StorageLocation = FileStorageSite.Internal,
                  DownloadLink = "Test",
                  QuickEditLink = "Test",
                },
                new AttachmentFile
                {
                  FileId = 6,
                  FileName = "Test",
                  Notes = "Test",
                  StorageLocation = FileStorageSite.Internal,
                  DownloadLink = "Test",
                  QuickEditLink = "Test",
                },
              ]
            ),
            new FileListFieldValue(
              3,
              [
                4,
                5,
              ]
            ),
            new AttachmentListFieldValue(
              4,
              [
                new AttachmentFile
                {
                  FileId = 7,
                  FileName = "Test",
                },
                new AttachmentFile
                {
                  FileId = 8,
                  FileName = "Test",
                },
              ]
            ),
            new AttachmentListFieldValue(
              5,
              [
                new AttachmentFile
                {
                  FileId = 1,
                  FileName = "Test",
                },
                new AttachmentFile
                {
                  FileId = 2,
                  FileName = "Test",
                },
                new AttachmentFile
                {
                  FileId = 3,
                  FileName = "Test",
                },
                new AttachmentFile
                {
                  FileId = 6,
                  FileName = "Test",
                },
                new AttachmentFile
                {
                  FileId = 7,
                  FileName = "Test",
                },
                new AttachmentFile
                {
                  FileId = 8,
                  FileName = "Test",
                },
              ]
            ),
          ]
        },
        new List<Field>
        {
          new Field
          {
            Id = 2,
            Name = "Test",
            Type = FieldType.Attachment,
          },
          new Field
          {
            Id = 3,
            Name = "Test",
            Type = FieldType.Image,
          },
          new Field
          {
            Id = 4,
            Name = "Test",
            Type = FieldType.Attachment,
          },
          new Field
          {
            Id = 5,
            Name = "Test",
            Type = FieldType.Attachment,
          },
        },
        new List<int>
        {
          1,
          2,
          3,
          5,
        }
      }
    };

  public static IEnumerable<object[]> GetUnTransferrableSourceRecords =>
    new List<object[]>
    {
      new object[]
      {
        AttachmentTransferSettings,
        SourceRecord,
        null!,
      },
      new object[]
      {
        AttachmentTransferSettings,
        SourceRecord,
        new GetPagedRecordsResponse
        {
          Items = [],
        }
      },
      new object[]
      {
        AttachmentTransferSettings,
        SourceRecord,
        new GetPagedRecordsResponse
        {
          Items =
          [
            new ResultRecord
            {
              AppId = 1,
              RecordId = 2,
              FieldData =
              [
                new StringFieldValue(
                  AttachmentTransferSettings.TargetMatchFieldId,
                  "Test"
                ),
              ],
            },
            new ResultRecord
            {
              AppId = 1,
              RecordId = 3,
              FieldData =
              [
                new StringFieldValue(
                  AttachmentTransferSettings.TargetMatchFieldId,
                  "Test"
                ),
              ],
            },
          ],
        }
      },
    };

  public static List<ResultRecord> GetPageOfFileFieldValues(
    Field attachmentField,
    Field imageField
  ) =>
    [
      new ResultRecord
      {
        AppId = 1,
        RecordId = 1,
        FieldData =
        [
          new AttachmentListFieldValue
          {
            FieldId = attachmentField.Id,
            Value =
            [
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
            ]
          },
          new FileListFieldValue
          {
            FieldId = imageField.Id,
            Value = [3, 4,]
          }
        ]
      },
      new ResultRecord
      {
        AppId = 1,
        RecordId = 2,
        FieldData =
        [
          new AttachmentListFieldValue
          {
            FieldId = attachmentField.Id,
            Value =
            [
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
            ]
          },
          new FileListFieldValue
          {
            FieldId = imageField.Id,
            Value = [7, 8,]
          }
        ]
      }
    ];

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

  public static AttachmentTransferSettings AttachmentTransferSettings =>
    new()
    {
      SourceMatchFieldId = 1,
      TargetMatchFieldId = 2,
      ProcessFlagFieldId = 3,
      ProcessFlagValue = "Test",
      ProcessFlagListValueId = Guid.NewGuid(),
      ProcessedFlagValue = "Tested",
      ProcessedFlagListValueId = Guid.NewGuid(),
    };

  private static ResultRecord SourceRecord =>
    new()
    {
      AppId = 1,
      RecordId = 1,
      FieldData =
      [
        new StringFieldValue(
          AttachmentTransferSettings.SourceMatchFieldId,
          "Test"
        ),
      ],
    };

  private static GetPagedRecordsResponse EmptyPageOfRecords =>
    new()
    {
      PageNumber = 1,
      TotalPages = 1,
      TotalRecords = 0,
      Items = [],
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
    [
      new ResultRecord
      {
        AppId = 1,
        RecordId = 1,
        FieldData =
        [
          new StringFieldValue
          {
            FieldId = 1,
            Value = "Field 1",
          },
        ],
      },
      new ResultRecord
      {
        AppId = 1,
        RecordId = 2,
        FieldData =
        [
          new StringFieldValue
          {
            FieldId = 1,
            Value = "Field 1",
          },
        ],
      },
    ];

  private static SaveRecordResponse SaveRecordResponse =>
    new()
    {
      Id = 1,
    };
}
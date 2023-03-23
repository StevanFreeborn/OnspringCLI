namespace OnspringCLI.Tests.TestData;

public class FieldDataFactory
{
  public static IEnumerable<object[]> GetOnePageOfFields =>
    new List<object[]>
    {
      new object[]
      {
        PageOfFields,
      },
    };

  public static IEnumerable<object[]> GetFirstPageOfFields =>
    new List<object[]>
    {
      new object[]
      {
        PageOneOfFields,
      },
    };

  public static IEnumerable<object[]> GetTwoPagesOfFields =>
    new List<object[]>
    {
      new object[]
      {
        PageOneOfFields,
        PageTwoOfFields,
      },
    };

  public static IEnumerable<object[]> GetField =>
    new List<object[]>
    {
      new object[]
      {
        Field1,
      },
    };

  public static IEnumerable<object[]> NoFileFields =>
    new List<object[]>
    {
      new object[]
      {
        new List<Field>
        {
          new Field { Id = 1, Name = "test", Type = FieldType.Text },
          new Field { Id = 2, Name = "test2", Type = FieldType.Text }
        },
      },
    };

  public static IEnumerable<object[]> HasImageField =>
    new List<object[]>
    {
      new object[]
      {
        new List<Field>
        {
          new Field { Id = 1, Name = "test", Type = FieldType.Text },
          new Field { Id = 2, Name = "test2", Type = FieldType.Image }
        },
      },
    };

  public static IEnumerable<object[]> HasAttachmentField =>
    new List<object[]>
    {
      new object[]
      {
        new List<Field>
        {
          new Field { Id = 1, Name = "test", Type = FieldType.Text },
          new Field { Id = 2, Name = "test2", Type = FieldType.Attachment }
        },
      },
    };

  public static IEnumerable<object[]> HasImageAndAttachmentField =>
    new List<object[]>
    {
      new object[]
      {
        new List<Field>
        {
          new Field { Id = 1, Name = "test", Type = FieldType.Text },
          new Field { Id = 2, Name = "test2", Type = FieldType.Image },
          new Field { Id = 3, Name = "test3", Type = FieldType.Attachment }
        },
      },
    };

  public static IEnumerable<object[]> FileFields =>
    new List<object[]>
    {
      new object[]
      {
        new List<Field>
        {
          new Field { Id = 1, Name = "attachment", Type = FieldType.Image },
          new Field { Id = 2, Name = "image", Type = FieldType.Attachment }
        },
      },
    };

  private static GetPagedFieldsResponse PageOfFields =>
    new()
    {
      PageNumber = 1,
      TotalPages = 1,
      TotalRecords = 3,
      Items = PageOneFields,
    };

  private static GetPagedFieldsResponse PageOneOfFields =>
    new()
    {
      PageNumber = 1,
      TotalPages = 2,
      TotalRecords = 6,
      Items = PageOneFields,
    };

  private static GetPagedFieldsResponse PageTwoOfFields =>
    new()
    {
      PageNumber = 2,
      TotalPages = 2,
      TotalRecords = 6,
      Items = PageTwoFields,
    };

  private static List<Field> PageOneFields =>
    new()
    {
      Field1,
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

  private static List<Field> PageTwoFields =>
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

  private static Field Field1 =>
    new()
    {
      Id = 1,
      Name = "Field 1",
      Type = FieldType.Attachment,
    };
}
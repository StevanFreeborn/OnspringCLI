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

  public static IEnumerable<object[]> GetField => new List<object[]>
  {
    new object[]
    {
      Field1,
    },
  };

  public static GetPagedFieldsResponse PageOfFields =>
  new()
  {
    PageNumber = 1,
    TotalPages = 1,
    TotalRecords = 3,
    Items = PageOneFields,
  };

  public static GetPagedFieldsResponse PageOneOfFields =>
  new()
  {
    PageNumber = 1,
    TotalPages = 2,
    TotalRecords = 6,
    Items = PageOneFields,
  };

  public static GetPagedFieldsResponse PageTwoOfFields =>
  new()
  {
    PageNumber = 2,
    TotalPages = 2,
    TotalRecords = 6,
    Items = PageTwoFields,
  };

  public static List<Field> PageOneFields =>
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

  public static Field Field1 =>
  new()
  {
    Id = 1,
    Name = "Field 1",
    Type = FieldType.Attachment,
  };
}
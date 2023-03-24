namespace OnspringCLI.Tests.TestData;

public class ReportDataFactory
{
  public static IEnumerable<object[]> GetReportData =>
    new List<object[]>
    {
      new object[]
      {
        ReportData,
      },
    };

  private static ReportData ReportData =>
    new()
    {
      Columns = new List<string>
      {
        "Column 1",
        "Column 2",
      },
      Rows = new List<ReportDataRow>
      {
        new()
        {
          RecordId = 1,
          Cells = new List<object>
          {
            "Cell 1",
            "Cell 2",
          },
        },
        new()
        {
          RecordId = 2,
          Cells = new List<object>
          {
            "Cell 1",
            "Cell 2",
          },
        }
      },
    };
}
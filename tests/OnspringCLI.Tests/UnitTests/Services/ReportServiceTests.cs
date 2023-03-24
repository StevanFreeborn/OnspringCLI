namespace OnspringCLI.Tests.UnitTests.Services;

public class ReportServiceTests
{
  private readonly Mock<ILogger> _loggerMock;
  private readonly IReportService _reportService;
  public ReportServiceTests()
  {
    _loggerMock = new Mock<ILogger>();

    _loggerMock
    .Setup(
      x => x.ForContext<It.IsAnyType>()
    )
    .Returns(
      _loggerMock.Object
    );

    _reportService = new ReportService(
      _loggerMock.Object
    );
  }

  [Fact]
  public void WriteCsvReport_WhenCalledWithDataAndNoMapClass_ItShouldWriteDataToCsv()
  {
    var directory = "output";
    var fileName = "without_map_class.csv";

    var path = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      directory,
      fileName
    );

    var data = new List<int> { 1, 2, 3 };

    _reportService.WriteCsvReport(
      data,
      null!,
      directory,
      fileName
    );

    using var reader = new StreamReader(path);

    var config = new CsvConfiguration(
      CultureInfo.InvariantCulture
    )
    {
      HasHeaderRecord = false,
    };

    using var csv = new CsvReader(reader, config);
    var csvData = csv.GetRecords<int>().ToList();

    csvData.Should().BeEquivalentTo(data);
  }

  [Fact]
  public void WriteCsvReport_WhenCalledWithDataAndMapClass_ItShouldWriteDataToCsv()
  {
    var directory = "output";
    var fileName = "with_map_class.csv";

    var path = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      directory,
      fileName
    );

    var data = new List<OnspringFileRequest>
    {
      new(1,1,1),
      new(2,2,2),
      new(3,3,3),
    };

    _reportService.WriteCsvReport(
      data,
      typeof(OnspringFileRequestMap),
      directory,
      fileName
    );

    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    csv.Context.RegisterClassMap<OnspringFileRequestMap>();
    var csvData = csv.GetRecords<OnspringFileRequest>().ToList();

    csvData.Should().BeEquivalentTo(data);
  }

  [Fact]
  public void GetReportPath_WhenCalledWithDirectoryAndFileName_ItShouldReturnPath()
  {
    var directory = "output";
    var fileName = "report.csv";

    var path = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      directory,
      fileName
    );

    var result = ReportService.GetReportPath(
      directory,
      fileName
    );

    result.Should().Be(path);
  }
}
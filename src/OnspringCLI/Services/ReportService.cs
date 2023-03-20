namespace OnspringCLI.Services;

public class ReportService : IReportService
{
  private readonly ILogger _logger;

  public ReportService(ILogger logger)
  {
    _logger = logger;
  }

  public void WriteCsvReport<T>(
    List<T> records,
    Type mapType,
    string outputDirectory,
    string fileName
  )
  {
    var reportPath = GetReportPath(outputDirectory, fileName);
    using var writer = new StreamWriter(reportPath);

    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
      ShouldQuote = (field) => false,
    };

    using var csv = new CsvWriter(writer, config);

    csv.Context.RegisterClassMap(mapType);

    _logger.Debug("Writing report to {FileName}.", fileName);

    csv.WriteRecords(records);

    _logger.Debug("Report written to {FileName}.", fileName);
  }

  internal static string GetReportPath(
    string outputDirectory,
    string fileName
  )
  {
    var outputDirectoryPath = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      outputDirectory
    );

    Directory.CreateDirectory(
      outputDirectoryPath
    );

    return Path.Combine(
      outputDirectoryPath,
      fileName
    );
  }
}
namespace OnspringCLI.Services;

public class ReportService : IReportService
{
  private readonly ILogger _logger;

  public ReportService(ILogger logger)
  {
    _logger = logger;
  }

  public void WriteCsvFileInfoReport(
    List<OnspringFileInfoResult> onspringFiles,
    string outputDirectory
  )
  {
    var fileName = GetReportPath(outputDirectory);
    using var writer = new StreamWriter(fileName);

    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
      ShouldQuote = (field) => false,
    };

    using (var csv = new CsvWriter(writer, config))
    {
      csv.Context.RegisterClassMap<OnspringFileInfoResultMap>();
      _logger.Debug("Writing report to {FileName}.", fileName);
      csv.WriteRecords(onspringFiles);
      _logger.Debug("Report written to {FileName}.", fileName);
    };
  }

  internal static string GetReportPath(string outputDirectory)
  {
    var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    var outputDirectoryPath = Path.Combine(currentDirectory, outputDirectory);

    if (!Directory.Exists(outputDirectoryPath))
    {
      Directory.CreateDirectory(outputDirectoryPath);
    }

    return Path.Combine(
      outputDirectoryPath,
      "attachment_report.csv"
    );
  }
}
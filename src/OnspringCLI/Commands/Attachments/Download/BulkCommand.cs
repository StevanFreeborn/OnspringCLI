namespace OnspringCLI.Commands.Attachments.Download;

public class BulkCommand : Command
{
  public BulkCommand() : base("bulk", "Download attachments in bulk")
  {
    AddOption(
      new Option<int>(
        aliases: new[] { "--app-id", "-a" },
        description: "The app id where the attachments are held that will be downloaded."
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<string>(
        aliases: new[] { "--output-directory", "-o" },
        description: "The name of the directory the attachments will be downloaded to.",
        getDefaultValue: () => "output"
      )
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--field-filter", "-ff" },
        description: "A comma separated list of field ids to whose attachments will be downloaded.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--records-filter", "-rf" },
        description: "A comma separated list of record ids whose attachments will be downloaded.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    AddOption(
      new Option<int>(
        aliases: new[] { "--report-filter", "-rpf" },
        description: "The id of the report whose records' attachments will be downloaded."
      )
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IAttachmentsProcessor _processor;
    public int AppId { get; set; } = 0;
    public string OutputDirectory { get; set; } = string.Empty;
    public List<int> FieldFilter { get; set; } = new();
    public List<int> RecordsFilter { get; set; } = new();
    public int ReportFilter { get; set; } = 0;

    public Handler(
      ILogger logger,
      IAttachmentsProcessor processor
    )
    {
      _logger = logger.ForContext<Handler>();
      _processor = processor;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
      _logger.Information("Starting Onspring Bulk Attachment Downloader.");

      _logger.Information("Retrieving file fields.");

      var fileFields = await _processor.GetFileFields(
        AppId,
        FieldFilter
      );

      if (fileFields.Count is 0)
      {
        _logger.Warning("No file fields found.");
        return 1;
      }

      _logger.Information(
        "File fields retrieved. {Count} file fields found.",
        fileFields.Count
      );

      if (ReportFilter is not 0)
      {
        _logger.Information(
          "Retrieving records from report {ReportId}.",
          ReportFilter
        );

        var records = await _processor.GetRecordIdsFromReport(
          ReportFilter
        );

        _logger.Information(
          "Records retrieved. {Count} records found.",
          records.Count
        );

        RecordsFilter.AddRange(records);
      }

      _logger.Information("Retrieving files that need to be downloaded.");

      var fileRequests = await _processor.GetFileRequests(
        AppId,
        fileFields,
        recordsFilter: RecordsFilter
      );

      if (fileRequests.Count is 0)
      {
        _logger.Warning("No files need to be downloaded.");
        return 2;
      }

      _logger.Information(
        "Files retrieved. {Count} files found.",
        fileRequests.Count
      );

      _logger.Information("Downloading files.");

      var outputDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        OutputDirectory
      );

      Directory.CreateDirectory(outputDirectory);

      var fileName = Path.Combine(
        outputDirectory,
        "file-list.csv"
      );

      using var writer = new StreamWriter(fileName);
      using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
      csv.Context.RegisterClassMap<OnspringFileResultMap>();

      csv.WriteHeader<OnspringFileResult>();
      csv.NextRecord();

      var erroredRequests = new List<OnspringFileRequest>();

      foreach (var fileRequest in fileRequests)
      {
        var file = await _processor.GetFile(
          fileRequest,
          OutputDirectory
        );

        if (file is null)
        {
          erroredRequests.Add(fileRequest);
          continue;
        }

        var isSaved = await _processor.TrySaveFile(file);

        if (isSaved is false)
        {
          erroredRequests.Add(fileRequest);
          continue;
        }

        csv.WriteRecord(file);
        csv.NextRecord();
      }

      if (erroredRequests.Any() is true)
      {
        _processor.WriteFileRequestErrorReport(
          erroredRequests,
          OutputDirectory
        );

        _logger.Warning(
          "Some files were not deleted. You can find a list of the files that were not deleted in the output directory: {OutputDirectory}",
          outputDirectory
        );
      }

      _logger.Information("Files downloaded.");

      _logger.Information("Onspring Bulk Attachment Downloader finished.");

      _logger.Information(
        "You can find downloaded files in the output directory: {OutputDirectory}",
        Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          OutputDirectory
        )
      );

      return 0;
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
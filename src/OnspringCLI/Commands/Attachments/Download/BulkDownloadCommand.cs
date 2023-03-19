namespace OnspringCLI.Commands.Attachments.Download;

public class BulkDownloadCommand : Command
{
  public BulkDownloadCommand() : base("bulk", "Download attachments in bulk")
  {
    AddOption(
      new Option<int>(
        aliases: new[] { "--app-id", "-a" },
        description: "The app id where the attachments are held."
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<string>(
        aliases: new[] { "--output-directory", "-o" },
        description: "The directory to download the attachments to.",
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

      foreach (var fileRequest in fileRequests)
      {
        _logger.Debug(
          "Downloading file {FileId} from field {FieldId} on record {RecordId}.",
          fileRequest.FileId,
          fileRequest.FieldId,
          fileRequest.RecordId
        );

        var file = await _processor.GetFile(
          fileRequest,
          OutputDirectory
        );

        if (file is null)
        {
          _logger.Warning(
            "File {FileId} could not be downloaded.",
            fileRequest.FileId
          );
          continue;
        }

        _logger.Debug(
          "File {FileId} downloaded.",
          file.FileId
        );

        _logger.Debug(
          "Saving file {FileId} to {OutputDirectory}.",
          file.FileId,
          OutputDirectory
        );

        var isFileSaved = await _processor.SaveFile(file);

        if (isFileSaved is false)
        {
          _logger.Warning(
            "File {FileId} could not be saved.",
            file.FileId
          );
          continue;
        }

        csv.WriteRecord(file);
        csv.NextRecord();

        _logger.Debug(
          "File {FileId} saved.",
          file.FileId
        );
      }

      _logger.Information("Files downloaded.");

      _logger.Information("Onspring Bulk Attachment Downloader finished.");

      return 0;
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
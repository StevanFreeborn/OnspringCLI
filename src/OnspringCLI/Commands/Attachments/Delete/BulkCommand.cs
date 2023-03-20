namespace OnspringCLI.Commands.Attachments.Delete;

public class BulkCommand : Command
{
  public BulkCommand() : base("bulk", "Delete attachments in bulk")
  {
    AddOption(
      new Option<int>(
        aliases: new[] { "--app-id", "-a" },
        description: "The app id where the attachments are held that will be deleted."
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--field-filter", "-ff" },
        description: "A comma separated list of field ids to whose attachments will be deleted.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--records-filter", "-rf" },
        description: "A comma separated list of record ids whose attachments will be deleted.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    AddOption(
      new Option<int>(
        aliases: new[] { "--report-filter", "-rpf" },
        description: "The id of the report whose records' attachments will be deleted."
      )
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IAttachmentsProcessor _processor;
    public int AppId { get; set; } = 0;
    public string OutputDirectory { get; set; } = "output";
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
      _logger.Information("Starting Onspring Bulk Attachment Deleter");

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

      _logger.Information("Retrieving files that need to be deleted.");

      var fileRequests = await _processor.GetFileRequests(
        AppId,
        fileFields,
        recordsFilter: RecordsFilter
      );

      if (fileRequests.Count is 0)
      {
        _logger.Warning("No files found to delete.");
        return 2;
      }

      _logger.Information(
        "Files retrieved. {Count} files found.",
        fileRequests.Count
      );

      _logger.Information("Deleting files.");

      var erroredRequests = new List<OnspringFileRequest>();

      foreach (var fileRequest in fileRequests)
      {
        var isDeleted = await _processor.TryDeleteFile(
          fileRequest
        );

        if (isDeleted is false)
        {
          erroredRequests.Add(fileRequest);
        }
      }

      if (erroredRequests.Any() is true)
      {
        _processor.WriteFileRequestErrorReport(
          erroredRequests,
          OutputDirectory
        );

        var outputDirectory = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          OutputDirectory
        );

        _logger.Warning(
          "Some files were not deleted. You can find a list of the files that were not deleted in the output directory: {OutputDirectory}",
          outputDirectory
        );
      }

      _logger.Information("Files deleted.");

      _logger.Information("Onspring Bulk Attachment Deleter finished.");

      await Log.CloseAndFlushAsync();

      return 0;
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
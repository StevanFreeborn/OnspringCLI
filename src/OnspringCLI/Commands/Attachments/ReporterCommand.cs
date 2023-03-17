namespace OnspringCLI.Commands.Attachments;

public class ReporterCommand : Command
{
  public ReporterCommand() : base(name: "reporter", "Report on attachments") { }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IAttachmentsProcessor _processor;
    public string? ApiKey { get; set; }
    public int AppId { get; set; }
    public string OutputDirectory { get; set; }
    public LogEventLevel LogLevel { get; set; }
    public List<int> FilesFilter { get; set; }

    public Handler(
      ILogger logger,
      IAttachmentsProcessor processor
    )
    {
      _logger = logger.ForContext<Handler>();
      _processor = processor;
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
      _logger.Information("Starting Onspring Attachment Reporter.");

      _logger.Information("Retrieving file fields.");

      var fileFields = await _processor.GetFileFields();

      if (fileFields.Count == 0)
      {
        _logger.Warning("No file fields could be found.");
        return 2;
      }

      _logger.Information(
        "File fields retrieved. {Count} file fields found.",
        fileFields.Count
      );

      _logger.Information("Retrieving files that need to be requested.");

      var fileRequests = await _processor.GetFileRequests(fileFields);

      if (fileRequests.Count == 0)
      {
        _logger.Warning("No files could be found.");
        return 3;
      }

      _logger.Information(
        "Files retrieved. {Count} files found.",
        fileRequests.Count
      );

      _logger.Information("Retrieving information for each file.");

      var fileInfos = await _processor.GetFileInfos(fileRequests);

      if (fileInfos.Count == 0)
      {
        _logger.Warning("No files information could be found.");
        return 4;
      }

      _logger.Information(
        "File info retrieved for {Count} of {Total} files.",
        fileInfos.Count,
        fileRequests.Count
      );

      _logger.Information("Start writing attachments report.");

      _processor.PrintReport(fileInfos);

      _logger.Information("Finished writing attachments report:");

      _logger.Information("Onspring Attachment Reporter finished.");

      _logger.Information(
        "You can find the log and report files in the output directory: {OutputDirectory}",
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _context.OutputDirectory)
      );

      await Log.CloseAndFlushAsync();

      return 0;
    }
  }
}

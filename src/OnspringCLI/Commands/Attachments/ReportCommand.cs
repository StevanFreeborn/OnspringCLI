namespace OnspringCLI.Commands.Attachments;

public class ReportCommand : Command
{
  public ReportCommand() : base(name: "report", "Report on the attachments in an Onspring app.")
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
        description: "The name of the directory to write the report to.",
        getDefaultValue: () => "output"
      )
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--files-filter", "-ff" },
        description: "A comma separated list of file ids to include in the report.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    var filesFilterCsv = new Option<FileInfo>(
      aliases: new string[] { "--files-filter-csv", "-ffcsv" },
      description: "The path to a csv file that specifies a list of file ids to include in the report."
    );

    filesFilterCsv.AddValidator(
      result =>
      {
        var value = result.GetValueOrDefault<FileInfo>();

        if (
          value is not null &&
          value.Exists is false
        )
        {
          result.ErrorMessage = $"The file {value.FullName} does not exist.";
        }
      }
    );

    AddOption(filesFilterCsv);
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IAttachmentsProcessor _processor;
    public int AppId { get; set; } = 0;
    public string OutputDirectory { get; set; } = "output";
    public List<int> FilesFilter { get; set; } = new();
    public FileInfo? FilesFilterCsv { get; set; } = null;
    public List<int> FilesFilterList => GetFilesFilterList();

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
      _logger.Information("Starting Onspring Attachment Reporter.");

      _logger.Information("Retrieving file fields.");

      var fileFields = await _processor.GetFileFields(AppId);

      if (fileFields.Count is 0)
      {
        _logger.Warning("No file fields could be found.");
        return 1;
      }

      _logger.Information(
        "File fields retrieved. {Count} file fields found.",
        fileFields.Count
      );

      _logger.Information("Retrieving files that need to be requested.");

      var fileRequests = await _processor.GetFileRequests(
        AppId,
        fileFields,
        filesFilter: FilesFilterList
      );

      if (fileRequests.Count is 0)
      {
        _logger.Warning("No files could be found.");
        return 2;
      }

      _logger.Information(
        "Files retrieved. {Count} files found.",
        fileRequests.Count
      );

      _logger.Information("Retrieving information for each file.");

      var fileInfos = await _processor.GetFileInfos(fileRequests);

      if (fileInfos.Count is 0)
      {
        _logger.Warning("No files information could be found.");
        return 3;
      }

      _logger.Information(
        "File info retrieved for {Count} of {Total} files.",
        fileInfos.Count,
        fileRequests.Count
      );

      _logger.Information("Start writing attachments report.");

      _processor.WriteFileInfoReport(fileInfos, OutputDirectory);

      _logger.Information("Finished writing attachments report:");

      _logger.Information("Onspring Attachment Reporter finished.");

      _logger.Information(
        "You can find the report in the output directory: {OutputDirectory}",
        Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          OutputDirectory
        )
      );

      await Log.CloseAndFlushAsync();

      return 0;
    }

    internal List<int> GetFilesFilterList()
    {
      var filesFilterList = new List<int>();

      filesFilterList.AddRange(FilesFilter);

      if (FilesFilterCsv is null)
      {
        return filesFilterList;
      }

      using var reader = new StreamReader(FilesFilterCsv.FullName);
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
      var files = csv.GetRecords<int>().ToList();
      filesFilterList.AddRange(files);

      return filesFilterList;
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
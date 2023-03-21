namespace OnspringCLI.Commands.Attachments;

public class TransferCommand : Command
{
  public TransferCommand() : base("transfer", "Transfer attachments")
  {
    AddOption(
      new Option<string>(
        aliases: new[] { "--target-api-key", "-tk" },
        description: "The API key to use to authenticate with an Onspring target instance."
      )
      {
        IsRequired = true,
      }
    );

    var settingsFileOption = new Option<FileInfo>(
      aliases: new string[] { "--settings-file", "-s" },
      description: "The path to the file that specifies configuration for the transferer."
    )
    {
      IsRequired = true
    };

    settingsFileOption.AddValidator(
      FileInfoOptionValidator.Validate
    );

    AddOption(
      settingsFileOption
    );

    AddOption(
      new Option<List<int>>(
        aliases: new[] { "--records-filter", "-rf" },
        description: "A comma separated list of record ids whose attachments will be transferred.",
        parseArgument: result => result.ParseToIntegerList()
      )
    );

    AddOption(
      new Option<int>(
        aliases: new[] { "--report-filter", "-rpf" },
        description: "The id of the report whose records' attachments will be transferred."
      )
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IAttachmentTransferSettingsFactory _settingsFactory;
    private readonly IAttachmentsProcessor _processor;
    public IAttachmentTransferSettings AttachmentTransferSettings { get; set; } = null!;
    public FileInfo SettingsFile { get; set; } = null!;
    public List<int> RecordsFilter { get; set; } = new();
    public int ReportFilter { get; set; } = 0;

    public Handler(
      ILogger logger,
      IAttachmentTransferSettingsFactory settingsFactory,
      IAttachmentsProcessor processor
    )
    {
      _logger = logger.ForContext<Handler>();
      _settingsFactory = settingsFactory;
      _processor = processor;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
      AttachmentTransferSettings = _settingsFactory.Create(SettingsFile);

      var hasValidMatchFields = await _processor.ValidateMatchFields(
        AttachmentTransferSettings
      );

      if (hasValidMatchFields is false)
      {
        _logger.Fatal(
          "Invalid match fields. Match fields should be of type text, date, number, auto number, or a formula with a non list output type."
        );
        return 1;
      }

      bool hasValidFlagFieldIdAndValues = await _processor.ValidateFlagFieldIdAndValues(
        AttachmentTransferSettings
      );

      if (hasValidFlagFieldIdAndValues is false)
      {
        _logger.Fatal(
          "Invalid flag field id or values. Flag field id should be of type text, date, number, auto number, or a formula with a non list output type."
        );
        return 2;
      }

      _logger.Information("Onspring Attachment Transferer Started");

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

      var sourceFieldIds = new List<int>
      {
        AttachmentTransferSettings.SourceMatchFieldId,
        AttachmentTransferSettings.ProcessFlagFieldId,
      };
      sourceFieldIds.AddRange(
        AttachmentTransferSettings
        .AttachmentFieldIdMappings
        .Keys
        .ToList()
      );

      var sourceRecords = await _processor.GetSourceRecords(
        AttachmentTransferSettings.SourceAppId,
        sourceFieldIds,
        RecordsFilter
      );

      if (sourceRecords.Count is 0)
      {
        _logger.Warning("No records found to transfer.");
        return 3;
      }

      _logger.Information(
        "Retrieved {Count} records from source app.",
        sourceRecords.Count
      );

      _logger.Information("Begin transferring attachments.");

      await Parallel.ForEachAsync(
        sourceRecords,
        async (record, token) =>
          await _processor.TransferAttachments(
            AttachmentTransferSettings,
            record
          )
      );

      _logger.Information("Attachments transferred.");

      _logger.Information("Onspring Attachment Transferer finished");

      await Log.CloseAndFlushAsync();

      return 0;
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
namespace OnspringCLI.Commands.Attachments;

public class TransferCommand : Command
{
  public TransferCommand() : base("transfer", "Transfer attachments")
  {
    var configFileOption = new Option<FileInfo>(
      aliases: new string[] { "--config", "-c" },
      description: "The path to the file that specifies configuration for the transferer."
    )
    {
      IsRequired = true
    };

    configFileOption.AddValidator(
      FileInfoOptionValidator.Validate
    );

    AddOption(
      configFileOption
    );

    AddOption(
      new Option<int>(
        aliases: new string[] { "--pageSize", "-ps" },
        description: "Set the size of each page of records processed.",
        getDefaultValue: () => 50
      )
    );

    AddOption(
      new Option<int?>(
        aliases: new string[] { "--pageNumber", "-pn" },
        description: "Set a limit to the number of pages of records processed.",
        getDefaultValue: () => null
      )
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IAttachmentsProcessor _attachmentsProcessor;
    private readonly IAttachmentTransferSettingsFactory _settingsFactory;
    private readonly IAttachmentTransferSettings _attachmentTransferSettings;
    public FileInfo ConfigFile { get; set; } = null!;
    public int PageSize { get; set; } = 50;
    public int? PageNumber { get; set; } = null;

    public Handler(
      ILogger logger,
      IAttachmentsProcessor attachmentsProcessor,
      IAttachmentTransferSettingsFactory settingsFactory
    )
    {
      _logger = logger.ForContext<Handler>();
      _attachmentsProcessor = attachmentsProcessor;
      _settingsFactory = settingsFactory;
      _attachmentTransferSettings = _settingsFactory.Create(ConfigFile);
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
      throw new NotImplementedException();
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
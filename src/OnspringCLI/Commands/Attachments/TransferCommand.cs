namespace OnspringCLI.Commands.Attachments;

public class TransferCommand : Command
{
  public TransferCommand() : base("transfer", "Transfer attachments")
  {
    AddOption(
      new Option<string>(
        aliases: new[] { "--target-api-key", "-sk" },
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
      new Option<int>(
        aliases: new string[] { "--page-size", "-ps" },
        description: "Set the size of each page of records processed.",
        getDefaultValue: () => 50
      )
    );

    AddOption(
      new Option<int?>(
        aliases: new string[] { "--page-number", "-pn" },
        description: "Set a limit to the number of pages of records processed.",
        getDefaultValue: () => null
      )
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IAttachmentTransferSettingsFactory _settingsFactory;
    public IAttachmentTransferSettings AttachmentTransferSettings { get; set; } = null!;
    public FileInfo SettingsFile { get; set; } = null!;
    public int PageSize { get; set; } = 50;
    public int? PageNumber { get; set; } = null;

    public Handler(
      ILogger logger,
      IAttachmentTransferSettingsFactory settingsFactory
    )
    {
      _logger = logger.ForContext<Handler>();
      _settingsFactory = settingsFactory;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
      AttachmentTransferSettings = _settingsFactory.Create(SettingsFile);
      Console.WriteLine("Transfering attachments...");
      return Task.FromResult(0);
    }

    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
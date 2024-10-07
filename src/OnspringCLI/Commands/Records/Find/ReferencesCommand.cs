
namespace OnspringCLI.Commands.Records.Find;

public class ReferencesCommand : Command
{
  public ReferencesCommand() : base("references", "Locate references to records")
  {
    AddOption(
      new Option<int>(
        aliases: ["--app-id", "-a"],
        description: "The app id to search for references in."
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<List<int>>(
        aliases: ["--record-ids", "-r"],
        description: "A comma separated list of record ids to find references for.",
        parseArgument: result => result.ParseToIntegerList()
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<string>(
        aliases: ["--output-directory", "-o"],
        description: "The name of the directory to write the report to.",
        getDefaultValue: () => "output"
      )
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IRecordsProcessor _processor;
    public int AppId { get; set; } = 0;
    public List<int> RecordIds { get; set; } = [];
    public string OutputDirectory { get; set; } = "output";

    public Handler(ILogger logger, IRecordsProcessor processor)
    {
      _logger = logger.ForContext<Handler>();
      _processor = processor;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
      _logger.Information("Starting references search");

      _logger.Information("Retrieving apps to search for references in.");

      var apps = await _processor.GetApps();

      if (apps.Count == 0)
      {
        _logger.Warning("No apps found.");
        return 1;
      }

      _logger.Information("Apps retrieved. {Count} apps found.", apps.Count);

      var references = new ConcurrentBag<RecordReference>();

      await Parallel.ForEachAsync(apps, async (app, _) =>
      {
        _logger.Information("Retrieving reference fields from app {SourceAppId} to app {TargetAppId}.", app.Id, AppId);

        var referenceFields = await _processor.GetReferenceFields(app.Id, AppId);

        if (referenceFields.Count == 0)
        {
          _logger.Debug("No reference fields found from app {SourceAppId} to app {TargetAppId}.", app.Id, AppId);
          return;
        }

        _logger.Information("Reference fields retrieved for app {SourceAppId}. {Count} reference fields found.", app.Id, referenceFields.Count);

        _logger.Information("Searching for references in app {SourceAppId}", app.Id);

        var appReferences = await _processor.GetReferences(app, referenceFields, RecordIds);

        if (appReferences.Count == 0)
        {
          _logger.Warning("No references found in app {SourceAppId}.", app.Id);
          return;
        }

        _logger.Information("References found in app {SourceAppId}. {Count} references found.", app.Id, appReferences.Count);
        appReferences.ForEach(references.Add);
      });

      _logger.Information("Finished searching for references.");

      if (references.IsEmpty)
      {
        _logger.Warning("No references found.");
        return 2;
      }

      _logger.Information("Found {Count} references.", references.Count);

      _logger.Information("Start writing references report.");

      _processor.WriteReferencesReport([.. references], OutputDirectory);

      _logger.Information("Finished writing references report.");

      _logger.Information("Finished references search.");

      _logger.Information(
        "You can find the report in the output directory: {OutputDirectory}",
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OutputDirectory)
      );

      await Log.CloseAndFlushAsync();

      return 0;
    }

    [ExcludeFromCodeCoverage]
    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
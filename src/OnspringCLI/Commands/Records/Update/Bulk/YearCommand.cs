namespace OnspringCLI.Commands.Records.Update.Bulk;

public class YearCommand : Command
{
  public YearCommand() : base("year", "Adjusts the value of a list and/or date field by given years")
  {
    var settingsFileOption = new Option<FileInfo>(
      aliases: ["--file", "-f"],
      description: "The path to the .csv file that contains the list of apps and fields to update."
    )
    {
      IsRequired = true
    };

    settingsFileOption.AddValidator(FileInfoOptionValidator.Validate);

    AddOption(settingsFileOption);

    AddOption(
      new Option<int>(
        aliases: ["--years", "-y"],
        description: "The number of years to adjust the field by."
      )
      {
        IsRequired = true
      }
    );
  }

  public new class Handler : ICommandHandler
  {
    private readonly ILogger _logger;
    private readonly IRecordsProcessor _processor;
    private readonly IUpdateYearSettingsFactory _settingsFactory;
    public FileInfo? File { get; set; }
    public int Years { get; set; }

    public Handler(ILogger logger, IRecordsProcessor processor, IUpdateYearSettingsFactory settingsFactory)
    {
      _logger = logger.ForContext<Handler>();
      _processor = processor;
      _settingsFactory = settingsFactory;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
      _logger.Information("Starting bulk year update");

      _logger.Information("Loading settings from {File}.", File?.FullName);
      var settings = await _settingsFactory.CreateAsync(File);

      _logger.Information("Validating apps.");
      var allApps = await _processor.GetApps();
      var appsValidationResult = settings.ValidateApps(allApps);

      if (appsValidationResult.IsValid is false)
      {
        _logger.Warning(
          "The following apps in the file could not be found: {Apps}.",
          string.Join(", ", appsValidationResult.AppsNotFound)
        );

        return 1;
      }

      _logger.Information("Validating fields.");
      var mappings = new Dictionary<App, List<Field>>();
      var fieldsNotFound = new Dictionary<string, List<string>>();
      var invalidFieldsFound = new Dictionary<string, List<string>>();

      foreach (var app in appsValidationResult.AppsFound)
      {
        var fields = await _processor.GetFieldsForApp(app.Id);
        var fieldsValidationResult = settings.ValidateFields(app, fields);

        if (fieldsValidationResult.FieldsNotFound.Count > 0)
        {
          fieldsNotFound.Add(app.Name, fieldsValidationResult.FieldsNotFound);
        }

        if (fieldsValidationResult.InvalidFields.Count > 0)
        {
          invalidFieldsFound.Add(app.Name, fieldsValidationResult.InvalidFields);
        }

        if (fieldsNotFound.ContainsKey(app.Name) || invalidFieldsFound.ContainsKey(app.Name))
        {
          continue;
        }

        mappings.Add(app, fieldsValidationResult.FieldsFound);
      }

      if (fieldsNotFound.Count > 0 || invalidFieldsFound.Count > 0)
      {
        foreach (var (app, fields) in fieldsNotFound)
        {
          _logger.Warning("The following fields in app {App} could not be found: {Fields}.", app, string.Join(", ", fields));
        }

        foreach (var (app, fields) in invalidFieldsFound)
        {
          _logger.Warning("The following fields in app {App} are not list or date fields: {Fields}.", app, string.Join(", ", fields));
        }

        return 2;
      }

      _logger.Information("Updating records...");

      var counts = new Dictionary<string, int>();

      foreach (var mapping in mappings)
      {
        await foreach (var record in _processor.GetRecords(mapping.Key, mapping.Value))
        {
          var updatedRecord = await _processor.UpdateRecordYearValues(
            mapping.Key.Name,
            record,
            mapping.Value,
            Years
          );

          if (updatedRecord is null)
          {
            continue;
          }

          if (counts.TryGetValue(mapping.Key.Name, out var value))
          {
            counts[mapping.Key.Name] = ++value;
          }
          else
          {
            counts.Add(mapping.Key.Name, 1);
          }
        }
      }

      foreach (var (app, count) in counts)
      {
        _logger.Information("Updated {Count} record(s) in app {App}.", count, app);
      }

      _logger.Information("Finished bulk year update");

      return 0;
    }

    [ExcludeFromCodeCoverage]
    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
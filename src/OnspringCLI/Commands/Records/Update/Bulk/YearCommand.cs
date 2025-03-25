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
      // TODO: Implement this method
      // 1. Parse the csv file to get a list of apps and their fields
      // 2. Validate the apps and fields exist and can be accessed by this app
      // 3. For each app page through the records and update the field value of fields
      //    that match a field in the list
      //    a. If the field is a list field, get the correct GUID for the target year. Add the value if necessary.
      //    b. If the field is a date field, adjust the date by the number of years
      //    c. Update the record with the new field values
      //    d. Log the record id and the field that was updated
      //    e. If the record could not be updated, log the record id and the error message, and continue to the next record
      // 4. Log the number of records updated and the number of records that could not be updated
      // 5. Return 0 if all records were updated successfully, 1 if any records could not be updated
      // 6. Write out errors to a file if any records could not be updated

      _logger.Information("Starting bulk year update");

      _logger.Information("Loading settings from {File}.", File.FullName);
      var settings = await _settingsFactory.CreateAsync(File);

      _logger.Information("Validating apps.");
      var allApps = await _processor.GetApps();
      var appsFound = allApps.Where(a => settings.AppFieldsMap.ContainsKey(a.Name)).ToList();

      if (appsFound.Count != settings.AppFieldsMap.Count)
      {
        var appsNotFound = settings.AppFieldsMap.Keys.Except(appsFound.Select(a => a.Name));
        _logger.Warning("The following apps in the file could not be found: {Apps}.", appsNotFound);
        return 1;
      }

      _logger.Information("Validating fields.");
      var mapping = new Dictionary<App, List<Field>>();
      var fieldsNotFound = new Dictionary<string, List<string>>();
      var invalidFieldsFound = new Dictionary<string, List<string>>();

      foreach (var app in appsFound)
      {
        var fields = await _processor.GetFieldsForApp(app.Id);
        var fieldsLookingFor = settings.AppFieldsMap[app.Name];
        var foundFields = fields.Where(f => fieldsLookingFor
          .Contains(f.Name, StringComparer.OrdinalIgnoreCase))
          .ToList();

        var (notFoundFields, invalidFields) = ValidateFields(fields, fieldsLookingFor);

        if (notFoundFields.Count > 0)
        {
          fieldsNotFound.Add(app.Name, notFoundFields);
        }

        if (invalidFields.Count > 0)
        {
          invalidFieldsFound.Add(app.Name, invalidFields);
        }

        if (fieldsNotFound.ContainsKey(app.Name) || invalidFieldsFound.ContainsKey(app.Name))
        {
          continue;
        }

        mapping.Add(app, foundFields);
      }

      if (fieldsNotFound.Count > 0)
      {
        foreach (var (app, fields) in fieldsNotFound)
        {
          _logger.Warning("The following fields in app {App} could not be found: {Fields}.", app, string.Join(", ", fields));
        }
      }

      if (invalidFieldsFound.Count > 0)
      {
        foreach (var (app, fields) in invalidFieldsFound)
        {
          _logger.Warning("The following fields in app {App} are not valid for updating: {Fields}.", app, string.Join(", ", fields));
        }
      }

      if (fieldsNotFound.Count > 0 || invalidFieldsFound.Count > 0)
      {
        return 2;
      }

      _logger.Information("Updating records.");

      return 0;
    }

    [ExcludeFromCodeCoverage]
    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }

    private static ValidationResult ValidateFields(List<Field> fields, List<string> fieldsLookingFor)
    {
      var foundFields = fields.Where(f => fieldsLookingFor
        .Contains(f.Name, StringComparer.OrdinalIgnoreCase))
        .ToList();

      var notFoundFields = fieldsLookingFor.Except(foundFields.Select(f => f.Name)).ToList();

      var invalidFields = foundFields
        .Where(f => f.Type is not FieldType.List and not FieldType.Date)
        .Select(f => f.Name)
        .ToList();

      return new(notFoundFields, invalidFields);
    }

    private record ValidationResult(List<string> NotFoundFields, List<string> InvalidFields);
  }
}
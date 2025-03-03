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
    public FileInfo File { get; set; }
    public int Years { get; set; }

    public Handler(ILogger logger, IRecordsProcessor processor)
    {
      _logger = logger.ForContext<Handler>();
      _processor = processor;
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
      var fieldsToUpdate = await GetFieldsToUpdateAsync();

      _logger.Information("Validating apps.");
      var allApps = await _processor.GetApps();
      var appsFound = allApps.Where(a => fieldsToUpdate.ContainsKey(a.Name)).ToList();

      if (appsFound.Count != fieldsToUpdate.Count)
      {
        var appsNotFound = fieldsToUpdate.Keys.Except(appsFound.Select(a => a.Name));
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
        var fieldsLookingFor = fieldsToUpdate[app.Name];
        var foundFields = fields.Where(f => fieldsLookingFor
              .Contains(f.Name, StringComparer.OrdinalIgnoreCase))
              .ToList();

        // TODO: Extract to method
        if (foundFields.Count != fieldsLookingFor.Count)
        {
          var fieldsNotFoundForApp = fieldsLookingFor.Except(foundFields.Select(f => f.Name));
          fieldsNotFound.Add(app.Name, [.. fieldsNotFoundForApp]);
        }

        // TODO: Extract to method
        if (foundFields.Any(f => f.Type is not FieldType.List and not FieldType.Date))
        {
          var invalidFields = foundFields
            .Where(f => f.Type is not FieldType.List and not FieldType.Date)
            .Select(f => f.Name);

          invalidFieldsFound.Add(app.Name, [.. invalidFields]);
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
          _logger.Warning("The following fields in app {App} could not be found: {Fields}.", app, fields);
        }

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

    private async Task<Dictionary<string, List<string>>> GetFieldsToUpdateAsync()
    {
      var fieldsToUpdate = new Dictionary<string, List<string>>();

      using var reader = new StreamReader(File.FullName);
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

      await foreach (var record in csv.GetRecordsAsync<FieldToUpdate>())
      {
        if (fieldsToUpdate.TryGetValue(record.App, out var value))
        {
          value.Add(record.Field);
          continue;
        }

        fieldsToUpdate.Add(record.App, [record.Field]);
      }

      return fieldsToUpdate;
    }

    private record FieldToUpdate(
      string App,
      string Field
    );
  }
}
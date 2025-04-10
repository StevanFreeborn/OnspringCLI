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

        foreach (var (app, fields) in fieldsNotFound)
        {
          _logger.Warning("The following fields in app {App} could not be found: {Fields}.", app, string.Join(", ", fields));
        }

        return 2;
      }

      _logger.Information("Updating records.");

      foreach (var mapping in mappings)
      {
        await foreach (var record in _processor.GetRecords(mapping.Key, mapping.Value))
        {
          var updatedRecord = new ResultRecord()
          {
            AppId = record.AppId,
            RecordId = record.RecordId
          };

          foreach (var field in mapping.Value)
          {
            var fieldValue = record.FieldData.FirstOrDefault(fv => fv.FieldId == field.Id);

            if (fieldValue is null)
            {
              _logger.Debug("No value found for {Field} on {Record} in {App}", field.Name, record.RecordId, mapping.Key);
              continue;
            }

            if (field is ListField listField && listField.Multiplicity is Multiplicity.SingleSelect)
            {
              var singleSelectListFieldValue = fieldValue.AsNullableGuid();

              if (singleSelectListFieldValue is null)
              {
                _logger.Debug("Unable to get value for {Field} on {Record} in {App}", field.Name, record.RecordId, mapping.Key);
                continue;
              }

              var value = listField.Values.FirstOrDefault(v => v.Id == singleSelectListFieldValue);

              if (value is null)
              {
                _logger.Debug(
                  "Unable to find list value for {Value} for {Field} on {Record} in {App}",
                  singleSelectListFieldValue,
                  field.Name,
                  record.RecordId,
                  mapping.Key
                );

                continue;
              }

              var isYear = int.TryParse(value.Name, out var valueAsYear);

              if (isYear is false)
              {
                _logger.Debug(
                  "Unable to parse a year value from {Value} for {Field} on {Record} in {App}",
                  value.Name,
                  field.Name,
                  record.RecordId,
                  mapping.Key
                );

                continue;
              }

              var newYearValue = valueAsYear + Years;



              continue;
            }

            if (field.Type is FieldType.Date)
            {
              var dateFieldValue = fieldValue.AsNullableDateTime();

              if (dateFieldValue is null || dateFieldValue.HasValue is false)
              {
                _logger.Debug("Unable to get value for {Field} on {Record} in {App}", field.Name, record.RecordId, mapping.Key);
                continue;
              }

              var newDateFieldValue = dateFieldValue.Value.AddYears(Years);
              _logger.Debug(
                "Updating value for {Field} on {Record} in {App} from {OldValue} to {NewValue}",
                field.Name,
                record.RecordId,
                mapping.Key,
                dateFieldValue.Value,
                newDateFieldValue
              );
              updatedRecord.FieldData.Add(new DateFieldValue(fieldValue.FieldId, newDateFieldValue));

              continue;
            }
          }
        }
      }

      return 0;
    }

    [ExcludeFromCodeCoverage]
    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }
  }
}
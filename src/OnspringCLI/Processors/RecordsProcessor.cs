namespace OnspringCLI.Processors;

internal class RecordsProcessor(
  ILogger logger,
  IReportService reportService,
  IOnspringService onspringService,
  IOptions<GlobalOptions> globalOptions
) : IRecordsProcessor
{
  private readonly ILogger _logger = logger.ForContext<RecordsProcessor>();
  private readonly IReportService _reportService = reportService;
  private readonly IOnspringService _onspringService = onspringService;
  private readonly GlobalOptions _globalOptions = globalOptions.Value;

  public Task<List<App>> GetApps()
  {
    return _onspringService.GetApps(_globalOptions.SourceApiKey);
  }

  public async Task<List<ReferenceField>> GetReferenceFields(int sourceAppId, int targetAppId)
  {
    var fields = await _onspringService.GetAllFields(_globalOptions.SourceApiKey, sourceAppId);

    return [.. fields
      .Where(f => f.Type is FieldType.Reference)
      .Cast<ReferenceField>()
      .Where(f => f.ReferencedAppId == targetAppId)];
  }

  public async Task<List<RecordReference>> GetReferences(App sourceApp, List<ReferenceField> referenceFields, List<int> recordIds)
  {
    var referenceFieldIds = referenceFields.Select(static f => f.Id).ToList();
    var pagingRequest = new PagingRequest { PageNumber = 1 };
    var totalPages = 1;

    _logger.Information("Retrieving references from app {SourceAppId}.", sourceApp.Id);

    var references = new ConcurrentBag<RecordReference>();

    do
    {
      var res = await _onspringService.GetAPageOfRecords(
        _globalOptions.SourceApiKey,
        sourceApp.Id,
        referenceFieldIds,
        pagingRequest
      );

      if (res is null)
      {
        _logger.Information("No records found in app {SourceAppId} for page {PageNumber}.", sourceApp.Id, pagingRequest.PageNumber);
        break;
      }

      totalPages = res.TotalPages;

      _logger.Information(
        "Records retrieved from app {SourceAppId} for page {PageNumber} of {TotalPages}.",
        sourceApp.Id,
        pagingRequest.PageNumber,
        totalPages
      );

      var referencesFromPage = GetReferencesFromRecords(
        sourceApp,
        res.Items,
        referenceFields,
        recordIds
      );

      referencesFromPage.ForEach(references.Add);
      pagingRequest.PageNumber++;
    } while (pagingRequest.PageNumber <= totalPages);

    return [.. references];
  }

  private static List<RecordReference> GetReferencesFromRecords(
    App sourceApp,
    List<ResultRecord> records,
    List<ReferenceField> referenceFields,
    List<int> recordIds
  )
  {
    var references = new List<RecordReference>();

    foreach (var record in records)
    {
      foreach (var field in referenceFields)
      {
        var fieldValue = record.FieldData.FirstOrDefault(f => f.FieldId == field.Id);

        if (fieldValue is null)
        {
          continue;
        }

        if (fieldValue is IntegerFieldValue intValue && intValue.Value.HasValue)
        {
          var isReference = recordIds.Contains(intValue.Value.Value);

          if (isReference is false)
          {
            continue;
          }

          references.Add(new()
          {
            TargetAppId = field.ReferencedAppId,
            TargetRecordId = intValue.Value.Value,
            SourceAppId = sourceApp.Id,
            SourceAppName = sourceApp.Name,
            SourceFieldId = field.Id,
            SourceFieldName = field.Name,
            SourceRecordId = record.RecordId
          });
        }

        if (fieldValue is IntegerListFieldValue intListValue && intListValue.Value.Count is not 0)
        {
          foreach (var value in intListValue.Value)
          {
            var isReference = recordIds.Contains(value);

            if (isReference is false)
            {
              continue;
            }

            references.Add(new()
            {
              TargetAppId = field.ReferencedAppId,
              TargetRecordId = value,
              SourceAppId = sourceApp.Id,
              SourceAppName = sourceApp.Name,
              SourceFieldId = field.Id,
              SourceFieldName = field.Name,
              SourceRecordId = record.RecordId
            });
          }
        }
      }
    }
    return references;
  }

  public void WriteReferencesReport(List<RecordReference> references, string outputDirectory)
  {
    _reportService.WriteCsvReport(
      references,
      typeof(RecordReferenceMap),
      outputDirectory,
      "references-report.csv"
    );
  }

  public async Task<List<Field>> GetFieldsForApp(int appId)
  {
    return await _onspringService.GetAllFields(_globalOptions.SourceApiKey, appId);
  }

  public async IAsyncEnumerable<ResultRecord> GetRecords(App app, List<Field> fields)
  {
    var pagingRequest = new PagingRequest { PageNumber = 1 };
    var totalPages = 1;

    do
    {
      var page = await _onspringService.GetAPageOfRecords(
        _globalOptions.SourceApiKey,
        app.Id,
        [.. fields.Select(static f => f.Id)],
        pagingRequest
      );

      if (page is null || page.Items.Count is 0)
      {
        _logger.Warning("No records found in app {AppId} for page {PageNumber}.", app.Id, pagingRequest.PageNumber);
        yield break;
      }

      totalPages = page.TotalPages;

      _logger.Information(
        "Records retrieved from app {AppId} for page {PageNumber} of {TotalPages}.",
        app.Id,
        pagingRequest.PageNumber,
        totalPages
      );

      foreach (var record in page.Items)
      {
        yield return record;
      }

      pagingRequest.PageNumber++;
    } while (pagingRequest.PageNumber <= totalPages);
  }

  public async Task<ResultRecord?> UpdateRecordYearValues(string appName, ResultRecord record, List<Field> fields, int years)
  {
    var updatedRecord = new ResultRecord()
    {
      AppId = record.AppId,
      RecordId = record.RecordId
    };

    foreach (var field in fields)
    {
      var fieldValue = record.FieldData.FirstOrDefault(fv => fv.FieldId == field.Id);

      if (fieldValue is null)
      {
        _logger.Debug("No value found for {Field} on {Record} in {App}", field.Name, record.RecordId, appName);
        continue;
      }

      if (field is ListField listField && listField.Multiplicity is Multiplicity.SingleSelect)
      {
        var singleSelectListFieldValue = fieldValue.AsNullableGuid();

        if (singleSelectListFieldValue is null)
        {
          _logger.Debug("Unable to get value for {Field} on {Record} in {App}", field.Name, record.RecordId, appName);
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
            appName
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
            appName
          );

          continue;
        }

        var newYearValue = valueAsYear + years;
        var listValue = new ListValue()
        {
          Name = newYearValue.ToString(CultureInfo.InvariantCulture),
          NumericValue = newYearValue,
        };

        var newYearValueId = await _onspringService.GetOrAddListValueByName(_globalOptions.SourceApiKey, listField.Id, listValue);

        if (newYearValueId is null)
        {
          _logger.Debug(
            "Unable to get new year value {NewYearValue} for {Field} on {Record} in {App}",
            newYearValue,
            field.Name,
            record.RecordId,
            appName
          );

          continue;
        }

        _logger.Debug(
          "Updating value for {Field} on {Record} in {App} from {OldValue} to {NewValue}",
          field.Name,
          record.RecordId,
          appName,
          valueAsYear,
          newYearValue
        );
        updatedRecord.FieldData.Add(new GuidFieldValue(listField.Id, newYearValueId));
        continue;
      }

      if (field.Type is FieldType.Date)
      {
        var dateFieldValue = fieldValue.AsNullableDateTime();

        if (dateFieldValue is null || dateFieldValue.HasValue is false)
        {
          _logger.Debug("Unable to get value for {Field} on {Record} in {App}", field.Name, record.RecordId, appName);
          continue;
        }

        var newDateFieldValue = dateFieldValue.Value.AddYears(years);
        _logger.Debug(
          "Updating value for {Field} on {Record} in {App} from {OldValue} to {NewValue}",
          field.Name,
          record.RecordId,
          appName,
          dateFieldValue.Value,
          newDateFieldValue
        );
        updatedRecord.FieldData.Add(new DateFieldValue(fieldValue.FieldId, newDateFieldValue));

        continue;
      }
    }

    if (updatedRecord.FieldData.Count is 0)
    {
      _logger.Debug("No fields to update for {Record} in {App}", record.RecordId, appName);
      return null;
    }

    var updateRecordResponse = await _onspringService.UpdateRecord(
      _globalOptions.SourceApiKey,
      updatedRecord
    );

    if (updateRecordResponse is null)
    {
      _logger.Warning("Unable to update record {Record} in {App}", record.RecordId, appName);
      return null;
    }

    _logger.Information(
      "Record {Record} in {App} updated successfully",
      record.RecordId,
      appName
    );

    return updatedRecord;
  }
}
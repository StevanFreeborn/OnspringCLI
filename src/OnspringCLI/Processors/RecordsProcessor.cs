namespace OnspringCLI.Processors;

class RecordsProcessor : IRecordsProcessor
{
  private readonly ILogger _logger;
  private readonly IReportService _reportService;
  private readonly IOnspringService _onspringService;
  private readonly GlobalOptions _globalOptions;

  public RecordsProcessor(
    ILogger logger, 
    IReportService reportService, 
    IOnspringService onspringService,
    IOptions<GlobalOptions> globalOptions
  )
  {
    _logger = logger.ForContext<RecordsProcessor>();
    _reportService = reportService;
    _onspringService = onspringService;
    _globalOptions = globalOptions.Value;
  }

  public Task<List<App>> GetApps() => _onspringService.GetApps(_globalOptions.SourceApiKey);

  public async Task<List<ReferenceField>> GetReferenceFields(int sourceAppId, int targetAppId)
  {
    var fields = await _onspringService.GetAllFields(_globalOptions.SourceApiKey, sourceAppId);
    
    return fields
      .Where(f => f.Type is FieldType.Reference)
      .Cast<ReferenceField>()
      .Where(f => f.ReferencedAppId == targetAppId)
      .ToList();
  }

  public async Task<List<RecordReference>> GetReferences(App sourceApp, List<ReferenceField> referenceFields, List<int> recordIds)
  {
    var referenceFieldIds = referenceFields.Select(f => f.Id).ToList();
    var pagingRequest = new PagingRequest { PageNumber = 1 };

    _logger.Debug("Retrieving references from app {SourceAppId}.", sourceApp.Id);

    var references = new ConcurrentBag<RecordReference>();

    var initialRes = await _onspringService.GetAPageOfRecords(
      _globalOptions.SourceApiKey,
      sourceApp.Id,
      referenceFieldIds,
      pagingRequest
    );

    if (initialRes is null)
    {
      _logger.Debug("No records found in app {SourceAppId} for page {PageNumber}.", sourceApp.Id, pagingRequest.PageNumber);
      return [.. references];
    }

    _logger.Debug(
      "Records retrieved from app {SourceAppId} for page {PageNumber}. {Count} records found.", 
      sourceApp.Id, 
      initialRes.PageNumber, 
      initialRes.Items.Count
    );

    var referencesFromFirstPage = GetReferencesFromRecords(
      sourceApp,
      initialRes.Items,
      referenceFields,
      recordIds
    );

    referencesFromFirstPage.ForEach(references.Add);

    var remainingPages = Enumerable.Range(initialRes.PageNumber + 1, initialRes.TotalPages - 1);
    var remainingPageRequests = remainingPages.Select(p => new PagingRequest { PageNumber = p });
    var remainingRequests = remainingPageRequests.Select(async (pr) => 
    {
      var res = await _onspringService.GetAPageOfRecords(
        _globalOptions.SourceApiKey,
        sourceApp.Id,
        referenceFieldIds,
        pr
      );

      if (res is null)
      {
        _logger.Debug("No records found in app {SourceAppId} for page {PageNumber}.", sourceApp.Id, pr.PageNumber);
        return;
      }

      _logger.Debug(
        "Records retrieved from app {SourceAppId} for page {PageNumber}. {Count} records found.", 
        sourceApp.Id, 
        pr.PageNumber, 
        res.Items.Count
      );

      var referencesFromPage = GetReferencesFromRecords(
        sourceApp,
        res.Items,
        referenceFields,
        recordIds
      );

      referencesFromPage.ForEach(references.Add);
    });

    await Task.WhenAll(remainingRequests);

    return [.. references];
  }

  private List<RecordReference> GetReferencesFromRecords(
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
}
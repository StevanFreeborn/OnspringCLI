namespace OnspringCLI.Processors;

class AttachmentsProcessor : IAttachmentsProcessor
{
  private readonly IOptions<GlobalOptions> _globalOptions;
  private readonly IOnspringService _onspringService;
  private readonly IReportService _reportService;
  private readonly ILogger _logger;
  private readonly LoggingLevelSwitch _logLevelSwitch;
  private readonly IProgressBarFactory _progressBarFactory;

  public AttachmentsProcessor(
    IOptions<GlobalOptions> globalOptions,
    IOnspringService onspringService,
    IReportService reportService,
    ILogger logger,
    LoggingLevelSwitch loggingLevelSwitch,
    IProgressBarFactory progressBarFactory
  )
  {
    _globalOptions = globalOptions;
    _onspringService = onspringService;
    _reportService = reportService;
    _logger = logger.ForContext<AttachmentsProcessor>();
    _logLevelSwitch = loggingLevelSwitch;
    _progressBarFactory = progressBarFactory;
  }

  public async Task<List<Field>> GetFileFields(
    int appId,
    List<int>? fieldsFilter = null
  )
  {
    var fields = await _onspringService.GetAllFields(
      _globalOptions.Value.SourceApiKey,
      appId
    );

    if (
      fieldsFilter is not null &&
      fieldsFilter.Any() is true
    )
    {
      fields = fields
      .Where(
        f => fieldsFilter.Contains(f.Id)
      )
      .ToList();
    }

    return fields
    .Where(f => f.Type == FieldType.Attachment || f.Type == FieldType.Image)
    .ToList();
  }

  public async Task<List<OnspringFileRequest>> GetFileRequests(
    int appId,
    List<Field> fileFields,
    List<int>? filesFilter = null,
    List<int>? recordsFilter = null
  )
  {
    var fileFieldIds = fileFields.Select(f => f.Id).ToList();
    var pagingRequest = new PagingRequest(1, 50);
    var totalPages = 1;
    var currentPage = pagingRequest.PageNumber;
    var fileRequests = new List<OnspringFileRequest>();

    _logger.Debug(
      "Retrieving records whose information needs to be requested."
    );

    do
    {
      _logger.Debug(
        "Retrieving records for page {PageNumber}.",
        currentPage
      );

      var res = await _onspringService.GetAPageOfRecords(
        _globalOptions.Value.SourceApiKey,
        appId,
        fileFieldIds,
        pagingRequest
      );

      if (res == null)
      {
        _logger.Warning(
          "No records found for page {PageNumber}.",
          currentPage
        );

        break;
      }

      _logger.Debug(
        "Records retrieved for page {PageNumber}. {Count} records found.",
        currentPage,
        res.Items.Count
      );

      totalPages = res.TotalPages;

      foreach (var record in res.Items)
      {
        if (
          recordsFilter is not null &&
          recordsFilter.Any() is true &&
          recordsFilter.Contains(record.RecordId) is false
        )
        {
          continue;
        }

        var requests = GetFileRequestsFromRecord(
          record,
          fileFields,
          filesFilter
        );

        fileRequests.AddRange(requests);
      }

      _logger.Debug(
        "Retrieved files from page {CurrentPage} of records.",
        currentPage
      );

      pagingRequest.PageNumber++;
      currentPage = pagingRequest.PageNumber;
    } while (currentPage <= totalPages);

    _logger.Debug(
      "Finished retrieving records whose information needs to be requested."
    );

    return fileRequests;
  }

  public async Task<List<OnspringFileInfoResult>> GetFileInfos(
    List<OnspringFileRequest> fileRequests
  )
  {
    var fileInfos = new ConcurrentBag<OnspringFileInfoResult>();

    _logger.Debug(
      "Retrieving information for {Count} files.",
      fileRequests.Count
    );

    using var pBar = _progressBarFactory.Create(
      fileRequests.Count,
      "Starting to retrieve information for files."
    );
    _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

    await Parallel.ForEachAsync(
      fileRequests,
      async (fileRequest, token) =>
      {
        pBar.Message = $"Retrieving file info for file {fileRequest.FileId}.";

        var fileInfo = await GetFileInfo(fileRequest);
        fileInfos.Add(fileInfo);

        pBar.Tick($"Retrieved file info for file {fileRequest.FileId}.");

        _logger.Debug(
          "File info retrieved for record {RecordId}, field {FieldId}, file {FileId}.",
          fileRequest.RecordId,
          fileRequest.FieldId,
          fileRequest.FileId
        );
      }
    );

    pBar.Message = "Finished retrieving information for files.";
    _logLevelSwitch.MinimumLevel = LogEventLevel.Information;

    _logger.Debug(
      "Information retrieved for {Count} files.",
      fileInfos.Count
    );

    return fileInfos.ToList();
  }

  public void WriteFileInfoReport(
    List<OnspringFileInfoResult> fileInfos,
    string outputDirectory
  )
  {
    _reportService.WriteCsvReport(
      fileInfos,
      typeof(OnspringFileInfoResultMap),
      outputDirectory,
      "attachments-report.csv"
    );
  }

  public void WriteFileRequestErrorReport(
    List<OnspringFileRequest> fileRequests,
    string outputDirectory
  )
  {
    _reportService.WriteCsvReport(
      fileRequests,
      typeof(OnspringFileRequestMap),
      outputDirectory,
      "file-request-errors.csv"
    );
  }

  public async Task<List<int>> GetRecordIdsFromReport(int reportId)
  {
    var report = await _onspringService.GetReport(
      _globalOptions.Value.SourceApiKey,
      reportId
    );

    if (report == null)
    {
      _logger.Warning(
        "Unable to get report {ReportId}.",
        reportId
      );

      return new List<int>();
    }

    return report
    .Rows
    .Select(r => r.RecordId)
    .ToList();
  }

  public async Task<List<OnspringFileRequest>> TryDownloadFiles(
    List<OnspringFileRequest> fileRequests,
    string outputDirectory
  )
  {
    var outputPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        outputDirectory
      );

    Directory.CreateDirectory(outputPath);

    var fileName = Path.Combine(
      outputPath,
      "file-list.csv"
    );

    using var writer = new StreamWriter(fileName);
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    csv.Context.RegisterClassMap<OnspringFileResultMap>();

    csv.WriteHeader<OnspringFileResult>();
    csv.NextRecord();

    using var pBar = _progressBarFactory.Create(
      fileRequests.Count,
      "Downloading files"
    );
    _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

    var erroredRequests = new List<OnspringFileRequest>();

    foreach (var fileRequest in fileRequests)
    {
      pBar.Message = $"Downloading file {fileRequest.FileId} in field {fileRequest.FieldId} for record {fileRequest.RecordId}";

      var file = await GetFile(
        fileRequest,
        outputDirectory
      );

      if (file is null)
      {
        erroredRequests.Add(fileRequest);
        continue;
      }

      var isSaved = await TrySaveFile(file);

      if (isSaved is false)
      {
        erroredRequests.Add(fileRequest);
        continue;
      }

      csv.WriteRecord(file);
      csv.NextRecord();

      pBar.Tick($"Downloaded file {fileRequest.FileId} in field {fileRequest.FieldId} for record {fileRequest.RecordId}");
    }

    pBar.Message = "Downloaded files";
    _logLevelSwitch.MinimumLevel = LogEventLevel.Information;

    return erroredRequests;
  }

  public async Task<List<OnspringFileRequest>> TryDeleteFiles(
    List<OnspringFileRequest> fileRequests
  )
  {
    using var pBar = _progressBarFactory.Create(
      fileRequests.Count,
      "Deleting files"
    );
    _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

    var erroredRequests = new List<OnspringFileRequest>();

    foreach (var fileRequest in fileRequests)
    {
      pBar.Message = $"Deleting file {fileRequest.FileId} in field {fileRequest.FieldId} for record {fileRequest.RecordId}";

      var isDeleted = await TryDeleteFile(
        fileRequest
      );

      if (isDeleted is false)
      {
        erroredRequests.Add(fileRequest);
      }

      pBar.Tick($"Deleted file {fileRequest.FileId} in field {fileRequest.FieldId} for record {fileRequest.RecordId}");
    }

    pBar.Message = "Deleted files";
    _logLevelSwitch.MinimumLevel = LogEventLevel.Information;

    return erroredRequests;
  }

  public async Task<bool> ValidateMatchFields(
    IAttachmentTransferSettings settings
  )
  {
    var sourceMatchField = await _onspringService.GetField(
      _globalOptions.Value.SourceApiKey,
      settings.SourceMatchFieldId
    );

    var targetMatchField = await _onspringService.GetField(
      _globalOptions.Value.TargetApiKey,
      settings.TargetMatchFieldId
    );

    if (
      sourceMatchField is null ||
      IsValidMatchFieldType(sourceMatchField) is false ||
      targetMatchField is null ||
      IsValidMatchFieldType(targetMatchField) is false
    )
    {
      return false;
    }

    return true;
  }

  public async Task<bool> ValidateFlagFieldIdAndValues(
    IAttachmentTransferSettings settings
  )
  {
    var flagField = await _onspringService.GetField(
      _globalOptions.Value.SourceApiKey,
      settings.ProcessFlagFieldId
    );

    if (
      flagField is null ||
      flagField is not ListField listField ||
      listField.Multiplicity is not Multiplicity.SingleSelect
    )
    {
      return false;
    }

    var processValue = settings.ProcessFlagValue;

    var processListValue = listField
    .Values
    .FirstOrDefault(
      v =>
        v.Name.ToLower() == processValue.ToLower() ||
        (Guid.TryParse(processValue, out var result) && v.Id == result)
    );

    var processedValue = settings.ProcessedFlagValue;

    var processedListValue = listField
    .Values
    .FirstOrDefault(
      v =>
        v.Name.ToLower() == processedValue.ToLower() ||
        (Guid.TryParse(processedValue, out var result) && v.Id == result)
    );

    if (
      processListValue is null ||
      processedListValue is null
    )
    {
      return false;
    }

    settings
    .ProcessFlagListValueId = processListValue.Id;

    settings
    .ProcessedFlagListValueId = processedListValue.Id;

    return true;
  }

  public async Task<List<ResultRecord>> GetSourceRecordsToProcess(
    IAttachmentTransferSettings settings,
    List<int>? recordsFilter = null
  )
  {
    var sourceFieldIds = new List<int>
    {
      settings.SourceMatchFieldId,
      settings.ProcessFlagFieldId,
    };

    sourceFieldIds.AddRange(
      settings
      .AttachmentFieldIdMappings
      .Keys
      .ToList()
    );

    var pagingRequest = new PagingRequest(1, 50);
    int totalPages;
    var currentPage = pagingRequest.PageNumber;
    var sourceRecords = new List<ResultRecord>();

    _logger.Debug(
      "Retrieving records whose attachments need to be transferred."
    );

    do
    {
      _logger.Debug(
        "Retrieving records for page {PageNumber}.",
        currentPage
      );

      var queryFilter = $"{settings.ProcessFlagFieldId} contains '{settings.ProcessFlagValue}'";

      var res = await _onspringService.GetAPageOfRecordsByQuery(
        _globalOptions.Value.SourceApiKey,
        settings.SourceAppId,
        sourceFieldIds,
        queryFilter,
        pagingRequest
      );

      if (res == null)
      {
        _logger.Warning(
          "No records found for page {PageNumber}.",
          currentPage
        );

        break;
      }

      _logger.Debug(
        "Records retrieved for page {PageNumber}. {Count} records found.",
        currentPage,
        res.Items.Count
      );

      totalPages = res.TotalPages;

      foreach (var record in res.Items)
      {
        if (
          recordsFilter is not null &&
          recordsFilter.Any() is true &&
          recordsFilter.Contains(record.RecordId) is false
        )
        {
          continue;
        }

        sourceRecords.Add(record);
      }

      pagingRequest.PageNumber++;
      currentPage = pagingRequest.PageNumber;
    } while (currentPage <= totalPages);

    _logger.Debug(
      "Finished retrieving records whose information needs to be transferred."
    );

    return sourceRecords;
  }

  public async Task TransferAttachments(
    IAttachmentTransferSettings settings,
    List<ResultRecord> sourceRecords
  )
  {
    _logger.Debug(
      "Transferring attachments for {Count} records.",
      sourceRecords.Count
    );

    using var pBar = _progressBarFactory.Create(
      sourceRecords.Count,
      "Transferring attachments"
    );
    _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

    await Parallel.ForEachAsync(
      sourceRecords,
      async (sourceRecord, token) =>
      {
        pBar.Message = $"Transferring attachments for record {sourceRecord.RecordId}";

        await TransferRecordAttachments(
          settings,
          sourceRecord
        );

        pBar.Tick($"Transferred attachments for record {sourceRecord.RecordId}");

        _logger.Debug(
          "Transferred attachments for record {RecordId}.",
          sourceRecord.RecordId
        );
      }
    );

    pBar.Message = "Transferred attachments";
    _logLevelSwitch.MinimumLevel = LogEventLevel.Information;

    _logger.Debug(
      "Finished transferring attachments for {Count} records.",
      sourceRecords.Count
    );
  }

  public async Task TransferRecordAttachments(
    IAttachmentTransferSettings settings,
    ResultRecord sourceRecord
  )
  {
    _logger.Debug(
      "Begin processing Source Record {SourceRecordId} in Source App {SourceAppId}.",
      sourceRecord.RecordId,
      settings.SourceAppId
    );

    var matchValue = GetRecordFieldValue(
      sourceRecord,
      settings.SourceMatchFieldId
    );

    if (matchValue is null)
    {
      _logger.Warning(
        "No match value found for Source Record {SourceRecordId} in Source App {SourceAppId}.",
        sourceRecord.RecordId,
        sourceRecord.AppId
      );

      return;
    }

    var queryFields = new List<int> { settings.TargetMatchFieldId };
    var queryFilter = $"{settings.TargetMatchFieldId} eq '{matchValue.GetValue()}'";

    var targetRecords = await _onspringService.GetAPageOfRecordsByQuery(
      _globalOptions.Value.TargetApiKey,
      settings.TargetAppId,
      queryFields,
      queryFilter
    );

    if (
      targetRecords is null ||
      targetRecords.Items.Count == 0
    )
    {
      _logger.Warning(
        "No Target Record found for Source Record {SourceRecordId} in Source App {SourceAppId}.",
        sourceRecord.RecordId,
        sourceRecord.AppId
      );

      return;
    }

    if (
      targetRecords.Items.Count > 1
    )
    {
      _logger.Warning(
        "Multiple Target Records found for Source Record {SourceRecordId} in Source App {SourceAppId}.",
        sourceRecord.RecordId,
        sourceRecord.AppId
      );

      return;
    }

    var targetRecordId = targetRecords.Items[0].RecordId;

    var sourceAttachmentFieldIds = settings
    .AttachmentFieldIdMappings
    .Keys
    .ToList();

    foreach (var sourceAttachmentFieldId in sourceAttachmentFieldIds)
    {
      var targetAttachmentFieldId = settings
      .AttachmentFieldIdMappings
      .GetValueOrDefault(
        sourceAttachmentFieldId
      );

      await TransferAttachmentsForFieldId(
        sourceRecord,
        targetRecordId,
        sourceAttachmentFieldId,
        targetAttachmentFieldId
      );
    }

    var sourceRecordUpdates = new ResultRecord
    {
      AppId = sourceRecord.AppId,
      RecordId = sourceRecord.RecordId,
      FieldData = new List<RecordFieldValue>
      {
        new StringFieldValue
        {
          FieldId = settings.ProcessFlagFieldId,
          Value = settings.ProcessedFlagListValueId.ToString()
        }
      }
    };

    var updateResponse = await _onspringService.UpdateRecord(
      _globalOptions.Value.SourceApiKey,
      sourceRecordUpdates
    );

    if (updateResponse is null)
    {
      _logger.Warning(
        "Failed to update Source Record {SourceRecordId} in Source App {SourceAppId} as processed.",
        sourceRecord.RecordId,
        sourceRecord.AppId
      );
    }

    _logger.Debug(
      "Finished processing Source Record {SourceRecordId} in Source App {SourceAppId}.",
      sourceRecord.RecordId,
      sourceRecord.AppId
    );
  }

  [ExcludeFromCodeCoverage]
  private async Task<bool> TryDeleteFile(
    OnspringFileRequest fileRequest
  )
  {
    try
    {
      _logger.Debug(
        "Deleting file {FileId} for record {RecordId} in field {FieldId}.",
        fileRequest.FileId,
        fileRequest.RecordId,
        fileRequest.FieldId
      );

      var isDeleted = await _onspringService.TryDeleteFile(
        _globalOptions.Value.SourceApiKey,
        fileRequest
      );

      if (isDeleted is false)
      {
        _logger.Warning(
          "Unable to delete file {FileId} for record {RecordId} in field {FieldId}.",
          fileRequest.FileId,
          fileRequest.RecordId,
          fileRequest.FieldId
        );

        return false;
      }

      _logger.Debug(
        "File {FileId} deleted for record {RecordId} in field {FieldId}.",
        fileRequest.FileId,
        fileRequest.RecordId,
        fileRequest.FieldId
      );

      return true;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Error deleting file {FileId} for record {RecordId} in field {FieldId}.",
        fileRequest.FileId,
        fileRequest.RecordId,
        fileRequest.FieldId
      );

      return false;
    }
  }

  internal async Task<OnspringFileResult?> GetFile(
    OnspringFileRequest fileRequest,
    string outputDirectory
  )
  {
    _logger.Debug(
      "Retrieving file {FileId} for record {RecordId} in field {FieldId}.",
      fileRequest.FileId,
      fileRequest.RecordId,
      fileRequest.FieldId
    );

    var res = await _onspringService.GetFile(
      _globalOptions.Value.SourceApiKey,
      fileRequest
    );

    if (res == null)
    {
      _logger.Warning(
        "Unable to get file {FileId} for record {RecordId} in field {FieldId}.",
        fileRequest.FileId,
        fileRequest.RecordId,
        fileRequest.FieldId
      );

      return null;
    }

    _logger.Debug(
      "File {FileId} retrieved for record {RecordId} in field {FieldId}.",
      fileRequest.FileId,
      fileRequest.RecordId,
      fileRequest.FieldId
    );

    var fileName = res.FileName.Trim('"');

    var filePath = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      outputDirectory,
      "files",
      $"{fileRequest.RecordId}-{fileRequest.FieldId}-{fileRequest.FileId}-{fileName}"
    );

    return new OnspringFileResult(
      fileRequest.RecordId,
      fileRequest.FieldId,
      fileRequest.FieldName,
      fileRequest.FileId,
      fileName,
      filePath,
      res.Stream
    );
  }

  internal async Task<bool> TrySaveFile(OnspringFileResult file)
  {
    try
    {
      _logger.Debug(
        "Saving file {FileName} for record {RecordId}, field {FieldId}, file {FileId}.",
        file.FileName,
        file.RecordId,
        file.FieldId,
        file.FileId
      );

      if (file.FilePath is null)
      {
        _logger.Warning(
          "Unable to save file. File path is null for record {RecordId}, field {FieldId}, file {FileId}.",
          file.RecordId,
          file.FieldId,
          file.FileId
        );

        return false;
      }

      var fileDirectory = Path.GetDirectoryName(file.FilePath);

      if (fileDirectory is null)
      {
        _logger.Warning(
          "Unable to save file. File directory is null for record {RecordId}, field {FieldId}, file {FileId}.",
          file.RecordId,
          file.FieldId,
          file.FileId
        );

        return false;
      }

      Directory.CreateDirectory(fileDirectory);
      await using var fileStream = File.Create(file.FilePath);
      await file.Stream.CopyToAsync(fileStream);

      _logger.Debug(
        "File {FileId} saved for record {RecordId} in field {FieldId}.",
        file.FileId,
        file.RecordId,
        file.FieldId
      );

      return true;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Error saving file {FileId} for record {RecordId} in field {FieldId}.",
        file.FileId,
        file.RecordId,
        file.FieldId
      );

      return false;
    }
  }

  internal async Task TransferAttachmentsForFieldId(
    ResultRecord sourceRecord,
    int targetRecordId,
    int sourceAttachmentFieldId,
    int targetAttachmentFieldId
  )
  {
    var attachmentFieldValue = GetRecordFieldValue(
      sourceRecord,
      sourceAttachmentFieldId
    );

    if (attachmentFieldValue is null)
    {
      _logger.Warning(
        "No field data found in Source Attachment Field {AttachmentFieldId} for Source Record {SourceRecordId} in Source App {SourceAppId}.",
        sourceAttachmentFieldId,
        sourceRecord.RecordId,
        sourceRecord.AppId
      );

      return;
    }

    var fileIds = GetFileIds(attachmentFieldValue);

    foreach (var fileId in fileIds)
    {
      await TransferAttachment(
        sourceRecord,
        targetRecordId,
        sourceAttachmentFieldId,
        targetAttachmentFieldId,
        fileId
      );
    }
  }

  internal async Task TransferAttachment(
    ResultRecord sourceRecord,
    int targetRecordId,
    int sourceAttachmentFieldId,
    int targetAttachmentFieldId,
    int sourceFileId
  )
  {
    var sourceFileRequest = new OnspringFileRequest(
      sourceRecord.RecordId,
      sourceAttachmentFieldId,
      sourceFileId
    );

    var sourceFileInfo = await _onspringService.GetFileInfo(
      _globalOptions.Value.SourceApiKey,
      sourceFileRequest
    );

    if (sourceFileInfo is null)
    {
      _logger.Warning(
        "No file information could be found for File: {@FileRequest}.",
        sourceFileRequest
      );

      return;
    }

    var sourceFile = await _onspringService.GetFile(
      _globalOptions.Value.SourceApiKey,
      sourceFileRequest
    );

    if (sourceFile is null)
    {
      _logger.Warning(
        "No file could be found for File: {@FileRequest}.",
        sourceFileRequest
      );

      return;
    }

    var saveFileRequest = new SaveFileRequest
    {
      RecordId = targetRecordId,
      FieldId = targetAttachmentFieldId,
      FileName = sourceFileInfo.Name,
      ContentType = sourceFileInfo.ContentType,
      FileStream = sourceFile.Stream,
      Notes = sourceFileInfo.Notes ?? string.Empty,
    };

    var res = await _onspringService.SaveFile(
      _globalOptions.Value.TargetApiKey,
      saveFileRequest
    );

    if (res is null)
    {
      _logger.Warning(
        "Source File {@SourceFileRequest} could not be saved in Target: {@SaveFileRequest}",
        sourceFileRequest,
        saveFileRequest
      );

      return;
    }

    _logger.Debug(
      "Source File {@SourceFileRequest} successfully saved as File {TargetFileId}: {@SaveFileRequest}",
      sourceFileRequest,
      res.Id,
      saveFileRequest
    );
  }

  internal static List<int> GetFileIds(RecordFieldValue attachmentFieldData)
  {
    return attachmentFieldData.Type switch
    {
      ResultValueType.FileList => attachmentFieldData.AsFileList(),
      _ => attachmentFieldData
      .AsAttachmentList()
      .Where(attachment => attachment.StorageLocation is FileStorageSite.Internal)
      .Select(attachment => attachment.FileId)
      .ToList(),
    };
  }

  internal static RecordFieldValue? GetRecordFieldValue(
    ResultRecord record,
    int fieldId
  )
  {
    return record
    .FieldData
    .FirstOrDefault(
      x => x.FieldId == fieldId
    );
  }

  internal async Task<OnspringFileInfoResult> GetFileInfo(
    OnspringFileRequest fileRequest
  )
  {
    _logger.Debug(
      "Retrieving file info for record {RecordId}, field {FieldId}, file {FileId}.",
      fileRequest.RecordId,
      fileRequest.FieldId,
      fileRequest.FileId
    );

    var res = await _onspringService.GetFile(
      _globalOptions.Value.SourceApiKey,
      fileRequest
    );

    if (res == null)
    {
      _logger.Warning(
        "Unable to get file info for record {RecordId}, field {FieldId}, file {FileId}.",
        fileRequest.RecordId,
        fileRequest.FieldId,
        fileRequest.FileId
      );

      return new OnspringFileInfoResult(
        fileRequest.RecordId,
        fileRequest.FieldId,
        fileRequest.FieldName,
        fileRequest.FileId,
        "Error: Unable to get file info",
        0
      );
    }

    _logger.Debug(
      "File info retrieved for record {RecordId}, field {FieldId}, file {FileId}.",
      fileRequest.RecordId,
      fileRequest.FieldId,
      fileRequest.FileId
    );

    return new OnspringFileInfoResult(
      fileRequest.RecordId,
      fileRequest.FieldId,
      fileRequest.FieldName,
      fileRequest.FileId,
      res.FileName,
      Convert.ToDecimal(res.ContentLength)
    );
  }

  internal static List<OnspringFileRequest> GetFileRequestsFromRecord(
    ResultRecord record,
    List<Field> fileFields,
    List<int>? filesFilter
  )
  {
    var hasFileFilter = filesFilter is not null && filesFilter.Any() is true;
    var fileRequests = new List<OnspringFileRequest>();

    foreach (var fieldValue in record.FieldData)
    {
      var field = fileFields.FirstOrDefault(f => f.Id == fieldValue.FieldId);

      if (field == null)
      {
        continue;
      }

      if (field.Type == FieldType.Attachment)
      {
        var attachments = fieldValue.AsAttachmentList();

        if (IsAllAttachmentsField(record, fileFields, attachments))
        {
          continue;
        }

        foreach (var attachment in attachments)
        {
          if (attachment.StorageLocation != FileStorageSite.Internal)
          {
            continue;
          }

          if (
            filesFilter is not null &&
            filesFilter.Any() is true &&
            filesFilter.Contains(attachment.FileId) is false
          )
          {
            continue;
          }

          fileRequests.Add(
            new OnspringFileRequest(
              record.RecordId,
              fieldValue.FieldId,
              field.Name,
              attachment.FileId
            )
          );
        }
      }

      if (field.Type == FieldType.Image)
      {
        var files = fieldValue.AsFileList();

        foreach (var file in files)
        {
          if (
            filesFilter is not null &&
            filesFilter.Any() is true &&
            filesFilter.Contains(file) is false
          )
          {
            continue;
          }

          fileRequests.Add(
            new OnspringFileRequest(
              record.RecordId,
              fieldValue.FieldId,
              field.Name,
              file
            )
          );
        }
      }
    }

    return fileRequests;
  }

  internal static bool IsAllAttachmentsField(
    ResultRecord record,
    List<Field> fileFields,
    List<AttachmentFile> attachmentFieldValue
  )
  {
    var attachmentFieldIds = fileFields
    .Where(f => f.Type == FieldType.Attachment)
    .Select(f => f.Id)
    .ToList();

    // If there is only one attachment field, then there can't be an "All Attachments" field.
    if (attachmentFieldIds.Count <= 1)
    {
      return false;
    }

    var attachmentIds = record.FieldData
    .Where(
      f => attachmentFieldIds.Contains(f.FieldId)
    )
    .SelectMany(
      f => f.AsAttachmentList()
    )
    .Select(
      f => f.FileId
    )
    .Distinct()
    .ToList();

    return attachmentFieldValue
    .Select(
      f => f.FileId
    )
    .Distinct()
    .SequenceEqual(
      attachmentIds
    );
  }

  [ExcludeFromCodeCoverage]
  private static bool IsValidMatchFieldType(Field field)
  {
    var isSupportedField = field.Type is
    FieldType.Text or
    FieldType.AutoNumber or
    FieldType.Date or
    FieldType.Number or
    FieldType.Formula;

    if (isSupportedField is false)
    {
      return false;
    }

    if (
      field.Type is FieldType.Formula &&
      field is FormulaField formulaField &&
      formulaField is not null
    )
    {
      return formulaField.OutputType is not FormulaOutputType.ListValue;
    }

    return true;
  }
}
namespace OnspringCLI.Processors;

class AttachmentsProcessor : IAttachmentsProcessor
{
  private readonly IOnspringService _onspringService;
  private readonly IReportService _reportService;
  private readonly ILogger _logger;

  public AttachmentsProcessor(
    IOnspringService onspringService,
    IReportService reportService,
    ILogger logger
  )
  {
    _onspringService = onspringService;
    _reportService = reportService;
    _logger = logger;
  }

  public async Task<List<Field>> GetFileFields(
    int appId,
    List<int>? fieldsFilter = null
  )
  {
    var fields = await _onspringService.GetAllFields(appId);

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

      _logger.Debug(
        "Retrieving records for page {PageNumber}.",
        currentPage
      );

      var res = await _onspringService.GetAPageOfRecords(
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

    await Parallel.ForEachAsync(
      fileRequests,
      async (fileRequest, token) =>
      {
        var fileInfo = await GetFileInfo(fileRequest);
        fileInfos.Add(fileInfo);

        _logger.Debug(
          "File info retrieved for record {RecordId}, field {FieldId}, file {FileId}.",
          fileRequest.RecordId,
          fileRequest.FieldId,
          fileRequest.FileId
        );
      }
    );

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
    var report = await _onspringService.GetReport(reportId);

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

  public async Task<OnspringFileResult?> GetFile(
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

    var res = await _onspringService.GetFile(fileRequest);

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

  public async Task<bool> TrySaveFile(OnspringFileResult file)
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

  public async Task<bool> TryDeleteFile(
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

      var isDeleted = await _onspringService.TryDeleteFile(fileRequest);

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

    var res = await _onspringService.GetFile(fileRequest);

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
}
using System.Collections.Concurrent;

namespace OnspringAttachmentReporter.Models;

class AttachmentsProcessor : IAttachmentsProcessor
{
  private readonly IOnspringService _onspringService;
  private readonly IReportService _reportService;
  private readonly ILogger _logger;

  public Processor(
    IOnspringService onspringService,
    IReportService reportService,
    ILogger logger
  )
  {
    _onspringService = onspringService;
    _reportService = reportService;
    _logger = logger;
  }

  public async Task<List<Field>> GetFileFields()
  {
    var fields = await _onspringService.GetAllFields();

    return fields
    .Where(f => f.Type == FieldType.Attachment || f.Type == FieldType.Image)
    .ToList();
  }

  public async Task<List<FileInfoRequest>> GetFileRequests(List<Field> fileFields)
  {
    var fileFieldIds = fileFields.Select(f => f.Id).ToList();
    var pagingRequest = new PagingRequest(1, 50);
    var totalPages = 1;
    var currentPage = pagingRequest.PageNumber;
    var fileRequests = new List<FileInfoRequest>();

    var options = new ProgressBarOptions
    {
      ForegroundColor = ConsoleColor.DarkBlue,
      ProgressCharacter = '─',
      ShowEstimatedDuration = false,
    };

    using var progressBar = new ProgressBar(
      totalPages,
      "Retrieving files whose information needs to be requested...",
      options
    );

    do
    {
      _logger.Debug(
        "Retrieving records for page {PageNumber}.",
        currentPage
      );

      progressBar.Message = $"Retrieving files from page {currentPage} of records.";

      var res = await _onspringService.GetAPageOfRecords(fileFieldIds, pagingRequest);

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

      progressBar.Tick($"Retrieved files from page {currentPage} of records.");

      foreach (var record in res.Items)
      {
        var requests = GetFileRequestsFromRecord(record, fileFields);
        fileRequests.AddRange(requests);
      }

      pagingRequest.PageNumber++;
      currentPage = pagingRequest.PageNumber;
    } while (currentPage <= totalPages);

    progressBar.Tick("Finished retrieving files whose information needs to be requested.");
    return fileRequests;
  }

  public async Task<List<FileInfoResult>> GetFileInfos(List<FileInfoRequest> fileRequests)
  {
    var fileInfos = new ConcurrentBag<FileInfoResult>();

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

  public void PrintReport(List<FileInfo> fileInfos)
  {
    _reportService.WriteReport(fileInfos);
  }

  [ExcludeFromCodeCoverage]
  private async Task<FileInfoResult> GetFileInfo(FileInfoRequest fileRequest)
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

      return new FileInfoResult(
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

    return new FileInfoResult(
      fileRequest.RecordId,
      fileRequest.FieldId,
      fileRequest.FieldName,
      fileRequest.FileId,
      res.FileName,
      Convert.ToDecimal(res.ContentLength)
    );
  }

  [ExcludeFromCodeCoverage]
  private static List<FileInfoRequest> GetFileRequestsFromRecord(
    ResultRecord record,
    List<Field> fileFields
  )
  {
    var fileRequests = new List<FileInfoRequest>();

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
            _context.FilesFilter.Any() is true &&
            _context.FilesFilter.Contains(attachment.FileId) is false
          )
          {
            continue;
          }

          fileRequests.Add(
            new FileInfoRequest(
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
            _context.FilesFilter.Any() is true &&
            _context.FilesFilter.Contains(file) is false
          )
          {
            continue;
          }

          fileRequests.Add(
            new FileInfoRequest(
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
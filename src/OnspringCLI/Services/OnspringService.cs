namespace OnspringCLI.Services;

class OnspringService : IOnspringService
{
  private readonly ILogger _logger;
  private readonly IOnspringClientFactory _clientFactory;

  public OnspringService(
    ILogger logger,
    IOnspringClientFactory clientFactory
  )
  {
    _logger = logger.ForContext<OnspringService>();
    _clientFactory = clientFactory;
  }

  public async Task<List<Field>> GetAllFields(
    string apiKey,
    int appId
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var fields = new List<Field>();
      var totalPages = 1;
      var pagingRequest = new PagingRequest(1, 50);
      var currentPage = pagingRequest.PageNumber;

      _logger.Debug("Retrieving file fields.");

      do
      {
        _logger.Debug(
          "Retrieving file fields from page {CurrentPage} of {TotalPages}.",
          currentPage,
          totalPages
        );

        var res = await ExecuteRequest(
          async () => await client.GetFieldsForAppAsync(
            appId,
            pagingRequest
          )
        );

        if (res.IsSuccessful is true)
        {
          fields.AddRange(res.Value.Items);
          totalPages = res.Value.TotalPages;
        }
        else
        {
          _logger.Error(
            "Unable to get fields. {StatusCode} - {Message}. Current page: {CurrentPage}. Total pages: {TotalPages}.",
            res.StatusCode,
            res.Message,
            currentPage,
            totalPages
          );
        }

        _logger.Debug(
          "Retrieved file fields from page {CurrentPage} of {TotalPages}.",
          currentPage,
          totalPages
        );

        pagingRequest.PageNumber++;
        currentPage = pagingRequest.PageNumber;
      } while (currentPage <= totalPages);

      _logger.Debug("Finished retrieving file fields.");

      return fields;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to get fields."
      );

      return new List<Field>();
    }
  }

  public async Task<GetPagedRecordsResponse?> GetAPageOfRecords(
    string apiKey,
    int appId,
    List<int> fieldIds,
    PagingRequest pagingRequest
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var request = new GetRecordsByAppRequest
      {
        AppId = appId,
        PagingRequest = pagingRequest,
        FieldIds = fieldIds
      };

      var res = await ExecuteRequest(
        async () => await client.GetRecordsForAppAsync(request)
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to get records. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to get records."
      );

      return null;
    }
  }

  public async Task<Field?> GetField(
    string apiKey,
    int fieldId
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var res = await ExecuteRequest(
        async () => await client.GetFieldAsync(fieldId)
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to get field. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to get field."
      );

      return null;
    }
  }

  public async Task<GetFileResponse?> GetFile(
    string apiKey,
    OnspringFileRequest fileRequest
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var res = await ExecuteRequest(
        async () => await client.GetFileAsync(
          fileRequest.RecordId,
          fileRequest.FieldId,
          fileRequest.FileId
        )
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to get file. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to get file: {@FileRequest}.",
        fileRequest
      );

      return null;
    }
  }

  public async Task<GetFileInfoResponse?> GetFileInfo(
    string apiKey,
    OnspringFileRequest fileRequest
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var res = await ExecuteRequest(
        async () => await client.GetFileInfoAsync(
          fileRequest.RecordId,
          fileRequest.FieldId,
          fileRequest.FileId
        )
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to get file info. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to get file info: {@FileRequest}.",
        fileRequest
      );

      return null;
    }
  }

  public async Task<GetPagedRecordsResponse?> GetAPageOfRecordsByQuery(
    string apiKey,
    int appId,
    List<int> fieldIds,
    string queryFilter,
    PagingRequest? pagingRequest = null
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var request = new QueryRecordsRequest
      {
        AppId = appId,
        FieldIds = fieldIds,
        Filter = queryFilter,
      };

      var res = await ExecuteRequest(
        async () => await client.QueryRecordsAsync(request, pagingRequest)
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to get records by query. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to get records by query."
      );

      return null;
    }
  }

  public async Task<ReportData?> GetReport(
    string apiKey,
    int reportId
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var res = await ExecuteRequest(
        async () => await client.GetReportAsync(reportId)
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to get report. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to get report."
      );

      return null;
    }
  }

  public async Task<CreatedWithIdResponse<int>?> SaveFile(
    string apiKey,
    SaveFileRequest request
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var res = await ExecuteRequest(
        async () => await client.SaveFileAsync(request)
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to save file. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to save file: {@SaveFileRequest}.",
        request
      );

      return null;
    }
  }

  public async Task<bool> TryDeleteFile(
    string apiKey,
    OnspringFileRequest fileRequest
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var res = await ExecuteRequest(
        async () => await client.DeleteFileAsync(
          fileRequest.RecordId,
          fileRequest.FieldId,
          fileRequest.FileId
        )
      );

      if (res.IsSuccessful is true)
      {
        return true;
      }

      _logger.Error(
        "Unable to delete file. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return false;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to delete file: {@FileRequest}.",
        fileRequest
      );

      return false;
    }
  }

  public async Task<CreatedWithIdResponse<int>?> UpdateRecord(
    string apiKey,
    ResultRecord recordUpdates
  )
  {
    try
    {
      var client = _clientFactory.Create(apiKey);

      var res = await ExecuteRequest(
        async () => await client.SaveRecordAsync(recordUpdates)
      );

      if (res.IsSuccessful is true)
      {
        return res.Value;
      }

      _logger.Error(
        "Unable to save record. {StatusCode} - {Message}.",
        res.StatusCode,
        res.Message
      );

      return null;
    }
    catch (Exception ex)
    {
      _logger.Error(
        ex,
        "Unable to save record: {@RecordUpdates}.",
        recordUpdates
      );

      return null;
    }
  }

  [ExcludeFromCodeCoverage]
  private async Task<ApiResponse<T>> ExecuteRequest<T>(
    Func<Task<ApiResponse<T>>> func,
    int retry = 1
  )
  {
    ApiResponse<T> response;
    var retryLimit = 3;

    try
    {
      do
      {
        response = await func();

        if (response.IsSuccessful is true)
        {
          return response;
        }

        _logger.Warning(
          "Request was unsuccessful. {StatusCode} - {Message}. ({Attempt} of {AttemptLimit})",
          response.StatusCode,
          response.Message,
          retry,
          retryLimit
        );

        retry++;

        if (retry > retryLimit)
        {
          break;
        }

        await Wait(retry);
      } while (retry <= retryLimit);
    }
    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
    {
      _logger.Error(
        ex,
        "Request failed. ({Attempt} of {AttemptLimit})",
        retry,
        retryLimit
      );

      retry++;

      if (retry > retryLimit)
      {
        throw;
      }

      await Wait(retry);

      return await ExecuteRequest(func, retry);
    }

    _logger.Error(
      "Request failed after {RetryLimit} attempts. {StatusCode} - {Message}.",
      retryLimit,
      response.StatusCode,
      response.Message
    );

    return response;
  }

  [ExcludeFromCodeCoverage]
  private async Task<ApiResponse> ExecuteRequest(
    Func<Task<ApiResponse>> func,
    int retry = 1
  )
  {
    ApiResponse response;
    var retryLimit = 3;

    try
    {
      do
      {
        response = await func();

        if (response.IsSuccessful is true)
        {
          return response;
        }

        _logger.Warning(
          "Request was unsuccessful. {StatusCode} - {Message}. ({Attempt} of {AttemptLimit})",
          response.StatusCode,
          response.Message,
          retry,
          retryLimit
        );

        retry++;

        if (retry > retryLimit)
        {
          break;
        }

        await Wait(retry);
      } while (retry <= retryLimit);
    }
    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
    {
      _logger.Error(
        ex,
        "Request failed. ({Attempt} of {AttemptLimit})",
        retry,
        retryLimit
      );

      retry++;

      if (retry > retryLimit)
      {
        throw;
      }

      await Wait(retry);

      return await ExecuteRequest(func, retry);
    }

    _logger.Error(
      "Request failed after {RetryLimit} attempts. {StatusCode} - {Message}.",
      retryLimit,
      response.StatusCode,
      response.Message
    );

    return response;
  }

  [ExcludeFromCodeCoverage]
  private async Task Wait(int retryAttempt)
  {
    var wait = 1000 * retryAttempt;

    _logger.Debug(
      "Waiting {Wait}s before retrying request.",
      wait
    );

    await Task.Delay(wait);

    _logger.Debug(
      "Retrying request. {Attempt} of {AttemptLimit}",
      retryAttempt,
      3
    );
  }
}
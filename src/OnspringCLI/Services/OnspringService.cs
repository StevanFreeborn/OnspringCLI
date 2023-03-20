namespace OnspringCLI.Services;

class OnspringService : IOnspringService
{
  private readonly ILogger _logger;
  private readonly IOptions<GlobalOptions> _options;
  private readonly IOnspringClient _client;

  public OnspringService(
    ILogger logger,
    IOptions<GlobalOptions> options
  )
  {
    _logger = logger;
    _options = options;

    _client = new OnspringClient(
      _options.Value.BaseUrl,
      _options.Value.ApiKey
    );
  }

  public async Task<List<Field>> GetAllFields(int appId)
  {
    try
    {
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
          async () => await _client.GetFieldsForAppAsync(
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
    int appId,
    List<int> fileFields,
    PagingRequest pagingRequest
  )
  {
    try
    {
      var request = new GetRecordsByAppRequest
      {
        AppId = appId,
        PagingRequest = pagingRequest,
        FieldIds = fileFields
      };

      var res = await ExecuteRequest(
        async () => await _client.GetRecordsForAppAsync(request)
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

  public async Task<GetFileResponse?> GetFile(OnspringFileRequest fileRequest)
  {
    try
    {
      var res = await ExecuteRequest(
        async () => await _client.GetFileAsync(
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

  public async Task<ReportData?> GetReport(int reportId)
  {
    try
    {
      var res = await ExecuteRequest(
        async () => await _client.GetReportAsync(reportId)
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

  public async Task<bool> TryDeleteFile(OnspringFileRequest fileRequest)
  {
    try
    {
      var res = await ExecuteRequest(
        async () => await _client.DeleteFileAsync(
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

  [ExcludeFromCodeCoverage]
  private async Task<ApiResponse<T>> ExecuteRequest<T>(Func<Task<ApiResponse<T>>> func, int retry = 1)
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
  private async Task<ApiResponse> ExecuteRequest(Func<Task<ApiResponse>> func, int retry = 1)
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
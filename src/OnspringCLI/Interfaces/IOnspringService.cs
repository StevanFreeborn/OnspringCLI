namespace OnspringCLI.Interfaces
{
  public interface IOnspringService
  {
    Task<bool> TryDeleteFile(string apiKey, OnspringFileRequest fileRequest);

    Task<List<Field>> GetAllFields(string apiKey, int appId);

    Task<List<App>> GetApps(string apiKey);

    Task<GetPagedRecordsResponse?> GetAPageOfRecords(
      string apiKey,
      int appId,
      List<int> fieldIds,
      PagingRequest pagingRequest
    );

    Task<GetFileResponse?> GetFile(
      string apiKey,
      OnspringFileRequest fileRequest
    );

    Task<ReportData?> GetReport(
      string apiKey,
      int reportId
    );

    Task<Field?> GetField(
      string apiKey,
      int fieldId
    );

    Task<GetPagedRecordsResponse?> GetAPageOfRecordsByQuery(
      string apiKey,
      int appId,
      List<int> fieldIds,
      string queryFilter,
      PagingRequest? pagingRequest = null
    );

    Task<GetFileInfoResponse?> GetFileInfo(
      string apiKey,
      OnspringFileRequest fileRequest
    );

    Task<CreatedWithIdResponse<int>?> SaveFile(
      string apiKey,
      SaveFileRequest request
    );

    Task<CreatedWithIdResponse<int>?> UpdateRecord(
      string apiKey,
      ResultRecord recordUpdates
    );
  }
}
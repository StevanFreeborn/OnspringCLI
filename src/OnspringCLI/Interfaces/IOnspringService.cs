namespace OnspringCLI.Interfaces
{
  public interface IOnspringService
  {
    Task<bool> TryDeleteFile(
      string apiKey,
      OnspringFileRequest fileRequest
    );

    Task<List<Field>> GetAllFields(
      string apiKey,
      int appId
    );

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
    Task<List<ResultRecord>> GetRecordsByQuery(
      string apiKey,
      int targetAppId,
      List<int> list,
      string queryFilter
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
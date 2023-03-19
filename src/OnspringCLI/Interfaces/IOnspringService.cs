namespace OnspringCLI.Interfaces
{
  public interface IOnspringService
  {
    Task<List<Field>> GetAllFields(int appId);
    Task<GetPagedRecordsResponse?> GetAPageOfRecords(int appId, List<int> fileFields, PagingRequest pagingRequest);
    Task<GetFileResponse?> GetFile(FileInfoRequest fileRequest);
    Task<ReportData?> GetReport(int reportId);
  }
}
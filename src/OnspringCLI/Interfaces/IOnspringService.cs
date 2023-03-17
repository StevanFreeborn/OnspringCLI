namespace OnspringCLI.Interfaces
{
  public interface IOnspringService
  {
    Task<List<Field>> GetAllFields();
    Task<GetPagedRecordsResponse?> GetAPageOfRecords(List<int> fileFields, PagingRequest pagingRequest);
    Task<GetFileResponse?> GetFile(FileInfoRequest fileRequest);
  }
}
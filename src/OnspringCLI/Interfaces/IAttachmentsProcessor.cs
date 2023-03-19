namespace OnspringCLI.Interfaces;

public interface IAttachmentsProcessor
{
  Task<List<Field>> GetFileFields(int appId, List<int>? fieldsFilter = null);

  Task<List<OnspringFileRequest>> GetFileRequests(
    int appId,
    List<Field> fileFields,
    List<int>? filesFilter = null,
    List<int>? recordsFilter = null
  );

  Task<List<OnspringFileInfoResult>> GetFileInfos(List<OnspringFileRequest> fileRequests);
  Task<OnspringFileResult?> GetFile(OnspringFileRequest fileRequest, string outputDirectory);
  Task<bool> SaveFile(OnspringFileResult file);
  Task<List<int>> GetRecordIdsFromReport(int reportId);
  void WriteFileInfoReport(List<OnspringFileInfoResult> fileInfos, string outputDirectory);
}
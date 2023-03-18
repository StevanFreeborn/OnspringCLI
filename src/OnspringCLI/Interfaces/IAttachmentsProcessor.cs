namespace OnspringCLI.Interfaces;

public interface IAttachmentsProcessor
{
  Task<List<Field>> GetFileFields(int appId, List<int>? fieldsFilter = null);
  Task<List<FileInfoRequest>> GetFileRequests(
    int appId,
    List<Field> fileFields,
    List<int>? filesFilter = null,
    List<int>? recordsFilter = null
  );

  Task<List<OnspringFileInfoResult>> GetFileInfos(List<FileInfoRequest> fileRequests);
  void WriteFileInfoReport(List<OnspringFileInfoResult> fileInfos, string outputDirectory);
}
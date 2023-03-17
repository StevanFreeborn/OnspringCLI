namespace OnspringCLI.Interfaces;

public interface IAttachmentsProcessor
{
  Task<List<Field>> GetFileFields(int appId);
  Task<List<FileInfoRequest>> GetFileRequests(int appId, List<Field> fileFields, List<int> filesFilter);
  Task<List<FileInfoResult>> GetFileInfos(List<FileInfoRequest> fileRequests);
  void PrintReport(List<FileInfoResult> fileInfos, string outputDirectory);
}
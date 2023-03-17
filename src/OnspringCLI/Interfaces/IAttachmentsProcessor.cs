namespace OnspringCLI.Interfaces;

public interface IAttachmentsProcessor
{
  Task<List<Field>> GetFileFields();
  Task<List<FileInfoRequest>> GetFileRequests(List<Field> fileFields);
  Task<List<FileInfoResult>> GetFileInfos(List<FileInfoRequest> fileRequests);
  void PrintReport(List<FileInfoResult> fileInfos);
}
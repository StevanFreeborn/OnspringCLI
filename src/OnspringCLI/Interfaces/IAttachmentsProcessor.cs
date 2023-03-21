namespace OnspringCLI.Interfaces;

public interface IAttachmentsProcessor
{
  Task<List<Field>> GetFileFields(
    int appId,
    List<int>? fieldsFilter = null
  );

  Task<List<OnspringFileRequest>> GetFileRequests(
    int appId,
    List<Field> fileFields,
    List<int>? filesFilter = null,
    List<int>? recordsFilter = null
  );

  Task<List<OnspringFileInfoResult>> GetFileInfos(
    List<OnspringFileRequest> fileRequests
  );

  Task<OnspringFileResult?> GetFile(
    OnspringFileRequest fileRequest,
    string outputDirectory
  );

  Task<bool> TrySaveFile(OnspringFileResult file);

  Task<bool> TryDeleteFile(
    OnspringFileRequest fileRequest
  );

  Task<List<int>> GetRecordIdsFromReport(int reportId);

  Task<bool> ValidateMatchFields(
    IAttachmentTransferSettings settings
  );

  Task<bool> ValidateFlagFieldIdAndValues(
    IAttachmentTransferSettings settings
  );

  Task<List<ResultRecord>> GetSourceRecords(
    int sourceAppId,
    List<int> sourceFieldIds,
    List<int>? recordsFilter = null
  );

  Task TransferAttachments(
    IAttachmentTransferSettings settings,
    ResultRecord sourceRecord
  );

  void WriteFileInfoReport(
    List<OnspringFileInfoResult> fileInfos,
    string outputDirectory
  );

  void WriteFileRequestErrorReport(
    List<OnspringFileRequest> erroredRequests,
    string outputDirectory
  );
}
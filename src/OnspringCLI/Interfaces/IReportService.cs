namespace OnspringCLI.Interfaces;

public interface IReportService
{
  void WriteReport(List<FileInfoResult> fileInfos);
}
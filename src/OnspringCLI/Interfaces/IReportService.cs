namespace OnspringCLI.Interfaces;

public interface IReportService
{
  void WriteCsvFileInfoReport(List<OnspringFileInfoResult> fileInfos, string outputDirectory);
}
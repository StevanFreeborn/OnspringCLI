namespace OnspringCLI.Interfaces;

public interface IReportService
{
  void WriteCsvReport<T>(List<T> records, Type mapType, string outputDirectory, string fileName);
}
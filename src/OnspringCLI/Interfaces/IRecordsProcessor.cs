

namespace OnspringCLI.Interfaces;

public interface IRecordsProcessor
{
  Task<List<App>> GetApps();
  Task<List<ReferenceField>> GetReferenceFields(int sourceAppId, int targetAppId);
  Task<List<RecordReference>> GetReferences(App sourceApp, List<ReferenceField> referenceFields, List<int> recordIds);
  void WriteReferencesReport(List<RecordReference> references, string outputDirectory);
}
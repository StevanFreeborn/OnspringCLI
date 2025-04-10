namespace OnspringCLI.Interfaces;

public interface IRecordsProcessor
{
  Task<List<App>> GetApps();
  IAsyncEnumerable<ResultRecord> GetRecords(App app, List<Field> fields);
  Task<List<Field>> GetFieldsForApp(int appId);
  Task<List<ReferenceField>> GetReferenceFields(int sourceAppId, int targetAppId);
  Task<List<RecordReference>> GetReferences(App sourceApp, List<ReferenceField> referenceFields, List<int> recordIds);
  void WriteReferencesReport(List<RecordReference> references, string outputDirectory);
  Task<ResultRecord?> UpdateRecordYearValues(string appName, ResultRecord record, List<Field> fields, int years);
}
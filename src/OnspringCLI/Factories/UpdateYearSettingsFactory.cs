
namespace OnspringCLI.Factories;

internal class UpdateYearSettingsFactory : IUpdateYearSettingsFactory
{
  public async Task<UpdateYearSettings> CreateAsync(FileInfo? file)
  {
    var fieldsToUpdate = new Dictionary<string, List<string>>();

    if (file is null)
    {
      throw new InvalidOperationException("The file path is not set.");
    }

    using var reader = new StreamReader(file.FullName);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    csv.Context.RegisterClassMap<FieldToUpdateMap>();

    await foreach (var record in csv.GetRecordsAsync<FieldToUpdate>())
    {
      if (fieldsToUpdate.TryGetValue(record.AppName, out var value))
      {
        value.Add(record.FieldName);
        continue;
      }

      fieldsToUpdate.Add(record.AppName, [record.FieldName]);
    }

    return new(fieldsToUpdate);
  }

  private class FieldToUpdateMap : ClassMap<FieldToUpdate>
  {
    public FieldToUpdateMap()
    {
      Map(static m => m.AppName).Name("AppName", "App Name", "App");
      Map(static m => m.FieldName).Name("FieldName", "Field Name", "Field");
    }
  }
}
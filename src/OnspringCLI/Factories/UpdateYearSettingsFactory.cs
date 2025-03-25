
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
}
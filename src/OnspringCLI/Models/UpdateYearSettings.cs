namespace OnspringCLI.Models;

public record UpdateYearSettings(Dictionary<string, List<string>> AppFieldsMap)
{
  public AppsValidationResult ValidateApps(List<App> apps)
  {
    var appsFound = apps.Where(a => AppFieldsMap.ContainsKey(a.Name)).ToList();
    var appsNotFound = AppFieldsMap.Keys.Except(apps.Select(a => a.Name)).ToList();
    return new(appsNotFound.Count == 0, appsNotFound, appsFound);
  }

  public FieldsValidationResult ValidateFields(App app, List<Field> fields)
  {
    var fieldsLookingFor = AppFieldsMap[app.Name];
    var foundFields = fields.Where(f => fieldsLookingFor
      .Contains(f.Name, StringComparer.OrdinalIgnoreCase))
      .ToList();

    var notFoundFields = fieldsLookingFor.Except(foundFields.Select(f => f.Name)).ToList();

    var invalidFields = foundFields
      .Where(f => f.Type is not FieldType.List and not FieldType.Date)
      .Select(f => f.Name)
      .ToList();

    return new(
      notFoundFields.Count + invalidFields.Count == 0,
      notFoundFields,
      invalidFields,
      foundFields
    );
  }
}

public record AppsValidationResult(
  bool IsValid,
  List<string> AppsNotFound,
  List<App> AppsFound
);

public record FieldsValidationResult(
  bool IsValid,
  List<string> FieldsNotFound,
  List<string> InvalidFields,
  List<Field> FieldsFound
);
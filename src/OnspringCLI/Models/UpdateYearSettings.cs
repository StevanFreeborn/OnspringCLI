namespace OnspringCLI.Models;

public record UpdateYearSettings(Dictionary<string, List<string>> AppFieldsMap)
{
  public AppsValidationResult ValidateApps(List<App> apps)
  {
    var appsFound = apps.Where(a => AppFieldsMap.ContainsKey(a.Name)).ToList();
    var appsNotFound = AppFieldsMap.Keys.Except(apps.Select(a => a.Name)).ToList();
    return new(appsNotFound.Count == 0, appsNotFound, appsFound);
  }
}

public record AppsValidationResult(bool IsValid, List<string> AppsNotFound, List<App> AppsFound);
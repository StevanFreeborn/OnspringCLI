namespace OnspringCLI.Tests.TestData;

public static class OptionsFactory
{
  public static string[] RequiredTransferOptionsWithValidSettingsFile =>
    new string[]
    {
      "--target-api-key",
      "123",
      "--settings-file",
      "TestData/Files/transfer.json"
    };

  public static string[] AllTransferOptions =>
    new string[]
    {
      "--target-api-key",
      "123",
      "--settings-file",
      "TestData/Files/transfer.json",
      "--report-filter",
      "1",
      "--records-filter",
      "1,2"
    };

  public static string[] RequiredTransferOptionsWithInvalidSettingsFile =>
    new string[]
    {
      "--target-api-key",
      "123",
      "--settings-file",
      "TestData/Files/fake.json"
    };

  public static string[] RequiredBulkDownloadOptions =>
    new string[]
    {
      "--app-id",
      "123",
    };

  public static string[] AllBulkDownloadOptions =>
    new string[]
    {
      "--app-id",
      "123",
      "--output-directory",
      "TestData/Files",
      "--fields-filter",
      "1,2",
      "--records-filter",
      "1,2",
      "--report-filter",
      "1"
    };

  public static string[] RequiredReportOptions =>
    new string[]
    {
      "--app-id",
      "123",
    };

  public static string[] AllReportOptions =>
    new string[]
    {
      "--app-id",
      "123",
      "--output-directory",
      "TestData/Files",
      "--files-filter",
      "1,2",
      "--files-filter-csv",
      "TestData/Files/testFilesFilter.csv"
    };

  public static string[] AllBulkDeleteOptions =>
    new string[]
    {
      "--app-id",
      "123",
      "--fields-filter",
      "1,2",
      "--records-filter",
      "1,2",
      "--report-filter",
      "1"
    };
}
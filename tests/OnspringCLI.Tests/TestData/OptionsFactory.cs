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
}
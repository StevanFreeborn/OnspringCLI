namespace OnspringCLI.Tests.TestData;

public static class TestFilesPathFactory
{
  public static string GetTestTransferSettingsFilePath() =>
    Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "TestData",
      "Files",
      "transfer.json"
    );

  public static string GetTestFilesFilterCsvPath() =>
    Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "TestData",
      "Files",
      "testFilesFilter.csv"
    );
}
namespace OnspringCLI.Tests.TestData;

public static class TestSettingsFileFactory
{
  public static string GetTestTransferSettingsFilePath() =>
    Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "TestData",
      "Files",
      "transfer.json"
    );
}
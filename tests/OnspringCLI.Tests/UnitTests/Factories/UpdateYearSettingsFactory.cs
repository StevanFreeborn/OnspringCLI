namespace OnspringCLI.Tests.UnitTests.Factories;

public class UpdateYearSettingsFactoryTests : IDisposable
{
  private readonly UpdateYearSettingsFactory _sut = new();
  private const string TestFilePath = "year-settings.csv";

  public UpdateYearSettingsFactoryTests()
  {
    CreateTestFile("AppName,FieldName\nApp1,Field1\nApp1,Field2\nApp2,Field1\nApp2,Field2");
  }

  [Fact]
  public async Task CreateAsync_WhenCalledWithNullFile_ItShouldThrowInvalidOperationException()
  {
    var action = () => _sut.CreateAsync(null);

    await action.Should().ThrowAsync<InvalidOperationException>();
  }

  [Fact]
  public async Task CreateAsync_WhenCalled_ItShouldReturnUpdateYearSettings()
  {
    var file = new FileInfo(TestFilePath);

    var result = await _sut.CreateAsync(file);

    result.Should().NotBeNull();
    result.AppFieldsMap["App1"].Should().BeEquivalentTo(new List<string> { "Field1", "Field2" });
    result.AppFieldsMap["App2"].Should().BeEquivalentTo(new List<string> { "Field1", "Field2" });
  }

  public void Dispose()
  {
    DeleteTestFile();
    GC.SuppressFinalize(this);
  }

  private static void CreateTestFile(string content)
  {
    File.WriteAllText(TestFilePath, content);
  }

  private static void DeleteTestFile()
  {
    File.Delete(TestFilePath);
  }
}

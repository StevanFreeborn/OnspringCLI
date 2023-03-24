namespace OnspringCLI.Tests.UnitTests.Models;

public class OnspringFileResultTests
{
  [Fact]
  public void OnspringFileResult_WhenCalled_ReturnsNewInstance()
  {
    var recordId = 1;
    var fieldId = 2;
    var fieldName = "Field";
    var fileId = 3;
    var fileName = "File";
    var filePath = "FilePath";
    var fileStream = new MemoryStream();

    var onspringFileResult = new OnspringFileResult(
      recordId,
      fieldId,
      fieldName,
      fileId,
      fileName,
      filePath,
      fileStream
    );

    onspringFileResult.Should().NotBeNull();
    onspringFileResult.RecordId.Should().Be(recordId);
    onspringFileResult.FieldId.Should().Be(fieldId);
    onspringFileResult.FieldName.Should().Be(fieldName);
    onspringFileResult.FileId.Should().Be(fileId);
    onspringFileResult.FileName.Should().Be(fileName);
    onspringFileResult.FilePath.Should().Be(filePath);
    onspringFileResult.Stream.Should().BeOfType<MemoryStream>();
  }
}
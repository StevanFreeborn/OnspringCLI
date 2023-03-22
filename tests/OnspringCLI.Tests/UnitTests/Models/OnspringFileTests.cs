namespace OnspringCLI.Tests.UnitTests.Models;

public class OnspringFileTests
{
  [Fact]
  public void OnspringFile_WhenCalled_ReturnsNewInstance()
  {
    var recordId = 1;
    var fieldId = 2;
    var fieldName = "Field";
    var fileId = 3;
    var fileName = "File";

    var onspringFile = new OnspringFile(
      recordId,
      fieldId,
      fieldName,
      fileId,
      fileName
    );

    onspringFile.Should().NotBeNull();
    onspringFile.RecordId.Should().Be(recordId);
    onspringFile.FieldId.Should().Be(fieldId);
    onspringFile.FieldName.Should().Be(fieldName);
    onspringFile.FileId.Should().Be(fileId);
    onspringFile.FileName.Should().Be(fileName);
  }
}
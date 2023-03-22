namespace OnspringCLI.Tests.UnitTests.Models;

public class OnspringFileInfoResultTests
{
  [Fact]
  public void OnspringFileInfoResult_WhenCalled_ReturnsNewInstance()
  {
    var recordId = 1;
    var fieldId = 2;
    var fieldName = "FieldName";
    var fileId = 3;
    var fileName = "FileName";
    decimal fileSizeInBytes = 1073741824;
    var fileSizeInKB = Math.Round(fileSizeInBytes / 1000, 4);
    var fileSizeInKiB = Math.Round(fileSizeInBytes / 1024, 4);
    var fileSizeInMB = Math.Round(fileSizeInBytes / 1000000, 4);
    var fileSizeInMiB = Math.Round(fileSizeInBytes / 1048576, 4);
    var fileSizeInGB = Math.Round(fileSizeInBytes / 1000000000, 4);
    var fileSizeInGiB = Math.Round(fileSizeInBytes / 1073741824, 4);

    var onspringFileInfoResult = new OnspringFileInfoResult(
      recordId,
      fieldId,
      fieldName,
      fileId,
      fileName,
      fileSizeInBytes
    );

    onspringFileInfoResult.Should().NotBeNull();
    onspringFileInfoResult.RecordId.Should().Be(recordId);
    onspringFileInfoResult.FieldId.Should().Be(fieldId);
    onspringFileInfoResult.FieldName.Should().Be(fieldName);
    onspringFileInfoResult.FileId.Should().Be(fileId);
    onspringFileInfoResult.FileName.Should().Be(fileName);
    onspringFileInfoResult.FileSizeInBytes.Should().Be(fileSizeInBytes);
    onspringFileInfoResult.FileSizeInKB.Should().Be(fileSizeInKB);
    onspringFileInfoResult.FileSizeInKiB.Should().Be(fileSizeInKiB);
    onspringFileInfoResult.FileSizeInMB.Should().Be(fileSizeInMB);
    onspringFileInfoResult.FileSizeInMiB.Should().Be(fileSizeInMiB);
    onspringFileInfoResult.FileSizeInGB.Should().Be(fileSizeInGB);
    onspringFileInfoResult.FileSizeInGiB.Should().Be(fileSizeInGiB);
  }
}
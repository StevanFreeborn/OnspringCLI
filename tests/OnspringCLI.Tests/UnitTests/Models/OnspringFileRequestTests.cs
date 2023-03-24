namespace OnspringCLI.Tests.UnitTests.Models;

public class OnspringFileRequestTests
{
  [Fact]
  public void OnspringFileRequest_WhenCalled_ReturnsNewInstance()
  {
    var onspringFileRequest = new OnspringFileRequest();

    onspringFileRequest.Should().NotBeNull();
  }

  [Fact]
  public void OnspringFileRequest_WhenCalledWithParameters_ReturnsNewInstance()
  {
    var recordId = 1;
    var fieldId = 2;
    var fieldName = "Field";
    var fileId = 3;

    var onspringFileRequest = new OnspringFileRequest(
      recordId,
      fieldId,
      fieldName,
      fileId
    );

    onspringFileRequest.Should().NotBeNull();
    onspringFileRequest.RecordId.Should().Be(recordId);
    onspringFileRequest.FieldId.Should().Be(fieldId);
    onspringFileRequest.FieldName.Should().Be(fieldName);
    onspringFileRequest.FileId.Should().Be(fileId);
  }

  [Fact]
  public void OnspringFileRequest_WhenCalledWithParameters_ReturnsNewInstanceWithNullValues()
  {
    var recordId = 1;
    var fieldId = 2;
    var fileId = 3;

    var onspringFileRequest = new OnspringFileRequest(
      recordId,
      fieldId,
      fileId
    );

    onspringFileRequest.Should().NotBeNull();
    onspringFileRequest.RecordId.Should().Be(recordId);
    onspringFileRequest.FieldId.Should().Be(fieldId);
    onspringFileRequest.FileId.Should().Be(fileId);
  }
}
namespace OnspringCLI.Tests.UnitTests.Models;

public class RecordReferenceTests
{
  [Fact]
  public void ParameterlessConstructor_WhenCalled_ItShouldReturnProperlyConstructedInstance()
  {
    var reference = new RecordReference();

    reference.TargetAppId.Should().Be(0);
    reference.TargetRecordId.Should().Be(0);
    reference.SourceAppId.Should().Be(0);
    reference.SourceAppName.Should().BeEmpty();
    reference.SourceFieldId.Should().Be(0);
    reference.SourceFieldName.Should().BeEmpty();
    reference.SourceRecordId.Should().Be(0);
  }

  [Fact]
  public void ParameterizedConstructor_WhenCalled_ItShouldReturnProperlyConstructedInstance()
  {
    var reference = new RecordReference(
      1,
      2,
      3,
      "Source App Name",
      4,
      "Source Field Name",
      5
    );

    reference.TargetAppId.Should().Be(1);
    reference.TargetRecordId.Should().Be(2);
    reference.SourceAppId.Should().Be(3);
    reference.SourceAppName.Should().Be("Source App Name");
    reference.SourceFieldId.Should().Be(4);
    reference.SourceFieldName.Should().Be("Source Field Name");
    reference.SourceRecordId.Should().Be(5);
  }
}
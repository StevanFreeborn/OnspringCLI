namespace OnspringCLI.Tests.UnitTests.Models;

public class UpdateYearSettingsTests
{
  [Fact]
  public void ValidateApps_WhenGivenValidListOfApps_ItShouldReturnResult()
  {
    var appFieldsMap = new Dictionary<string, List<string>>()
    {
      { "App 1" , []},
    };

    var settings = new UpdateYearSettings(appFieldsMap);
    var testApp = new App() { Name = "App 1" };

    var result = settings.ValidateApps([testApp]);

    result.IsValid.Should().BeTrue();
    result.AppsNotFound.Should().BeEmpty();
    result.AppsFound.Should().BeEquivalentTo([testApp]);
  }

  [Fact]
  public void ValidateApps_WhenGivenInvalidListOfApps_ItShouldReturnResult()
  {
    var appFieldsMap = new Dictionary<string, List<string>>()
    {
      { "App 1" , []},
    };

    var settings = new UpdateYearSettings(appFieldsMap);

    var result = settings.ValidateApps([new App() { Name = "App 2" }]);

    result.IsValid.Should().BeFalse();
    result.AppsNotFound.Should().BeEquivalentTo(["App 1"]);
    result.AppsFound.Should().BeEmpty();
  }

  [Fact]
  public void ValidateFields_WhenGivenFieldsThatCanNotBeFound_ItShouldReturnResult()
  {
    var testApp = new App() { Name = "App 1" };
    var testField = new Field() { Name = "Field 1" };
    var appFieldsMap = new Dictionary<string, List<string>>()
    {
      { testApp.Name, [testField.Name]},
    };

    var settings = new UpdateYearSettings(appFieldsMap);
    var result = settings.ValidateFields(testApp, [new Field() { Name = "Field 2" }]);

    result.IsValid.Should().BeFalse();
    result.FieldsNotFound.Should().BeEquivalentTo([testField.Name]);
    result.FieldsFound.Should().BeEmpty();
    result.InvalidFields.Should().BeEmpty();
  }

  [Fact]
  public void ValidateFields_WhenGivenFieldsThatAreInvalid_ItShouldReturnResult()
  {
    var testApp = new App() { Name = "App 1" };
    var testField = new ReferenceField() { Name = "Field 1", };
    var appFieldsMap = new Dictionary<string, List<string>>()
    {
      { testApp.Name, [testField.Name] }
    };

    var settings = new UpdateYearSettings(appFieldsMap);
    var result = settings.ValidateFields(testApp, [testField]);

    result.IsValid.Should().BeFalse();
    result.FieldsNotFound.Should().BeEmpty();
    result.FieldsFound.Should().BeEquivalentTo([testField]);
    result.InvalidFields.Should().NotBeEquivalentTo([testField]);
  }

  [Fact]
  public void ValidateFields_WhenGivenValidListOfFields_ItShouldReturnResult()
  {
    var testApp = new App() { Name = "App 1" };
    var testDateField = new Field() { Name = "Field 1", Type = FieldType.Date };
    var testListField = new Field() { Name = "Field 2", Type = FieldType.List };
    var appFieldsMap = new Dictionary<string, List<string>>()
    {
      { testApp.Name, [testDateField.Name, testListField.Name] }
    };

    var settings = new UpdateYearSettings(appFieldsMap);
    var result = settings.ValidateFields(testApp, [testDateField, testListField]);

    result.IsValid.Should().BeTrue();
    result.FieldsNotFound.Should().BeEmpty();
    result.FieldsFound.Should().BeEquivalentTo([testDateField, testListField]);
    result.InvalidFields.Should().BeEmpty();
  }
}
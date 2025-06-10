namespace OnspringCLI.Tests.UnitTests.Commands.Records.Update.Bulk;

public class YearCommandTests
{
  [Fact]
  public void YearCommand_WhenCalled_ReturnsNewInstance()
  {
    var yearCommand = new YearCommand();

    yearCommand.Should().NotBeNull();
    yearCommand.Name.Should().Be("year");
    yearCommand.Description.Should().Be("Adjusts the value of a list and/or date field by given years");
  }

  public class HandlerTests
  {
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IRecordsProcessor> _processorMock = new();
    private readonly Mock<IUpdateYearSettingsFactory> _settingsFactoryMock = new();
    private readonly YearCommand.Handler _handler;
    private readonly YearCommand _command;

    public HandlerTests()
    {
      _loggerMock
        .Setup(static x => x.ForContext<It.IsAnyType>())
        .Returns(_loggerMock.Object);

      _handler = new YearCommand.Handler(
        _loggerMock.Object,
        _processorMock.Object,
        _settingsFactoryMock.Object
      );

      _command = [];
      _command.SetHandler(_handler.InvokeAsync);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndNoAppsAreFound_ItShouldReturnNonZeroValue()
    {
      var map = new Dictionary<string, List<string>>
      {
        { "App1", new List<string> { "Field1", "Field2" } },
        { "App2", new List<string> { "Field3" } }
      };

      _settingsFactoryMock
        .Setup(static x => x.CreateAsync(It.IsAny<FileInfo>()))
        .ReturnsAsync(new UpdateYearSettings(map));

      _processorMock
        .Setup(static x => x.GetApps())
        .ReturnsAsync([]);

      var result = await _command.InvokeAsync(OptionsFactory.RequiredYearOptions);

      result.Should().NotBe(0);
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndFieldsAreNotFound_ItShouldReturnNonZeroValue()
    {
      var apps = new List<App>
      {
        new() { Name = "App1" },
        new() { Name = "App2" }
      };

      var map = apps.ToDictionary(
        static app => app.Name,
        static app => new List<string> { "Field1", "Field2" }
      );

      _settingsFactoryMock
        .Setup(static x => x.CreateAsync(It.IsAny<FileInfo>()))
        .ReturnsAsync(new UpdateYearSettings(map));

      _processorMock
        .Setup(static x => x.GetApps())
        .ReturnsAsync(apps);

      _processorMock
        .Setup(static x => x.GetFieldsForApp(It.IsAny<int>()))
        .ReturnsAsync([]);

      var result = await _command.InvokeAsync(OptionsFactory.RequiredYearOptions);

      result.Should().NotBe(0);

      _processorMock
        .Verify(
          static x => x.GetFieldsForApp(It.IsAny<int>()),
          Times.Exactly(apps.Count)
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndInvalidFieldsAreFound_ItShouldReturnNonZeroValue()
    {
      var apps = new List<App>
      {
        new() { Name = "App1" },
        new() { Name = "App2" }
      };

      var fields = new List<Field>
      {
        new() { Name = "Field1", Type = FieldType.Text },
        new() { Name = "Field2", Type = FieldType.List }
      };

      var map = apps.ToDictionary(
        static app => app.Name,
        app => fields.Select(static f => f.Name).ToList()
      );

      _settingsFactoryMock
        .Setup(static x => x.CreateAsync(It.IsAny<FileInfo>()))
        .ReturnsAsync(new UpdateYearSettings(map));

      _processorMock
        .Setup(static x => x.GetApps())
        .ReturnsAsync(apps);

      _processorMock
        .Setup(static x => x.GetFieldsForApp(It.IsAny<int>()))
        .ReturnsAsync(fields);

      var result = await _command.InvokeAsync(OptionsFactory.RequiredYearOptions);

      result.Should().NotBe(0);

      _processorMock
        .Verify(
          static x => x.GetFieldsForApp(It.IsAny<int>()),
          Times.Exactly(apps.Count)
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenCalledAndAllAppsAndFieldsAreValid_ItShouldReturnZero()
    {
      var apps = new List<App>
      {
        new() { Id = 1, Name = "App1" },
        new() { Id = 2, Name = "App2" }
      };

      var fields = new List<Field>
      {
        new() { Id = 1, Name = "Field1", Type = FieldType.List },
        new() { Id = 2, Name = "Field2", Type = FieldType.Date }
      };

      var map = apps.ToDictionary(
        static app => app.Name,
        app => fields.Select(static f => f.Name).ToList()
      );

      _settingsFactoryMock
        .Setup(static x => x.CreateAsync(It.IsAny<FileInfo>()))
        .ReturnsAsync(new UpdateYearSettings(map));

      _processorMock
        .Setup(static x => x.GetApps())
        .ReturnsAsync(apps);

      _processorMock
        .Setup(static x => x.GetFieldsForApp(It.IsAny<int>()))
        .ReturnsAsync(fields);

      static async IAsyncEnumerable<ResultRecord> GetRecords()
      {
        var records = new List<ResultRecord>()
        {
          new(),
          new(),
          new()
        };

        foreach (var record in records)
        {
          await Task.Delay(10);
          yield return record;
        }
      }

      _processorMock
        .Setup(static x => x.UpdateRecordYearValues(
          It.IsAny<string>(),
          It.IsAny<ResultRecord>(),
          It.IsAny<List<Field>>(),
          It.IsAny<int>()
        ))
        .ReturnsAsync(new ResultRecord());

      _processorMock
        .Setup(static x => x.GetRecords(It.IsAny<App>(), It.IsAny<List<Field>>()))
        .Returns(GetRecords);

      var result = await _command.InvokeAsync(OptionsFactory.RequiredYearOptions);

      result.Should().Be(0);
    }
  }
}
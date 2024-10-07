namespace OnspringCLI.Tests.UnitTests.Processors;

public class RecordsProcessorTests
{
  private readonly Mock<ILogger> _loggerMock = new();
  private readonly Mock<IReportService> _reportServiceMock = new();
  private readonly Mock<IOnspringService> _onspringServiceMock = new();
  private readonly Mock<IOptions<GlobalOptions>> _globalOptionsMock = new();
  private readonly RecordsProcessor _processor;

  public RecordsProcessorTests()
  {
    _globalOptionsMock
      .SetupGet(m => m.Value)
      .Returns(new GlobalOptions
      {
        SourceApiKey = "sourceApiKey",
        LogLevel = LogEventLevel.Verbose,
      });

    _loggerMock
      .Setup(m => m.ForContext<It.IsAnyType>())
      .Returns(_loggerMock.Object);

    _processor = new RecordsProcessor(
      _loggerMock.Object,
      _reportServiceMock.Object,
      _onspringServiceMock.Object,
      _globalOptionsMock.Object
    );
  }

  [Fact]
  public async Task GetApps_WhenCalled_ItShouldReturnApps()
  {
    var apps = new List<App> { new() };

    _onspringServiceMock
      .Setup(x => x.GetApps(It.IsAny<string>()))
      .ReturnsAsync(apps);

    var result = await _processor.GetApps();

    result.Should().BeEquivalentTo(apps);
  }

  [Fact]
  public async Task GetReferenceFields_WhenCalledAndNoReferenceFields_ItShouldReturnEmptyList()
  {
    _onspringServiceMock
      .Setup(x => x.GetAllFields(It.IsAny<string>(), It.IsAny<int>()))
      .ReturnsAsync([]);

    var result = await _processor.GetReferenceFields(It.IsAny<int>(), It.IsAny<int>());

    result.Should().BeEmpty();
  }

  [Fact]
  public async Task GetReferenceFields_WhenCalledAndReferenceFields_ItShouldReturnReferenceFields()
  {
    var fields = new List<Field>
    {
      new ReferenceField { Type = FieldType.Reference, ReferencedAppId = 1 },
      new ReferenceField { Type = FieldType.Reference, ReferencedAppId = 2 },
      new ReferenceField { Type = FieldType.Text },
    };

    _onspringServiceMock
      .Setup(x => x.GetAllFields(It.IsAny<string>(), It.IsAny<int>()))
      .ReturnsAsync(fields);

    var result = await _processor.GetReferenceFields(It.IsAny<int>(), 2);

    result.Should().HaveCount(1);
    result.First().ReferencedAppId.Should().Be(2);
  }

  [Fact]
  public async Task GetReferences_WhenCalledAndSourceAppHasNoRecords_ItShouldReturnEmptyList()
  {
    var sourceApp = new App { Id = 1 };
    var referenceFields = new List<ReferenceField> { new() };
    var recordIds = new List<int> { 1 };

    _onspringServiceMock
      .Setup(x => x.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ))
      .ReturnsAsync(null as GetPagedRecordsResponse);

    var result = await _processor.GetReferences(sourceApp, referenceFields, recordIds);

    result.Should().BeEmpty();
  }

  [Fact]
  public async Task GetReferences_WhenCalledAndSourceAppHasOnePageOfRecordsWithoutReferences_ItShouldReturnEmptyList()
  {
    var sourceApp = new App { Id = 1 };
    var referenceFields = new List<ReferenceField> { new() };
    var recordIds = new List<int> { 1 };

    var page = new GetPagedRecordsResponse()
    {
      TotalRecords = 1,
      PageNumber = 1,
      TotalPages = 1,
      Items = [new()],
    };

    _onspringServiceMock
      .Setup(x => x.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ))
      .ReturnsAsync(page);

    var result = await _processor.GetReferences(sourceApp, referenceFields, recordIds);

    result.Should().BeEmpty();
  }

  [Fact]
  public async Task GetReferences_WhenCalledAndSourceAppHasOnePageOfRecordsWithReferences_ItShouldReturnReferences()
  {
    var targetRecordId = 1;

    var sourceApp = new App { Id = 1, Name = "App 1" };
    var referenceFields = new List<ReferenceField>
    {
      new()
      {
        Id = 1,
        Name = "Reference Field 1",
        AppId = sourceApp.Id,
        ReferencedAppId = 2,
      },
      new()
      {
        Id = 2,
        Name = "Reference Field 2",
        AppId = sourceApp.Id,
        ReferencedAppId = 2,
      },
    };

    var recordIds = new List<int> { targetRecordId };

    var page = new GetPagedRecordsResponse()
    {
      TotalRecords = 1,
      PageNumber = 1,
      TotalPages = 1,
      Items = [
        new()
        {
          RecordId = 1,
          FieldData = [
            new IntegerFieldValue()
            {
              FieldId = referenceFields.First().Id,
              Value = targetRecordId
            },
            new IntegerListFieldValue()
            {
              FieldId = referenceFields.Last().Id,
              Value = [targetRecordId, 2]
            },
            new IntegerFieldValue()
            {
              FieldId = 3,
              Value = 2
            }
          ]
        }
      ],
    };

    _onspringServiceMock
      .Setup(x => x.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ))
      .ReturnsAsync(page);

    var result = await _processor.GetReferences(sourceApp, referenceFields, recordIds);

    result.Should().BeEquivalentTo([
      new RecordReference()
      {
        TargetAppId = referenceFields.First().ReferencedAppId,
        TargetRecordId = targetRecordId,
        SourceAppId = sourceApp.Id,
        SourceAppName = sourceApp.Name,
        SourceFieldId = referenceFields.First().Id,
        SourceFieldName = referenceFields.First().Name,
        SourceRecordId = page.Items.First().RecordId,
      },
      new RecordReference()
      {
        TargetAppId = referenceFields.Last().ReferencedAppId,
        TargetRecordId = targetRecordId,
        SourceAppId = sourceApp.Id,
        SourceAppName = sourceApp.Name,
        SourceFieldId = referenceFields.Last().Id,
        SourceFieldName = referenceFields.Last().Name,
        SourceRecordId = page.Items.First().RecordId,
      }
    ]);
  }

  [Fact]
  public async Task GetReferences_WhenCalledAndSourceAppHasMultiplePagesOfRecordsWithReferences_ItShouldReturnReferences()
  {
    var targetRecordId = 1;

    var sourceApp = new App { Id = 1, Name = "App 1" };
    var referenceFields = new List<ReferenceField>
    {
      new()
      {
        Id = 1,
        Name = "Reference Field 1",
        AppId = sourceApp.Id,
        ReferencedAppId = 2,
      },
      new()
      {
        Id = 2,
        Name = "Reference Field 2",
        AppId = sourceApp.Id,
        ReferencedAppId = 2,
      }
    };

    var recordIds = new List<int> { targetRecordId };

    var pageOne = new GetPagedRecordsResponse()
    {
      TotalRecords = 2,
      PageNumber = 1,
      TotalPages = 2,
      Items = [
        new()
        {
          RecordId = 1,
          FieldData = [
            new IntegerFieldValue()
            {
              FieldId = referenceFields.First().Id,
              Value = targetRecordId
            },
          ]
        }
      ],
    };

    var pageTwo = new GetPagedRecordsResponse()
    {
      TotalRecords = 2,
      PageNumber = 2,
      TotalPages = 2,
      Items = [
        new()
        {
          RecordId = 2,
          FieldData = [
            new IntegerListFieldValue()
            {
              FieldId = referenceFields.Last().Id,
              Value = [targetRecordId, 2]
            }
          ]
        }
      ],
    };

    _onspringServiceMock
      .SetupSequence(x => x.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ))
      .ReturnsAsync(pageOne)
      .ReturnsAsync(pageTwo);

    var result = await _processor.GetReferences(sourceApp, referenceFields, recordIds);

    result.Should().BeEquivalentTo([
      new RecordReference()
      {
        TargetAppId = referenceFields.First().ReferencedAppId,
        TargetRecordId = targetRecordId,
        SourceAppId = sourceApp.Id,
        SourceAppName = sourceApp.Name,
        SourceFieldId = referenceFields.First().Id,
        SourceFieldName = referenceFields.First().Name,
        SourceRecordId = pageOne.Items.First().RecordId,
      },
      new RecordReference()
      {
        TargetAppId = referenceFields.Last().ReferencedAppId,
        TargetRecordId = targetRecordId,
        SourceAppId = sourceApp.Id,
        SourceAppName = sourceApp.Name,
        SourceFieldId = referenceFields.Last().Id,
        SourceFieldName = referenceFields.Last().Name,
        SourceRecordId = pageTwo.Items.First().RecordId,
      }
    ]);
  }

  [Fact]
  public void WriteReferencesReport_WhenCalled_ItShouldWriteReferencesToCsv()
  {
    _processor.WriteReferencesReport([], "output");

    _reportServiceMock.Verify(
      m => m.WriteCsvReport(
        It.IsAny<List<RecordReference>>(),
        typeof(RecordReferenceMap),
        It.IsAny<string>(),
        It.IsAny<string>()
      ),
      Times.Once
    );
  }
}
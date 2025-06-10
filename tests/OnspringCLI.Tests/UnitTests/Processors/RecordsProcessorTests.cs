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
      .SetupGet(static m => m.Value)
      .Returns(new GlobalOptions
      {
        SourceApiKey = "sourceApiKey",
        LogLevel = LogEventLevel.Verbose,
      });

    _loggerMock
      .Setup(static m => m.ForContext<It.IsAnyType>())
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
      .Setup(static x => x.GetApps(It.IsAny<string>()))
      .ReturnsAsync(apps);

    var result = await _processor.GetApps();

    result.Should().BeEquivalentTo(apps);
  }

  [Fact]
  public async Task GetReferenceFields_WhenCalledAndNoReferenceFields_ItShouldReturnEmptyList()
  {
    _onspringServiceMock
      .Setup(static x => x.GetAllFields(It.IsAny<string>(), It.IsAny<int>()))
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
      .Setup(static x => x.GetAllFields(It.IsAny<string>(), It.IsAny<int>()))
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
      .Setup(static x => x.GetAPageOfRecords(
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
      .Setup(static x => x.GetAPageOfRecords(
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
      new()
      {
        Id = 3,
        Name = "Reference Field 3",
        AppId = sourceApp.Id,
        ReferencedAppId = 2,
      }
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
              FieldId = referenceFields[0].Id,
              Value = targetRecordId
            },
            new IntegerListFieldValue()
            {
              FieldId = referenceFields[1].Id,
              Value = [targetRecordId, 2]
            },
            new IntegerFieldValue()
            {
              FieldId = referenceFields[2].Id,
              Value = 2
            }
          ]
        }
      ],
    };

    _onspringServiceMock
      .Setup(static x => x.GetAPageOfRecords(
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
        TargetAppId = referenceFields[0].ReferencedAppId,
        TargetRecordId = targetRecordId,
        SourceAppId = sourceApp.Id,
        SourceAppName = sourceApp.Name,
        SourceFieldId = referenceFields[0].Id,
        SourceFieldName = referenceFields[0].Name,
        SourceRecordId = page.Items.First().RecordId,
      },
      new RecordReference()
      {
        TargetAppId = referenceFields[1].ReferencedAppId,
        TargetRecordId = targetRecordId,
        SourceAppId = sourceApp.Id,
        SourceAppName = sourceApp.Name,
        SourceFieldId = referenceFields[1].Id,
        SourceFieldName = referenceFields[1].Name,
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
      .SetupSequence(static x => x.GetAPageOfRecords(
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
      static m => m.WriteCsvReport(
        It.IsAny<List<RecordReference>>(),
        typeof(RecordReferenceMap),
        It.IsAny<string>(),
        It.IsAny<string>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task GetFieldsForApp_WhenCalled_ItShouldReturnFields()
  {
    var fields = new List<Field> { new() };

    _onspringServiceMock
      .Setup(static x => x.GetAllFields(It.IsAny<string>(), It.IsAny<int>()))
      .ReturnsAsync(fields);

    var result = await _processor.GetFieldsForApp(It.IsAny<int>());

    result.Should().BeEquivalentTo(fields);
  }

  [Fact]
  public async Task GetRecords_WhenCalledAndPageIsNull_ItShouldReturnEmptyList()
  {
    _onspringServiceMock
      .Setup(static x => x.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ))
      .ReturnsAsync(null as GetPagedRecordsResponse);

    var result = new List<ResultRecord>();

    await foreach (var record in _processor.GetRecords(new(), []))
    {
      result.Add(record);
    }

    result.Should().BeEmpty();
  }

  [Fact]
  public async Task GetRecords_WhenCalledAndPageHasNoRecords_ItShouldReturnEmptyList()
  {
    var page = new GetPagedRecordsResponse
    {
      TotalRecords = 0,
      PageNumber = 1,
      TotalPages = 1,
      Items = []
    };

    _onspringServiceMock
      .Setup(static x => x.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ))
      .ReturnsAsync(page);

    var result = new List<ResultRecord>();

    await foreach (var record in _processor.GetRecords(new(), []))
    {
      result.Add(record);
    }

    result.Should().BeEmpty();
  }

  [Fact]
  public async Task GetRecords_WhenCalledAndPageHasRecords_ItShouldReturnRecords()
  {
    var page = new GetPagedRecordsResponse
    {
      TotalRecords = 1,
      PageNumber = 1,
      TotalPages = 1,
      Items = [new() { RecordId = 1, FieldData = [] }]
    };

    _onspringServiceMock
      .Setup(static x => x.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ))
      .ReturnsAsync(page);

    var result = new List<ResultRecord>();

    await foreach (var record in _processor.GetRecords(new(), [new()]))
    {
      result.Add(record);
    }

    result.Should().HaveCount(1);
    result.First().RecordId.Should().Be(1);
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldHasNoValue_ItShouldNotUpdateRecord()
  {
    var fields = new List<Field>
    {
      new()
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [],
    };

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsNotListOrDate_ItShouldNotUpdateRecord()
  {
    var textField = new Field
    {
      Id = 1,
      Type = FieldType.Text
    };

    var fields = new List<Field>
    {
      textField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new StringFieldValue { FieldId = textField.Id, Value = "Not a year" }
      ],
    };

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsMultiSelectList_ItShouldNotUpdateRecord()
  {
    var multiSelectField = new ListField
    {
      Id = 1,
      Type = FieldType.List,
      Multiplicity = Multiplicity.MultiSelect
    };

    var fields = new List<Field>
    {
      multiSelectField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new GuidListFieldValue { FieldId = multiSelectField.Id, Value = [] }
      ],
    };

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsSingleSelectListButValueIsNull_ItShouldNotUpdateRecord()
  {
    var singleSelectField = new ListField
    {
      Id = 1,
      Type = FieldType.List,
      Multiplicity = Multiplicity.SingleSelect
    };

    var fields = new List<Field>
    {
      singleSelectField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new GuidFieldValue { FieldId = singleSelectField.Id, Value = null }
      ],
    };

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsSingleSelectListButUnableToFindListValue_ItShouldNotUpdateRecord()
  {
    var singleSelectField = new ListField
    {
      Id = 1,
      Type = FieldType.List,
      Multiplicity = Multiplicity.SingleSelect,
      Values = []
    };

    var fields = new List<Field>
    {
      singleSelectField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new GuidFieldValue { FieldId = singleSelectField.Id, Value = Guid.NewGuid() }
      ],
    };

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsSingleSelectListButValueIsNotAYear_ItShouldNotUpdateRecord()
  {
    var listValue = new ListValue
    {
      Id = Guid.NewGuid(),
      Name = "Not a year",
    };

    var singleSelectField = new ListField
    {
      Id = 1,
      Type = FieldType.List,
      Multiplicity = Multiplicity.SingleSelect,
      Values = [listValue]
    };

    var fields = new List<Field>
    {
      singleSelectField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new GuidFieldValue { FieldId = singleSelectField.Id, Value = listValue.Id }
      ],
    };

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsSingleSelectListButUnableToAddNewValue_ItShouldNotUpdateRecord()
  {
    var listValue = new ListValue
    {
      Id = Guid.NewGuid(),
      Name = "2023",
    };

    var singleSelectField = new ListField
    {
      Id = 1,
      Type = FieldType.List,
      Multiplicity = Multiplicity.SingleSelect,
      Values = [listValue]
    };

    var fields = new List<Field>
    {
      singleSelectField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new GuidFieldValue { FieldId = singleSelectField.Id, Value = listValue.Id }
      ],
    };

    _onspringServiceMock
      .Setup(static x => x.GetOrAddListValueByName(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ListValue>()))
      .ReturnsAsync(null as Guid?);

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsSingleSelectListAndValueIsAYearButUpdatingRecordFailsItShouldReturnNull()
  {
    var year = 2023;
    var numberOfYears = 1;
    var expectedYear = year + numberOfYears;

    var listValue = new ListValue
    {
      Id = Guid.NewGuid(),
      Name = year.ToString(CultureInfo.InvariantCulture),
    };

    var singleSelectField = new ListField
    {
      Id = 1,
      Type = FieldType.List,
      Multiplicity = Multiplicity.SingleSelect,
      Values = [listValue]
    };

    var fields = new List<Field>
    {
      singleSelectField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new GuidFieldValue { FieldId = singleSelectField.Id, Value = listValue.Id }
      ],
    };

    var newListValueGuid = Guid.NewGuid();

    _onspringServiceMock
      .Setup(x => x.GetOrAddListValueByName(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.Is<ListValue>(
          x => x.Name == expectedYear.ToString(CultureInfo.InvariantCulture) &&
            x.NumericValue == expectedYear
        )
      ))
      .ReturnsAsync(newListValueGuid);

    _onspringServiceMock
      .Setup(static x => x.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()))
      .ReturnsAsync(null as CreatedWithIdResponse<int>);

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, numberOfYears);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Once
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsSingleSelectListAndValueIsAYear_ItShouldUpdateRecord()
  {
    var year = 2023;
    var numberOfYears = 1;
    var expectedYear = year + numberOfYears;

    var listValue = new ListValue
    {
      Id = Guid.NewGuid(),
      Name = year.ToString(CultureInfo.InvariantCulture),
    };

    var singleSelectField = new ListField
    {
      Id = 1,
      Type = FieldType.List,
      Multiplicity = Multiplicity.SingleSelect,
      Values = [listValue]
    };

    var fields = new List<Field>
    {
      singleSelectField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new GuidFieldValue { FieldId = singleSelectField.Id, Value = listValue.Id }
      ],
    };

    var newListValueGuid = Guid.NewGuid();

    _onspringServiceMock
      .Setup(x => x.GetOrAddListValueByName(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.Is<ListValue>(x => x.Name == expectedYear.ToString(CultureInfo.InvariantCulture) &&
          x.NumericValue == expectedYear
        )
      ))
      .ReturnsAsync(newListValueGuid);

    _onspringServiceMock
      .Setup(static x => x.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()))
      .ReturnsAsync(new CreatedWithIdResponse<int>(1));

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result!.FieldData.Should().HaveCount(1);
    result.FieldData.First().As<GuidFieldValue>().Value.Should().Be(newListValueGuid);

    _onspringServiceMock.Verify(
      m => m.UpdateRecord(It.IsAny<string>(), result),
      Times.Once
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsDateButValueIsNull_ItShouldNotUpdateRecord()
  {
    var dateField = new Field
    {
      Id = 1,
      Type = FieldType.Date,
    };

    var fields = new List<Field>
    {
      dateField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new DateFieldValue { FieldId = dateField.Id, Value = null }
      ],
    };

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, 1);

    result.Should().BeNull();

    _onspringServiceMock.Verify(
      static m => m.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()),
      Times.Never
    );
  }

  [Fact]
  public async Task UpdateRecordYearValues_WhenCalledAndFieldIsDate_ItShouldUpdateRecord()
  {
    var year = 2023;
    var numberOfYears = 1;
    var expectedYear = year + numberOfYears;

    var dateField = new Field
    {
      Id = 1,
      Type = FieldType.Date,
    };

    var fields = new List<Field>
    {
      dateField,
    };

    var record = new ResultRecord
    {
      RecordId = 1,
      FieldData = [
        new DateFieldValue
        {
          FieldId = dateField.Id,
          Value = new DateTime(year, 1, 1)
        }
      ],
    };

    _onspringServiceMock
      .Setup(static x => x.UpdateRecord(It.IsAny<string>(), It.IsAny<ResultRecord>()))
      .ReturnsAsync(new CreatedWithIdResponse<int>(1));

    var result = await _processor.UpdateRecordYearValues("appName", record, fields, numberOfYears);

    result!.FieldData.Should().HaveCount(1);
    result.FieldData.First().As<DateFieldValue>().Value.Should().Be(new(expectedYear, 1, 1));

    _onspringServiceMock.Verify(
      m => m.UpdateRecord(It.IsAny<string>(), result),
      Times.Once
    );
  }
}
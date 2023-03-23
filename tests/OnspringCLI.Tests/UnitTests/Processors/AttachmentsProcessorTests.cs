namespace OnspringCLI.Tests.UnitTests.Processors;

public class AttachmentsProcessorTests
{
  private readonly Mock<IOptions<GlobalOptions>> _globalOptionsMock;
  private readonly Mock<IOnspringService> _onspringServiceMock;
  private readonly Mock<IReportService> _reportServiceMock;
  private readonly Mock<ILogger> _loggerMock;
  private readonly LoggingLevelSwitch _loggingLevelSwitch;
  private readonly Mock<IProgressBarFactory> _progressBarFactoryMock;
  private readonly AttachmentsProcessor _attachmentsProcessor;

  public AttachmentsProcessorTests()
  {
    _globalOptionsMock = new Mock<IOptions<GlobalOptions>>();

    _globalOptionsMock
    .SetupGet(
      m => m.Value
    )
    .Returns(
      new GlobalOptions
      {
        SourceApiKey = "sourceApiKey",
        TargetApiKey = "targetApiKey",
        LogLevel = LogEventLevel.Verbose,
      }
    );

    _onspringServiceMock = new Mock<IOnspringService>();
    _reportServiceMock = new Mock<IReportService>();
    _loggerMock = new Mock<ILogger>();

    _loggerMock
    .Setup(
      m => m.ForContext<It.IsAnyType>()
    )
    .Returns(
      _loggerMock.Object
    );


    _loggingLevelSwitch = new LoggingLevelSwitch();
    _progressBarFactoryMock = new Mock<IProgressBarFactory>();

    _progressBarFactoryMock
    .Setup(
      m => m.Create(
        It.IsAny<int>(),
        It.IsAny<string>()
      )
    )
    .Returns(
      new ProgressBar(1, "test", new ProgressBarOptions())
    );

    _attachmentsProcessor = new AttachmentsProcessor(
      _globalOptionsMock.Object,
      _onspringServiceMock.Object,
      _reportServiceMock.Object,
      _loggerMock.Object,
      _loggingLevelSwitch,
      _progressBarFactoryMock.Object
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.NoFileFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileFields_WhenCalledAndNoFileFieldsFound_ItShouldReturnAnEmptyList(
    List<Field> fields
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      fields
    );

    var result = await _attachmentsProcessor.GetFileFields(
      It.IsAny<int>(),
      It.IsAny<List<int>>()
    );

    result.Should().BeEmpty();

    _onspringServiceMock.Verify(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.HasImageField),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileFields_WhenCalledAndImageFieldIsFound_ItShouldReturnAListOfFileFields(
    List<Field> fields
  )
  {
    var imageField = fields.First(f => f.Type == FieldType.Image);

    _onspringServiceMock
    .Setup(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      fields
    );

    var result = await _attachmentsProcessor.GetFileFields(
      It.IsAny<int>(),
      It.IsAny<List<int>>()
    );

    result.Should().HaveCount(1);
    result.First().Id.Should().Be(imageField.Id);
    result.First().Name.Should().Be(imageField.Name);
    result.First().Type.Should().Be(imageField.Type);

    _onspringServiceMock.Verify(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.HasAttachmentField),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileFields_WhenCalledAndAttachmentFieldIsFound_ItShouldReturnAListOfFileFields(
    List<Field> fields
  )
  {
    var attachmentField = fields.First(f => f.Type == FieldType.Attachment);

    _onspringServiceMock
    .Setup(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      fields
    );

    var result = await _attachmentsProcessor.GetFileFields(
      It.IsAny<int>(),
      It.IsAny<List<int>>()
    );

    result.Should().HaveCount(1);
    result.First().Id.Should().Be(attachmentField.Id);
    result.First().Name.Should().Be(attachmentField.Name);
    result.First().Type.Should().Be(attachmentField.Type);

    _onspringServiceMock.Verify(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.HasImageAndAttachmentField),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileFields_WhenCalledAndBothImageAndAttachmentFieldsAreFound_ItShouldReturnAListOfFileFields(
    List<Field> fields
  )
  {
    var attachmentField = fields.First(f => f.Type == FieldType.Attachment);
    var imageField = fields.First(f => f.Type == FieldType.Image);

    _onspringServiceMock
    .Setup(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      fields
    );

    var result = await _attachmentsProcessor.GetFileFields(
      It.IsAny<int>(),
      It.IsAny<List<int>>()
    );

    result.Should().HaveCount(2);
    var resultImageField = result.First(f => f.Type == FieldType.Image);
    var resultAttachmentField = result.First(f => f.Type == FieldType.Attachment);

    resultAttachmentField.Id.Should().Be(attachmentField.Id);
    resultAttachmentField.Name.Should().Be(attachmentField.Name);
    resultAttachmentField.Type.Should().Be(attachmentField.Type);
    resultImageField.Id.Should().Be(imageField.Id);
    resultImageField.Name.Should().Be(imageField.Name);
    resultImageField.Type.Should().Be(imageField.Type);

    _onspringServiceMock.Verify(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.HasImageAndAttachmentField),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileFields_WhenCalledAndFileFilterValuesAreGiven_ItShouldReturnAListOfFileFieldsOnlyWithIdsThatMatchTheFilterValues(
    List<Field> fields
  )
  {
    var attachmentField = fields.First(f => f.Type == FieldType.Attachment);
    var imageField = fields.First(f => f.Type == FieldType.Image);

    _onspringServiceMock
    .Setup(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      fields
    );

    var result = await _attachmentsProcessor.GetFileFields(
      It.IsAny<int>(),
      new List<int> { attachmentField.Id }
    );

    result.Should().HaveCount(1);
    var resultAttachmentField = result.First(f => f.Type == FieldType.Attachment);

    resultAttachmentField.Id.Should().Be(attachmentField.Id);
    resultAttachmentField.Name.Should().Be(attachmentField.Name);
    resultAttachmentField.Type.Should().Be(attachmentField.Type);

    _onspringServiceMock.Verify(
      m => m.GetAllFields(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.FileFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileRequests_WhenCalledAndNoRecordsAreFound_ItShouldReturnAnEmptyList(
    List<Field> fileFields
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      (GetPagedRecordsResponse?)null
    );

    var result = await _attachmentsProcessor.GetFileRequests(
      It.IsAny<int>(),
      fileFields,
      It.IsAny<List<int>>(),
      It.IsAny<List<int>>()
    );

    result.Should().NotBeNull();
    result.Should().BeEmpty();

    _onspringServiceMock.Verify(m =>
      m.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.FileFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileRequests_WhenCalledAndRecordsAreFound_ItShouldReturnAListOfFileRequests(
    List<Field> fileFields
  )
  {
    var attachmentField = fileFields.First(f => f.Type == FieldType.Attachment);
    var imageField = fileFields.First(f => f.Type == FieldType.Image);

    var records = RecordDataFactory.GetPageOfFileFieldValues(
      attachmentField,
      imageField
    );

    var res = new GetPagedRecordsResponse
    {
      PageNumber = 1,
      TotalPages = 1,
      TotalRecords = 1,
      Items = records,
    };

    _onspringServiceMock
    .Setup(
      m => m.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      res
    );

    var result = await _attachmentsProcessor.GetFileRequests(
      It.IsAny<int>(),
      fileFields,
      It.IsAny<List<int>>(),
      It.IsAny<List<int>>()
    );

    result.Should().NotBeNull();
    result.Should().HaveCount(8);
    result.Should().BeOfType<List<OnspringFileRequest>>();

    result
    .Select(f => f.RecordId)
    .Distinct()
    .Should().HaveCount(2);

    result
    .Select(f => f.RecordId)
    .Distinct()
    .ToList().Should().BeEquivalentTo(new List<int> { 1, 2 });

    result
    .Select(f => f.FieldId)
    .Distinct().Should().HaveCount(2);

    result
    .Select(f => f.FieldId)
    .Distinct()
    .ToList().Should().BeEquivalentTo(new List<int> { attachmentField.Id, imageField.Id });

    result
    .Select(f => f.FileId)
    .ToList().Should().BeEquivalentTo(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 });

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.FileFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetFileRequests_WhenCalledAndRecordFilterIsGiven_ItShouldReturnAListOfFileRequestsThatOnlyIncludeRecordIdsInTheFilter(
  List<Field> fileFields
  )
  {
    var attachmentField = fileFields.First(f => f.Type == FieldType.Attachment);
    var imageField = fileFields.First(f => f.Type == FieldType.Image);

    var records = RecordDataFactory.GetPageOfFileFieldValues(
      attachmentField,
      imageField
    );

    var res = new GetPagedRecordsResponse
    {
      PageNumber = 1,
      TotalPages = 1,
      TotalRecords = 1,
      Items = records,
    };

    _onspringServiceMock
    .Setup(
      m => m.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      res
    );

    var result = await _attachmentsProcessor.GetFileRequests(
      It.IsAny<int>(),
      fileFields,
      It.IsAny<List<int>>(),
      new List<int> { 1 }
    );

    result.Should().NotBeNull();
    result.Should().HaveCount(4);
    result.Should().BeOfType<List<OnspringFileRequest>>();

    result
    .Select(f => f.RecordId)
    .Distinct()
    .Should().HaveCount(1);

    result
    .Select(f => f.RecordId)
    .Distinct()
    .ToList().First().Should().Be(1);

    result
    .Select(f => f.FieldId)
    .Distinct().Should().HaveCount(2);

    result
    .Select(f => f.FieldId)
    .Distinct()
    .ToList().Should().BeEquivalentTo(new List<int> { attachmentField.Id, imageField.Id });

    result
    .Select(f => f.FileId)
    .ToList().Should().BeEquivalentTo(new List<int> { 1, 2, 3, 4 });

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecords(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FileDataFactory.FileRequests),
    MemberType = typeof(FileDataFactory)
  )]
  public async Task GetFileInfos_WhenCalledAndNoFilesInfoIsFound_ItShouldReturnListOfFileInfosWithErrorMessages(
    List<OnspringFileRequest> fileRequests
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      (GetFileResponse?)null
    );

    var result = await _attachmentsProcessor.GetFileInfos(
      fileRequests
    );

    result.Should().NotBeNull();
    result.Should().HaveCount(4);

    foreach (var fileInfo in result)
    {
      fileInfo.Should().NotBeNull();
      fileInfo.Should().BeOfType<OnspringFileInfoResult>();
      fileInfo.FileName.Should().Be("Error: Unable to get file info");
      fileInfo.FileSizeInBytes.Should().Be(0);
      fileInfo.FileSizeInKB.Should().Be(0);
      fileInfo.FileSizeInKiB.Should().Be(0);
      fileInfo.FileSizeInMB.Should().Be(0);
      fileInfo.FileSizeInMiB.Should().Be(0);
      fileInfo.FileSizeInGB.Should().Be(0);
      fileInfo.FileSizeInGiB.Should().Be(0);
    }

    _onspringServiceMock.Verify(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Exactly(4)
    );
  }

  [Theory]
  [MemberData(
    nameof(FileDataFactory.FileRequestsWithResponses),
    MemberType = typeof(FileDataFactory)
  )]
  public async Task GetFileInfos_WhenCalledAndFilesInfoIsFound_ItShouldReturnAListOfFileInfos(
    List<OnspringFileRequest> fileRequests,
    List<GetFileResponse> getFileResponses
  )
  {
    _onspringServiceMock
    .SetupSequence(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(getFileResponses[0])
    .ReturnsAsync(getFileResponses[1])
    .ReturnsAsync(getFileResponses[2])
    .ReturnsAsync(getFileResponses[3]);

    var result = await _attachmentsProcessor.GetFileInfos(fileRequests);

    result.Should().HaveCount(4);
    result.Should().BeOfType<List<OnspringFileInfoResult>>();

    foreach (var fileInfo in result)
    {
      fileInfo.Should().NotBeNull();
      fileInfo.Should().BeOfType<OnspringFileInfoResult>();
      fileInfo.FileName.Should().Be("test1");
      fileInfo.FileSizeInBytes.Should().Be(100);
    }

    _onspringServiceMock.Verify(m =>
      m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Exactly(4)
    );
  }

  [Fact]
  public void WriteFileInfoReport_WhenCalled_ItShouldWriteFileInfoReport()
  {
    _attachmentsProcessor.WriteFileInfoReport(
      It.IsAny<List<OnspringFileInfoResult>>(),
      It.IsAny<string>()
    );

    _reportServiceMock.Verify(
      m => m.WriteCsvReport(
        It.IsAny<List<OnspringFileInfoResult>>(),
        typeof(OnspringFileInfoResultMap),
        It.IsAny<string>(),
        It.IsAny<string>()
      ),
      Times.Once
    );
  }

  [Fact]
  public void WriteFileRequestErrorReport_WhenCalled_ItShouldWriteErrorReport()
  {
    _attachmentsProcessor.WriteFileRequestErrorReport(
      It.IsAny<List<OnspringFileRequest>>(),
      It.IsAny<string>()
    );

    _reportServiceMock.Verify(
      m => m.WriteCsvReport(
        It.IsAny<List<OnspringFileRequest>>(),
        typeof(OnspringFileRequestMap),
        It.IsAny<string>(),
        It.IsAny<string>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task GetRecordIdsFromReport_WhenCalledAndNoReportIsFound_ItShouldReturnAnEmptyList()
  {
    _onspringServiceMock
    .Setup(
      m => m.GetReport(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      (ReportData?)null
    );

    var result = await _attachmentsProcessor.GetRecordIdsFromReport(
      It.IsAny<int>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<int>>();
    result.Should().BeEmpty();

    _onspringServiceMock.Verify(
      m => m.GetReport(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(ReportDataFactory.GetReportData),
    MemberType = typeof(ReportDataFactory)
  )]
  public async Task GetRecordIdsFromReport_WhenCalledAndReportIsFound_ItShouldReturnListOfRecordIds(
    ReportData reportData
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetReport(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      reportData
    );

    var result = await _attachmentsProcessor.GetRecordIdsFromReport(
      It.IsAny<int>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<int>>();
    result.Should().BeEquivalentTo(
      reportData.Rows.Select(r => r.RecordId)
    );

    _onspringServiceMock.Verify(
      m => m.GetReport(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }
}
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
      (GetPagedRecordsResponse?) null
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
      (GetFileResponse?) null
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
      (ReportData?) null
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

  [Theory]
  [MemberData(
    nameof(FileDataFactory.GetFileResponse),
    MemberType = typeof(FileDataFactory)
  )]
  public async Task TryDownloadFiles_WhenCalledAndNoErrorsOccur_ItShouldReturnAnEmptyList(
    GetFileResponse getFileResponse
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
      getFileResponse
    );

    var fileRequest = new OnspringFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      FieldName = "test1",
      FileId = 1,
    };

    var result = await _attachmentsProcessor.TryDownloadFiles(
      new List<OnspringFileRequest> { fileRequest },
      "output"
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<OnspringFileRequest>>();
    result.Should().BeEmpty();

    _onspringServiceMock.Verify(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task TryDownloadFiles_WhenCalledAndNoFileFound_ItShouldReturnAListOfErroredRequests()
  {
    _onspringServiceMock
    .Setup(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      (GetFileResponse?) null
    );

    var fileRequest = new OnspringFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      FieldName = "test1",
      FileId = 1,
    };

    var result = await _attachmentsProcessor.TryDownloadFiles(
      new List<OnspringFileRequest> { fileRequest },
      "output"
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<OnspringFileRequest>>();
    result.Should().BeEquivalentTo(
      new List<OnspringFileRequest> { fileRequest }
    );

    _onspringServiceMock.Verify(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task TryDownloadFiles_WhenCalledAndFileCanNotBeSaved_ItShouldReturnAListOfErroredRequests()
  {
    var fileRequest = new OnspringFileRequest(1, 1, "test", 1);
    var fileRequests = new List<OnspringFileRequest> { fileRequest };
    var stream = new MemoryStream();
    var getFileResponse = new GetFileResponse
    {
      Stream = stream,
      FileName = "test",
      ContentType = "test",
      ContentLength = 1,
    };

    _onspringServiceMock
    .Setup(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      getFileResponse
    );

    stream.Dispose();

    var result = await _attachmentsProcessor.TryDownloadFiles(
      fileRequests,
      "output"
    );

    result.Should().NotBeNull();
    result.Should().NotBeEmpty();
    result.Should().BeOfType<List<OnspringFileRequest>>();
    result.Should().BeEquivalentTo(
      fileRequests
    );
  }

  [Fact]
  public async Task TryDeleteFiles_WhenCalledAndNoErrorsOccur_ItShouldReturnAnEmptyList()
  {
    _onspringServiceMock
    .Setup(
      m => m.TryDeleteFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      true
    );

    var fileRequest = new OnspringFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      FieldName = "test1",
      FileId = 1,
    };

    var result = await _attachmentsProcessor.TryDeleteFiles(
      new List<OnspringFileRequest> { fileRequest }
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<OnspringFileRequest>>();
    result.Should().BeEmpty();

    _onspringServiceMock.Verify(
      m => m.TryDeleteFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task TryDeleteFiles_WhenCalledAndNoFileFound_ItShouldReturnAListOfErroredRequests()
  {
    _onspringServiceMock
    .Setup(
      m => m.TryDeleteFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      false
    );

    var fileRequest = new OnspringFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      FieldName = "test1",
      FileId = 1,
    };

    var result = await _attachmentsProcessor.TryDeleteFiles(
      new List<OnspringFileRequest> { fileRequest }
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<OnspringFileRequest>>();
    result.Should().BeEquivalentTo(
      new List<OnspringFileRequest> { fileRequest }
    );

    _onspringServiceMock.Verify(
      m => m.TryDeleteFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.InvalidMatchFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task ValidateMatchFields_WhenCalledAndMatchFieldsAreInvalid_ItShouldReturnFalse(
    Field? sourceMatchField,
    Field? targetMatchField
  )
  {
    _onspringServiceMock
    .SetupSequence(
      m => m.GetField(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      sourceMatchField
    )
    .ReturnsAsync(
      targetMatchField
    );

    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      TargetMatchFieldId = 1,
    };

    var result = await _attachmentsProcessor.ValidateMatchFields(
      settings
    );

    result.Should().BeFalse();

    _onspringServiceMock.Verify(
      m => m.GetField(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Exactly(2)
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.ValidMatchFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task ValidateMatchFields_WhenCalledAndMatchFieldsAreValid_ItShouldReturnTrue(
    Field? sourceMatchField,
    Field? targetMatchField
  )
  {
    _onspringServiceMock
    .SetupSequence(
      m => m.GetField(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      sourceMatchField
    )
    .ReturnsAsync(
      targetMatchField
    );

    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      TargetMatchFieldId = 1,
    };

    var result = await _attachmentsProcessor.ValidateMatchFields(
      settings
    );

    result.Should().BeTrue();

    _onspringServiceMock.Verify(
      m => m.GetField(
        It.IsAny<string>(),
        It.IsAny<int>()
      ),
      Times.Exactly(2)
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.InvalidFlagFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task ValidateFlagFieldIdAndValues_WhenCalledAndFlagFieldIsInvalid_ItShouldReturnFalse(
    AttachmentTransferSettings settings,
    Field? flagField
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetField(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      flagField
    );

    var result = await _attachmentsProcessor.ValidateFlagFieldIdAndValues(
      settings
    );

    result.Should().BeFalse();
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.ValidFlagFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task ValidateFlagFieldIdAndValues_WhenCalledAndFlagFieldIsValid_ItShouldReturnTrue(
    AttachmentTransferSettings settings,
    Field? flagField
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetField(
        It.IsAny<string>(),
        It.IsAny<int>()
      )
    )
    .ReturnsAsync(
      flagField
    );

    var result = await _attachmentsProcessor.ValidateFlagFieldIdAndValues(
      settings
    );

    result.Should().BeTrue();
  }

  [Fact]
  public async Task GetSourceRecordsToProcess_WhenCalledAndNoRecordsCanBeRetrieved_ItShouldReturnAnEmptyList()
  {
    _onspringServiceMock
    .Setup(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      (GetPagedRecordsResponse?) null
    );

    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      ProcessFlagFieldId = 1,
    };

    var result = await _attachmentsProcessor.GetSourceRecordsToProcess(
      settings
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<ResultRecord>>();
    result.Should().BeEmpty();

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(RecordDataFactory.GetOnePageOfRecords),
    MemberType = typeof(RecordDataFactory)
  )]
  public async Task GetSourceRecordsToProcess_WhenCalledAndRecordsAreFound_ItShouldReturnAListOfRecords(
    GetPagedRecordsResponse response
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      response
    );

    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      ProcessFlagFieldId = 1,
    };

    var result = await _attachmentsProcessor.GetSourceRecordsToProcess(
      settings
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<ResultRecord>>();
    result.Should().BeEquivalentTo(
      response.Items
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(RecordDataFactory.GetOnePageOfRecords),
    MemberType = typeof(RecordDataFactory)
  )]
  public async Task GetSourceRecordsToProcess_WhenCalledAndRecordFilterIsGiven_ItShouldReturnAListOfRecordsThatMatchTheFilter(
    GetPagedRecordsResponse response
  )
  {
    _onspringServiceMock
    .Setup(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      response
    );

    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      ProcessFlagFieldId = 1,
    };

    var result = await _attachmentsProcessor.GetSourceRecordsToProcess(
      settings,
      new List<int> { 1 }
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<List<ResultRecord>>();
    result.Should().BeEquivalentTo(
      new List<ResultRecord> { response.Items[0] }
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task TransferAttachments_WhenCalledAndNoRecordsAreGiven_ItShouldNotTransferAttachments()
  {
    await _attachmentsProcessor.TransferAttachments(
      new AttachmentTransferSettings(),
      new List<ResultRecord>()
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Never
    );
  }

  [Fact]
  public async Task TransferAttachments_WhenCalledAndRecordsAreGiven_ItShouldTransferAttachments()
  {
    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      TargetMatchFieldId = 2,
      ProcessFlagFieldId = 3,
      ProcessFlagValue = "Test",
      ProcessFlagListValueId = Guid.NewGuid(),
      ProcessedFlagValue = "Tested",
      ProcessedFlagListValueId = Guid.NewGuid(),
    };

    _onspringServiceMock.
    Setup(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      (GetPagedRecordsResponse?) null
    );

    await _attachmentsProcessor.TransferAttachments(
      settings,
      new List<ResultRecord>
      {
        new ResultRecord
        {
          AppId = 1,
          RecordId = 1,
          FieldData = new List<RecordFieldValue>
          {
            new StringFieldValue(
              settings.SourceMatchFieldId,
              "Test"
            ),
          },
        },
      }
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task TransferRecordAttachments_WhenCalledAndMatchValueCanNotBeFoundInSourceRecord_ItShouldReturnEarly()
  {
    await _attachmentsProcessor.TransferRecordAttachments(
      RecordDataFactory.AttachmentTransferSettings,
      new ResultRecord()
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.GetFileInfo(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.UpdateRecord(
        It.IsAny<string>(),
        It.IsAny<ResultRecord>()
      ),
      Times.Never
    );
  }

  [Theory]
  [MemberData(
    nameof(RecordDataFactory.GetUnTransferrableSourceRecords),
    MemberType = typeof(RecordDataFactory)
  )]
  public async Task TransferRecordAttachments_WhenCalledAndSourceRecordCanNotBeTransferred_ItShouldReturnEarly(
    AttachmentTransferSettings settings,
    ResultRecord sourceRecord,
    GetPagedRecordsResponse response
  )
  {
    _onspringServiceMock.
    Setup(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      response
    );

    await _attachmentsProcessor.TransferRecordAttachments(
      settings,
      sourceRecord
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );

    _onspringServiceMock.Verify(
      m => m.GetFileInfo(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.UpdateRecord(
        It.IsAny<string>(),
        It.IsAny<ResultRecord>()
      ),
      Times.Never
    );
  }

  [Fact]
  public async Task TransferRecordAttachments_WhenCalledAndTargetRecordIsFound_ItShouldTransferAttachmentAndUpdateRecord()
  {
    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      TargetMatchFieldId = 2,
      ProcessFlagFieldId = 3,
      ProcessFlagValue = "Test",
      ProcessFlagListValueId = Guid.NewGuid(),
      ProcessedFlagValue = "Tested",
      ProcessedFlagListValueId = Guid.NewGuid(),
      AttachmentFieldMappings = new Dictionary<string, int>
      {
        { "4", 4 },
      }
    };

    var sourceRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>
      {
        new StringFieldValue(
          settings.SourceMatchFieldId,
          "Test"
        ),
        new AttachmentListFieldValue(
          4,
          new List<AttachmentFile>
          {
            new AttachmentFile
            {
              FileId = 4,
              FileName = "Test",
              Notes = "Test",
              StorageLocation = FileStorageSite.Internal,
            },
          }
        ),
      },
    };

    var targetRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 2,
      FieldData = new List<RecordFieldValue>
      {
        new StringFieldValue(
          settings.TargetMatchFieldId,
          "Test"
        ),
      },
    };

    _onspringServiceMock.
    Setup(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      new GetPagedRecordsResponse
      {
        Items = new List<ResultRecord> { targetRecord },
      }
    );

    _onspringServiceMock
    .Setup(
      m => m.UpdateRecord(
        It.IsAny<string>(),
        It.IsAny<ResultRecord>()
      )
    )
    .ReturnsAsync(
      new CreatedWithIdResponse<int>
      {
        Id = 1,
      }
    );

    await _attachmentsProcessor.TransferRecordAttachments(
      settings,
      sourceRecord
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );

    _onspringServiceMock.Verify(
      m => m.UpdateRecord(
        It.IsAny<string>(),
        It.IsAny<ResultRecord>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task TransferRecordAttachment_WhenCalledAndTargetRecordIsFoundButCannotUpdateSourceRecord_ItShouldLogAWarning()
  {
    var settings = new AttachmentTransferSettings
    {
      SourceMatchFieldId = 1,
      TargetMatchFieldId = 2,
      ProcessFlagFieldId = 3,
      ProcessFlagValue = "Test",
      ProcessFlagListValueId = Guid.NewGuid(),
      ProcessedFlagValue = "Tested",
      ProcessedFlagListValueId = Guid.NewGuid(),
      AttachmentFieldMappings = new Dictionary<string, int>
      {
        { "4", 4 },
      }
    };

    var sourceRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>
      {
        new StringFieldValue(
          settings.SourceMatchFieldId,
          "Test"
        ),
        new AttachmentListFieldValue(
          4,
          new List<AttachmentFile>
          {
            new AttachmentFile
            {
              FileId = 4,
              FileName = "Test",
              Notes = "Test",
              StorageLocation = FileStorageSite.Internal,
            },
          }
        ),
      },
    };

    var targetRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 2,
      FieldData = new List<RecordFieldValue>
      {
        new StringFieldValue(
          settings.TargetMatchFieldId,
          "Test"
        ),
      },
    };

    _onspringServiceMock.
    Setup(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      new GetPagedRecordsResponse
      {
        Items = new List<ResultRecord> { targetRecord },
      }
    );

    _onspringServiceMock
    .Setup(
      m => m.UpdateRecord(
        It.IsAny<string>(),
        It.IsAny<ResultRecord>()
      )
    )
    .ReturnsAsync(
      (CreatedWithIdResponse<int>?) null
    );

    await _attachmentsProcessor.TransferRecordAttachments(
      settings,
      sourceRecord
    );

    _onspringServiceMock.Verify(
      m => m.GetAPageOfRecordsByQuery(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<List<int>>(),
        It.IsAny<string>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );

    _onspringServiceMock.Verify(
      m => m.UpdateRecord(
        It.IsAny<string>(),
        It.IsAny<ResultRecord>()
      ),
      Times.Once
    );

    _loggerMock.Verify(
      m => m.Warning(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FileDataFactory.InvalidOnspringFileResults),
    MemberType = typeof(FileDataFactory)
  )]
  public async Task TrySveFile_WhenCalledAndOnspringFileResultCanNotBeSaved_ItShouldReturnFalse(
    OnspringFileResult fileResult
  )
  {
    var result = await _attachmentsProcessor.TrySaveFile(
      fileResult
    );

    result.Should().BeFalse();
  }

  [Fact]
  public async Task TrySveFile_WhenCalledAndOnspringFileResultCanBeSaved_ItShouldReturnTrue()
  {
    var fileResult = new OnspringFileResult(
      1,
      1,
      "Test",
      1,
      "Test",
      "TestData/Test",
      new MemoryStream()
    );

    var result = await _attachmentsProcessor.TrySaveFile(
      fileResult
    );

    result.Should().BeTrue();
  }

  [Fact]
  public async Task TrySaveFile_WhenCalledAndAnExceptionIsThrown_ItShouldReturnFalse()
  {
    var stream = new MemoryStream();

    var fileResult = new OnspringFileResult(
      1,
      1,
      "Test",
      1,
      "Test",
      "TestData/Test",
      stream
    );

    stream.Dispose();

    var result = await _attachmentsProcessor.TrySaveFile(
      fileResult
    );

    result.Should().BeFalse();
  }

  [Fact]
  public async Task TransferAttachmentsForField_WhenCalledAndAttachmentFieldValueCanNotBeFound_ItShouldReturnEarly()
  {
    var sourceRecord = new ResultRecord();

    await _attachmentsProcessor.TransferAttachmentsForField(
      sourceRecord,
      1,
      1,
      1
    );

    _onspringServiceMock.Verify(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.GetFileInfo(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      ),
      Times.Never
    );
  }

  [Fact]
  public async Task TransferAttachment_WhenCalledAndSourceFileInfoCanNotBeFound_ItShouldReturnEarly()
  {
    var sourceRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>(),
    };

    _onspringServiceMock
    .Setup(
      m => m.GetFileInfo(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      (GetFileInfoResponse?) null
    );

    await _attachmentsProcessor.TransferAttachment(
      sourceRecord,
      1,
      1,
      1,
      1
    );

    _onspringServiceMock.Verify(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      ),
      Times.Never
    );

    _onspringServiceMock.Verify(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      ),
      Times.Never
    );
  }

  [Fact]
  public async Task TransferAttachment_WhenCalledAndSourceFileCanNotBeFound_ItShouldReturnEarly()
  {
    var sourceRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>(),
    };

    _onspringServiceMock
    .Setup(
      m => m.GetFileInfo(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      new GetFileInfoResponse()
    );

    _onspringServiceMock
    .Setup(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      (GetFileResponse?) null
    );

    await _attachmentsProcessor.TransferAttachment(
      sourceRecord,
      1,
      1,
      1,
      1
    );

    _onspringServiceMock.Verify(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      ),
      Times.Never
    );
  }

  [Theory]
  [InlineData("Test")]
  [InlineData(null)]
  public async Task TransferAttachment_WhenCalledAndSourceFileAndFileInfoCanBeFound_ItShouldSaveTheFile(
    string? notes
  )
  {
    var sourceRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>(),
    };

    _onspringServiceMock
    .Setup(
      m => m.GetFileInfo(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      new GetFileInfoResponse
      {
        Name = "Test",
        CreatedDate = DateTime.Now,
        ModifiedDate = DateTime.Now,
        Owner = "Test",
        Notes = notes,
        FileHref = "TestData/Test",
      }
    );

    _onspringServiceMock
    .Setup(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      new GetFileResponse
      {
        Stream = new MemoryStream(),
        FileName = "Test",
        ContentType = "Test",
        ContentLength = 1,
      }
    );

    _onspringServiceMock
    .Setup(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      )
    )
    .ReturnsAsync(
      new CreatedWithIdResponse<int>
      {
        Id = 1,
      }
    );

    await _attachmentsProcessor.TransferAttachment(
      sourceRecord,
      1,
      1,
      1,
      1
    );

    _onspringServiceMock.Verify(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task TransferAttachment_WhenCalledAndSourceFileAndFileInfoCanBeFoundButFileCanNotBeSaved_ItShouldLogAWarning()
  {
    var sourceRecord = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>(),
    };

    _onspringServiceMock
    .Setup(
      m => m.GetFileInfo(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      new GetFileInfoResponse
      {
        Name = "Test",
        CreatedDate = DateTime.Now,
        ModifiedDate = DateTime.Now,
        Owner = "Test",
        Notes = "Test",
        FileHref = "TestData/Test",
      }
    );

    _onspringServiceMock
    .Setup(
      m => m.GetFile(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>()
      )
    )
    .ReturnsAsync(
      new GetFileResponse
      {
        Stream = new MemoryStream(),
        FileName = "Test",
        ContentType = "Test",
        ContentLength = 1,
      }
    );

    _onspringServiceMock
    .Setup(
      m => m.SaveFile(
        It.IsAny<string>(),
        It.IsAny<SaveFileRequest>()
      )
    )
    .ReturnsAsync(
      (CreatedWithIdResponse<int>?) null
    );

    await _attachmentsProcessor.TransferAttachment(
      sourceRecord,
      1,
      1,
      1,
      1
    );

    _loggerMock.Verify(
      m => m.Warning(
        It.IsAny<string>(),
        It.IsAny<OnspringFileRequest>(),
        It.IsAny<SaveFileRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(RecordDataFactory.GetRecordWithFileToBeRequested),
    MemberType = typeof(RecordDataFactory)
  )]
  public void GetFileRequestsFromRecord_WhenCalled_ItShouldReturnAListOfFileRequests(
    ResultRecord record,
    List<Field> fields,
    List<int> filesFilter
  )
  {
    var fileRequests = AttachmentsProcessor.GetFileRequestsFromRecord(
      record,
      fields,
      filesFilter
    );

    fileRequests.Should().NotBeNull();
    fileRequests.Should().NotBeEmpty();
    fileRequests.Should().BeOfType<List<OnspringFileRequest>>();
    fileRequests.Should().HaveCount(3);
    fileRequests.Select(r => r.FileId).Should().BeEquivalentTo(
      new List<int> { 1, 3, 5 }
    );
  }

  [Fact]
  public void IsAllAttachmentsField_WhenCalledAndOneAttachmentFieldAndTheAllAttachmentFieldAreReturned_ItShouldReturnFalse()
  {
    var record = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = new List<RecordFieldValue>
      {
        new AttachmentListFieldValue
        {
          FieldId = 1,
          Value = new List<AttachmentFile>
          {
            new AttachmentFile
            {
              FileId = 2,
            },
          },
        },
        new AttachmentListFieldValue
        {
          FieldId = 2,
          Value = new List<AttachmentFile>
          {
            new AttachmentFile
            {
              FileId = 2,
            },
          },
        },
      },
    };

    var fields = new List<Field>
    {
      new Field
      {
        Id = 1,
        Name = "Attachment",
        Type = FieldType.Attachment,
      },
      new Field
      {
        Id = 2,
        Name = "All Attachments",
        Type = FieldType.Attachment,
      },
    };

    var attachmentFieldValue = new List<AttachmentFile>
    {
      new AttachmentFile
      {
        FileId = 2,
      },
    };

    var result = AttachmentsProcessor.IsAllAttachmentsField(
      record,
      fields,
      attachmentFieldValue
    );

    result.Should().BeFalse();
  }
}
using Xunit.Sdk;

namespace OnspringCLI.Tests.UnitTests.Services;

public class OnspringServiceTests
{
  private readonly Mock<ILogger> _loggerMock;
  private readonly Mock<IOnspringClient> _mockClient;
  private readonly Mock<IOnspringClientFactory> _clientFactoryMock;
  private readonly OnspringService _onspringService;
  public OnspringServiceTests()
  {
    _loggerMock = new Mock<ILogger>();
    _clientFactoryMock = new Mock<IOnspringClientFactory>();
    _mockClient = new Mock<IOnspringClient>();

    _loggerMock
      .Setup(
        x => x.ForContext<It.IsAnyType>()
      )
      .Returns(
        _loggerMock.Object
      );

    _clientFactoryMock
      .Setup(
        m => m.Create(
          It.IsAny<string>()
        )
      )
      .Returns(
        _mockClient.Object
      );

    _onspringService = new OnspringService(
      _loggerMock.Object,
      _clientFactoryMock.Object
    );
  }

  [Fact]
  public async Task GetAllFields_WhenCalledAndNoFieldsAreFound_ItShouldReturnAnEmptyList()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      new GetPagedFieldsResponse
      {
        Items = []
      }
    );

    _mockClient
      .Setup(
        m => m.GetFieldsForAppAsync(
          It.IsAny<int>(),
          It.IsAny<PagingRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAllFields(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeEmpty();

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(nameof(FieldDataFactory.GetOnePageOfFields), MemberType = typeof(FieldDataFactory))]
  public async Task GetAllFields_WhenCalledAndOnePageOfFieldsAreFound_ItShouldReturnAListOfFields(
    GetPagedFieldsResponse pagedFieldsResponse
  )
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      pagedFieldsResponse
    );

    _mockClient
      .Setup(
        m => m.GetFieldsForAppAsync(
          It.IsAny<int>(),
          It.IsAny<PagingRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAllFields(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().HaveCount(pagedFieldsResponse.Items.Count);
    result.Should().BeOfType<List<Field>>();
    result.Should().BeEquivalentTo(pagedFieldsResponse.Items);

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(nameof(FieldDataFactory.GetTwoPagesOfFields), MemberType = typeof(FieldDataFactory))]
  public async Task GetAllFields_WhenCalledAndMultiplePagesOfFieldsAreFound_ItShouldReturnAListOfFields(
    GetPagedFieldsResponse pageTwo,
    GetPagedFieldsResponse pageOne
  )
  {
    var pageOneRes = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      pageOne
    );

    var pageTwoRes = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      pageTwo
    );

    _mockClient
      .SetupSequence(
        m => m.GetFieldsForAppAsync(
          It.IsAny<int>(),
          It.IsAny<PagingRequest>()
        )
      )
      .ReturnsAsync(pageOneRes)
      .ReturnsAsync(pageTwoRes);

    var result = await _onspringService.GetAllFields(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().HaveCount(pageOne.Items.Count + pageTwo.Items.Count);
    result.Should().BeOfType<List<Field>>();
    result.Should().BeEquivalentTo(pageOne.Items.Concat(pageTwo.Items));

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(2)
    );
  }

  [Theory]
  [MemberData(nameof(FieldDataFactory.GetFirstPageOfFields), MemberType = typeof(FieldDataFactory))]
  public async Task GetAllFields_WhenCalledAndMultiplePagesOfFieldsAreFoundAndOnePageReturnsAnError_ItShouldReturnAListOfFieldsAfterRetryingFailedPageThreeTimes(
    GetPagedFieldsResponse pageOne
  )
  {
    var pageOneRes = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      pageOne
    );

    var pageTwoRes = ApiResponseFactory.GetApiResponse<GetPagedFieldsResponse>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .SetupSequence(
        m => m.GetFieldsForAppAsync(
          It.IsAny<int>(),
          It.IsAny<PagingRequest>()
        )
      )
      .ReturnsAsync(pageOneRes)
      .ReturnsAsync(pageTwoRes)
      .ReturnsAsync(pageTwoRes)
      .ReturnsAsync(pageTwoRes);

    var result = await _onspringService.GetAllFields(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().HaveCount(pageOne.Items.Count);
    result.Should().BeOfType<List<Field>>();
    result.Should().BeEquivalentTo(pageOne.Items);

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(4)
    );
  }

  [Fact]
  public async Task GetAllFields_WhenCalledAndExceptionIsThrown_ItShouldReturnAnEmptyList()
  {
    _mockClient
      .Setup(
        m => m.GetFieldsForAppAsync(
          It.IsAny<int>(),
          It.IsAny<PagingRequest>()
        )
      )
      .Throws(
        new Exception()
      );

    var result = await _onspringService.GetAllFields(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeEmpty();
    result.Should().BeOfType<List<Field>>();

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetAllFields_WhenCalledAndHttpRequestExceptionOrTaskCanceledExceptionIsThrown_ItShouldReturnAnEmptyListAfterRetryingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.GetFieldsForAppAsync(
          It.IsAny<int>(),
          It.IsAny<PagingRequest>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new TaskCanceledException());

    var result = await _onspringService.GetAllFields(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeEmpty();
    result.Should().BeOfType<List<Field>>();

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(RecordDataFactory.GetOnePageOfRecords), MemberType = typeof(RecordDataFactory))]
  public async Task GetAPageOfRecords_WhenCalledAndOnePageOfRecordsAreFound_ItShouldAPagedResponseWithAListOfRecords(
    GetPagedRecordsResponse recordsResponse
  )
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      recordsResponse
    );

    _mockClient
      .Setup(
        m => m.GetRecordsForAppAsync(
          It.IsAny<GetRecordsByAppRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAPageOfRecords(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<GetPagedRecordsResponse>();

    if (result is null)
    {
      return;
    }

    result.TotalPages.Should().Be(recordsResponse.TotalPages);
    result.TotalRecords.Should().Be(recordsResponse.TotalRecords);
    result.PageNumber.Should().Be(recordsResponse.PageNumber);
    result.Items.Should().HaveCount(recordsResponse.Items.Count);
    result.Items.Should().BeOfType<List<ResultRecord>>();
    result.Items.Should().BeEquivalentTo(recordsResponse.Items);

    _mockClient.Verify(
      m => m.GetRecordsForAppAsync(
        It.IsAny<GetRecordsByAppRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetAPageOfRecords_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<GetPagedRecordsResponse>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .SetupSequence(
        m => m.GetRecordsForAppAsync(
          It.IsAny<GetRecordsByAppRequest>()
        )
      )
      .ReturnsAsync(apiResponse)
      .ReturnsAsync(apiResponse)
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAPageOfRecords(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetRecordsForAppAsync(
        It.IsAny<GetRecordsByAppRequest>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(RecordDataFactory.GetEmptyPageOfRecords), MemberType = typeof(RecordDataFactory))]
  public async Task GetAPageOfRecords_WhenCalledAndNoRecordsAreFound_ItShouldReturnAPagedResponseWithAnEmptyListOfRecords(
    GetPagedRecordsResponse recordsResponse
  )
  {
    var totalPages = 1;
    var totalRecords = 0;
    var pageNumber = 1;

    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      recordsResponse
    );

    _mockClient
      .Setup(
        m => m.GetRecordsForAppAsync(
          It.IsAny<GetRecordsByAppRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAPageOfRecords(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<GetPagedRecordsResponse>();

    if (result is null)
    {
      return;
    }

    result.TotalPages.Should().Be(totalPages);
    result.TotalRecords.Should().Be(totalRecords);
    result.PageNumber.Should().Be(pageNumber);
    result.Items.Should().BeEmpty();
    result.Items.Should().BeOfType<List<ResultRecord>>();

    _mockClient.Verify(
      m => m.GetRecordsForAppAsync(
        It.IsAny<GetRecordsByAppRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetAPageOfRecords_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(
        m => m.GetRecordsForAppAsync(
          It.IsAny<GetRecordsByAppRequest>()
        )
      )
      .Throws(
        new Exception()
      );

    var result = await _onspringService.GetAPageOfRecords(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetRecordsForAppAsync(
        It.IsAny<GetRecordsByAppRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetAPageOfRecords_WhenCalledAndHttpRequestOrTaskExceptionIsThrown_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.GetRecordsForAppAsync(
          It.IsAny<GetRecordsByAppRequest>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new HttpRequestException());

    var result = await _onspringService.GetAPageOfRecords(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetRecordsForAppAsync(
        It.IsAny<GetRecordsByAppRequest>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(FileDataFactory.GetFileResponse), MemberType = typeof(FileDataFactory))]
  public async Task GetFile_WhenCalledAndFileIsFound_ItShouldReturnAFile(
    GetFileResponse fileResponse
  )
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      fileResponse
    );

    _mockClient
      .Setup(
        m => m.GetFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(apiResponse);

    var fileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFile(
      It.IsAny<string>(),
      fileRequest
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<GetFileResponse>();

    if (result is null)
    {
      return;
    }

    result.FileName.Should().Be(fileResponse.FileName);
    result.ContentLength.Should().Be(fileResponse.ContentLength);
    result.ContentType.Should().Be(fileResponse.ContentType);
    result.Stream.Should().NotBeNull();
  }

  [Fact]
  public async Task GetFile_WhenCalledAndFileIsNotFound_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<GetFileResponse>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .Setup(
        m => m.GetFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(apiResponse);

    var fileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFile(
      It.IsAny<string>(),
      fileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFileAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task GetFile_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(
        m => m.GetFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .Throws(new Exception());

    var fileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFile(
      It.IsAny<string>(),
      fileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFileAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetFile_WhenCalledAndHttpRequestOrTaskCanceledExceptionIsThrown_ItShouldReturnNullAfterAttemptingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.GetFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new TaskCanceledException());

    var fileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFile(
      It.IsAny<string>(),
      fileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFileAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(FieldDataFactory.GetField), MemberType = typeof(FieldDataFactory))]
  public async Task GetField_WhenCalledAndFieldIsFound_ItShouldReturnAField(
    Field field
  )
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      field
    );

    _mockClient
      .Setup(m => m.GetFieldAsync(It.IsAny<int>()))
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetField(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<Field>();

    if (result is null)
    {
      return;
    }

    result.Id.Should().Be(field.Id);
    result.Name.Should().Be(field.Name);
    result.Type.Should().Be(field.Type);

    _mockClient.Verify(
      m => m.GetFieldAsync(
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetField_WhenCalledAndFieldIsNotFound_ItShouldReturnNull()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<Field>(
      HttpStatusCode.NotFound,
      "Not Found"
    );

    _mockClient
      .Setup(m => m.GetFieldAsync(It.IsAny<int>()))
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetField(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFieldAsync(
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetField_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(m => m.GetFieldAsync(It.IsAny<int>()))
      .Throws(new Exception());

    var result = await _onspringService.GetField(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFieldAsync(
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetField_WhenCalledAndHttpRequestOrTaskCanceledExceptionIsThrown_ItShouldReturnNullAfterAttemptingThreeTimes()
  {
    _mockClient
      .SetupSequence(m => m.GetFieldAsync(It.IsAny<int>()))
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new TaskCanceledException());

    var result = await _onspringService.GetField(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFieldAsync(
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task GetField_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnNullAfterAttemptingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<Field>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .Setup(m => m.GetFieldAsync(It.IsAny<int>()))
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetField(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFieldAsync(
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(FileDataFactory.GetFileInfoResponse), MemberType = typeof(FileDataFactory))]
  public async Task GetFileInfo_WhenCalledAndFileInfoIsFound_ItShouldReturnAFileInfo(
    GetFileInfoResponse fileInfo
  )
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      fileInfo
    );

    _mockClient
      .Setup(
        m => m.GetFileInfoAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(apiResponse);

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFileInfo(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<GetFileInfoResponse>();
    result.Should().BeEquivalentTo(fileInfo);

    _mockClient.Verify(
      m => m.GetFileInfoAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetFileInfo_WhenCalledAndFileInfoIsNotFound_ItShouldReturnNull()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<GetFileInfoResponse>(
      HttpStatusCode.NotFound,
      "Not Found"
    );

    _mockClient
      .Setup(
        m => m.GetFileInfoAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(apiResponse);

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFileInfo(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFileInfoAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetFileInfo_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(
        m => m.GetFileInfoAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .Throws(new Exception());

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFileInfo(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFileInfoAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetFileInfo_WhenCalledAndHttpRequestOrTaskCanceledExceptionIsThrown_ItShouldReturnNullAfterAttemptingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.GetFileInfoAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new TaskCanceledException());

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFileInfo(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFileInfoAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task GetFileInfo_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnNullAfterAttemptingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<GetFileInfoResponse>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .Setup(
        m => m.GetFileInfoAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(apiResponse);

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.GetFileInfo(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetFileInfoAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(
  nameof(RecordDataFactory.GetOnePageOfRecords),
  MemberType = typeof(RecordDataFactory)
)]
  public async Task GetAPageOfRecordsByQuery_WhenCalledAndOnePageOfRecordsAreFound_ItShouldAPagedResponseWithAListOfRecords(
  GetPagedRecordsResponse recordsResponse
)
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      recordsResponse
    );

    _mockClient
      .Setup(
        m => m.QueryRecordsAsync(
          It.IsAny<QueryRecordsRequest>(),
          It.IsAny<PagingRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAPageOfRecordsByQuery(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<string>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<GetPagedRecordsResponse>();

    if (result is null)
    {
      return;
    }

    result.TotalPages.Should().Be(recordsResponse.TotalPages);
    result.TotalRecords.Should().Be(recordsResponse.TotalRecords);
    result.PageNumber.Should().Be(recordsResponse.PageNumber);
    result.Items.Should().HaveCount(recordsResponse.Items.Count);
    result.Items.Should().BeOfType<List<ResultRecord>>();
    result.Items.Should().BeEquivalentTo(recordsResponse.Items);

    _mockClient.Verify(
      m => m.QueryRecordsAsync(
        It.IsAny<QueryRecordsRequest>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetAPageOfRecordsByQuery_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<GetPagedRecordsResponse>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .SetupSequence(
        m => m.QueryRecordsAsync(
          It.IsAny<QueryRecordsRequest>(),
          It.IsAny<PagingRequest>()
        )
      )
      .ReturnsAsync(apiResponse)
      .ReturnsAsync(apiResponse)
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAPageOfRecordsByQuery(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<string>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.QueryRecordsAsync(
        It.IsAny<QueryRecordsRequest>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(RecordDataFactory.GetEmptyPageOfRecords), MemberType = typeof(RecordDataFactory))]
  public async Task GetAPageOfRecordsByQuery_WhenCalledAndNoRecordsAreFound_ItShouldReturnAPagedResponseWithAnEmptyListOfRecords(
    GetPagedRecordsResponse recordsResponse
  )
  {
    var totalPages = 1;
    var totalRecords = 0;
    var pageNumber = 1;

    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      recordsResponse
    );

    _mockClient
      .Setup(
        m => m.QueryRecordsAsync(
          It.IsAny<QueryRecordsRequest>(),
          It.IsAny<PagingRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetAPageOfRecordsByQuery(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<string>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<GetPagedRecordsResponse>();

    if (result is null)
    {
      return;
    }

    result.TotalPages.Should().Be(totalPages);
    result.TotalRecords.Should().Be(totalRecords);
    result.PageNumber.Should().Be(pageNumber);
    result.Items.Should().BeEmpty();
    result.Items.Should().BeOfType<List<ResultRecord>>();

    _mockClient.Verify(
      m => m.QueryRecordsAsync(
        It.IsAny<QueryRecordsRequest>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetAPageOfRecordsByQuery_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(
        m => m.QueryRecordsAsync(
          It.IsAny<QueryRecordsRequest>(),
          It.IsAny<PagingRequest>()
        )
      )
      .Throws(new Exception());

    var result = await _onspringService.GetAPageOfRecordsByQuery(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<string>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.QueryRecordsAsync(
        It.IsAny<QueryRecordsRequest>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetAPageOfRecordsByQuery_WhenCalledAndHttpRequestOrTaskExceptionIsThrown_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.QueryRecordsAsync(
          It.IsAny<QueryRecordsRequest>(),
          It.IsAny<PagingRequest>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new HttpRequestException());

    var result = await _onspringService.GetAPageOfRecordsByQuery(
      It.IsAny<string>(),
      It.IsAny<int>(),
      It.IsAny<List<int>>(),
      It.IsAny<string>(),
      It.IsAny<PagingRequest>()
    );

    result.Should().BeNull();
    _mockClient.Verify(
      m => m.QueryRecordsAsync(
        It.IsAny<QueryRecordsRequest>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(ReportDataFactory.GetReportData), MemberType = typeof(ReportDataFactory))]
  public async Task GetReport_WhenCalledAndReportIsFound_ItShouldReturnReportData(ReportData reportData)
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      reportData
    );

    _mockClient
      .Setup(
        m => m.GetReportAsync(
          It.IsAny<int>(),
          It.IsAny<ReportDataType>(),
          It.IsAny<DataFormat>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetReport(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<ReportData>();
    result.Should().BeEquivalentTo(reportData);

    _mockClient.Verify(
      m => m.GetReportAsync(
        It.IsAny<int>(),
        It.IsAny<ReportDataType>(),
        It.IsAny<DataFormat>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetReport_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<ReportData>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .Setup(
        m => m.GetReportAsync(
          It.IsAny<int>(),
          It.IsAny<ReportDataType>(),
          It.IsAny<DataFormat>()
        )
      )
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetReport(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetReportAsync(
        It.IsAny<int>(),
        It.IsAny<ReportDataType>(),
        It.IsAny<DataFormat>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task GetReport_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(
        m => m.GetReportAsync(
          It.IsAny<int>(),
          It.IsAny<ReportDataType>(),
          It.IsAny<DataFormat>()
        )
      )
      .Throws(new Exception());

    var result = await _onspringService.GetReport(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.GetReportAsync(
        It.IsAny<int>(),
        It.IsAny<ReportDataType>(),
        It.IsAny<DataFormat>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task GetReport_WhenCalledAndHttpRequestOrTaskExceptionIsThrown_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.GetReportAsync(
          It.IsAny<int>(),
          It.IsAny<ReportDataType>(),
          It.IsAny<DataFormat>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new HttpRequestException());

    var result = await _onspringService.GetReport(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().BeNull();
    _mockClient.Verify(
      m => m.GetReportAsync(
        It.IsAny<int>(),
        It.IsAny<ReportDataType>(),
        It.IsAny<DataFormat>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(FileDataFactory.GetCreatedWithIdResponse), MemberType = typeof(FileDataFactory))]
  public async Task SaveFile_WhenCalledAndFileIsSaved_ItShouldReturnACreatedWithIdResponse(CreatedWithIdResponse<int> createdWithIdResponse)
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "OK",
      createdWithIdResponse
    );

    _mockClient
      .Setup(
        m => m.SaveFileAsync(
          It.IsAny<SaveFileRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var saveFileRequest = new SaveFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      Notes = "Notes",
      FileName = "FileName",
      ModifiedDate = DateTime.Now,
      ContentType = "ContentType",
      FileStream = new MemoryStream()
    };

    var result = await _onspringService.SaveFile(
      It.IsAny<string>(),
      saveFileRequest
    );

    result.Should().NotBeNull();
    result.Should().BeOfType<CreatedWithIdResponse<int>>();
    result.Should().BeEquivalentTo(createdWithIdResponse);

    _mockClient.Verify(
      m => m.SaveFileAsync(
        It.IsAny<SaveFileRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task SaveFile_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<CreatedWithIdResponse<int>>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .Setup(
        m => m.SaveFileAsync(
          It.IsAny<SaveFileRequest>()
        )
      )
      .ReturnsAsync(apiResponse);

    var saveFileRequest = new SaveFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      Notes = "Notes",
      FileName = "FileName",
      ModifiedDate = DateTime.Now,
      ContentType = "ContentType",
      FileStream = new MemoryStream()
    };

    var result = await _onspringService.SaveFile(
      It.IsAny<string>(),
      saveFileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.SaveFileAsync(
        It.IsAny<SaveFileRequest>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task SaveFile_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(
        m => m.SaveFileAsync(
          It.IsAny<SaveFileRequest>()
        )
      )
      .Throws(new Exception());

    var saveFileRequest = new SaveFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      Notes = "Notes",
      FileName = "FileName",
      ModifiedDate = DateTime.Now,
      ContentType = "ContentType",
      FileStream = new MemoryStream()
    };

    var result = await _onspringService.SaveFile(
      It.IsAny<string>(),
      saveFileRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.SaveFileAsync(
        It.IsAny<SaveFileRequest>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task SaveFile_WhenCalledAndHttpRequestOrTaskExceptionIsThrown_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.SaveFileAsync(
          It.IsAny<SaveFileRequest>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new HttpRequestException());

    var saveFileRequest = new SaveFileRequest
    {
      RecordId = 1,
      FieldId = 1,
      Notes = "Notes",
      FileName = "FileName",
      ModifiedDate = DateTime.Now,
      ContentType = "ContentType",
      FileStream = new MemoryStream()
    };

    var result = await _onspringService.SaveFile(
      It.IsAny<string>(),
      saveFileRequest
    );

    result.Should().BeNull();
    _mockClient.Verify(
      m => m.SaveFileAsync(
        It.IsAny<SaveFileRequest>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task TryDeleteFile_WhenCalledAndFileIsDeleted_ItShouldReturnTrue()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.NoContent,
      "No Content"
    );

    _mockClient
      .Setup(
        m => m.DeleteFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(apiResponse);

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.TryDeleteFile(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeTrue();

    _mockClient.Verify(
      m => m.DeleteFileAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task TryDeleteFile_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnFalseAfterRetryingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .Setup(
        m => m.DeleteFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .ReturnsAsync(apiResponse);

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.TryDeleteFile(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeFalse();

    _mockClient.Verify(
      m => m.DeleteFileAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task TryDeleteFile_WhenCalledAndExceptionIsThrown_ItShouldReturnFalse()
  {
    _mockClient
      .Setup(
        m => m.DeleteFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .Throws(new Exception());

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.TryDeleteFile(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeFalse();

    _mockClient.Verify(
      m => m.DeleteFileAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task TryDeleteFile_WhenCalledAndHttpRequestOrTaskExceptionIsThrown_ItShouldReturnFalseAfterRetryingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.DeleteFileAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new HttpRequestException());

    var onspringFileRequest = new OnspringFileRequest(
      1,
      1,
      1
    );

    var result = await _onspringService.TryDeleteFile(
      It.IsAny<string>(),
      onspringFileRequest
    );

    result.Should().BeFalse();

    _mockClient.Verify(
      m => m.DeleteFileAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<int>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(RecordDataFactory.GetSaveRecordResponse), MemberType = typeof(RecordDataFactory))]
  public async Task UpdateRecord_WhenCalledAndRecordIsUpdated_ItShouldReturnACreatedWithIdResponse(SaveRecordResponse saveRecordResponse)
  {
    var apiResponse = ApiResponseFactory.GetApiResponse(
      HttpStatusCode.OK,
      "Ok",
      saveRecordResponse
    );

    _mockClient
      .Setup(
        m => m.SaveRecordAsync(
          It.IsAny<ResultRecord>()
        )
      )
      .ReturnsAsync(apiResponse);

    var updateRecordRequest = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = []
    };

    var result = await _onspringService.UpdateRecord(
      It.IsAny<string>(),
      updateRecordRequest
    );

    result.Should().BeEquivalentTo(saveRecordResponse);

    _mockClient.Verify(
      m => m.SaveRecordAsync(
        It.IsAny<ResultRecord>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task UpdateRecord_WhenCalledAndRequestIsUnsuccessful_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<SaveRecordResponse>(
      HttpStatusCode.InternalServerError,
      "Internal Server Error"
    );

    _mockClient
      .Setup(
        m => m.SaveRecordAsync(
          It.IsAny<ResultRecord>()
        )
      )
      .ReturnsAsync(apiResponse);

    var updateRecordRequest = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = []
    };

    var result = await _onspringService.UpdateRecord(
      It.IsAny<string>(),
      updateRecordRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.SaveRecordAsync(
        It.IsAny<ResultRecord>()
      ),
      Times.Exactly(3)
    );
  }

  [Fact]
  public async Task UpdateRecord_WhenCalledAndExceptionIsThrown_ItShouldReturnNull()
  {
    _mockClient
      .Setup(
        m => m.SaveRecordAsync(
          It.IsAny<ResultRecord>()
        )
      )
      .Throws(new Exception());

    var updateRecordRequest = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = []
    };

    var result = await _onspringService.UpdateRecord(
      It.IsAny<string>(),
      updateRecordRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.SaveRecordAsync(
        It.IsAny<ResultRecord>()
      ),
      Times.Exactly(1)
    );
  }

  [Fact]
  public async Task UpdateRecord_WhenCalledAndHttpRequestOrTaskExceptionIsThrown_ItShouldReturnNullAfterRetryingThreeTimes()
  {
    _mockClient
      .SetupSequence(
        m => m.SaveRecordAsync(
          It.IsAny<ResultRecord>()
        )
      )
      .Throws(new HttpRequestException())
      .Throws(new TaskCanceledException())
      .Throws(new HttpRequestException());

    var updateRecordRequest = new ResultRecord
    {
      AppId = 1,
      RecordId = 1,
      FieldData = []
    };

    var result = await _onspringService.UpdateRecord(
      It.IsAny<string>(),
      updateRecordRequest
    );

    result.Should().BeNull();

    _mockClient.Verify(
      m => m.SaveRecordAsync(
        It.IsAny<ResultRecord>()
      ),
      Times.Exactly(3)
    );
  }

  [Theory]
  [MemberData(nameof(ApiResponseFactory.GetResponsesThatNeedToBeRetried), MemberType = typeof(ApiResponseFactory))]
  public void NeedsToBeRetried_WhenCalledAndStatusCodeCanBeRetried_ItShouldReturnTrue(ApiResponse response)
  {
    var result = OnspringService.NeedsToBeRetried(response);
    result.Should().BeTrue();
  }

  [Theory]
  [MemberData(nameof(ApiResponseFactory.GetResponsesThatShouldNotBeRetried), MemberType = typeof(ApiResponseFactory))]
  public void NeedsToBeRetried_WhenCalledAndStatusCodeCannotBeRetried_ItShouldReturnFalse(ApiResponse response)
  {
    var result = OnspringService.NeedsToBeRetried(response);
    result.Should().BeFalse();
  }

  [Theory]
  [InlineData(HttpStatusCode.InternalServerError)]
  [InlineData(HttpStatusCode.BadGateway)]
  [InlineData(HttpStatusCode.ServiceUnavailable)]
  [InlineData(HttpStatusCode.GatewayTimeout)]
  [InlineData(HttpStatusCode.RequestTimeout)]
  [InlineData(HttpStatusCode.TooManyRequests)]
  public void CanBeRetried_WhenCalledAndStatusCodeCanBeRetried_ItShouldReturnTrue(HttpStatusCode statusCode)
  {
    var result = OnspringService.CanBeRetried(statusCode);
    result.Should().BeTrue();
  }

  [Theory]
  [InlineData(HttpStatusCode.NotFound)]
  [InlineData(HttpStatusCode.BadRequest)]
  [InlineData(HttpStatusCode.Unauthorized)]
  [InlineData(HttpStatusCode.Forbidden)]
  public void CanBeRetried_WhenCalledAndStatusCodeCannotBeRetried_ItShouldReturnFalse(HttpStatusCode statusCode)
  {
    var result = OnspringService.CanBeRetried(statusCode);
    result.Should().BeFalse();
  }

  [Fact]
  public async Task GetApps_WhenCalledAndInitialRequestFails_ItShouldReturnEmptyList()
  {
    var apiResponse = ApiResponseFactory.GetApiResponse<GetPagedAppsResponse>(HttpStatusCode.BadRequest, "Bad Request");

    _mockClient
      .Setup(m => m.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ReturnsAsync(apiResponse);

    var result = await _onspringService.GetApps(It.IsAny<string>());

    result.Should().BeEmpty();
  }

  [Fact]
  public async Task GetApps_WhenCalledAndInitialRequestAndRemainingRequestSucceeds_ItShouldReturnListOfApps()
  {
    var apps = new List<App>()
    {
      new(),
    };

    var firstPage = new ApiResponse<GetPagedAppsResponse>()
    {
      StatusCode = HttpStatusCode.OK,
      Value = new()
      {
        PageNumber = 1,
        TotalPages = 2,
        TotalRecords = 2,
        Items = apps
      }
    };

    var secondPage = new ApiResponse<GetPagedAppsResponse>()
    {
      StatusCode = HttpStatusCode.OK,
      Value = new()
      {
        PageNumber = 2,
        TotalPages = 2,
        TotalRecords = 2,
        Items = apps
      }
    };

    _mockClient
      .SetupSequence(m => m.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ReturnsAsync(firstPage)
      .ReturnsAsync(secondPage);

    var result = await _onspringService.GetApps(It.IsAny<string>());

    result.Should().HaveCount(2);

    _mockClient.Verify(m => m.GetAppsAsync(
      It.IsAny<PagingRequest>()), 
      Times.Exactly(2)
    );
  }

  [Fact]
  public async Task GetApps_WhenCalledAndInitialRequestSucceedsAndRemainingRequestFails_ItShouldReturnListOfApps()
  {
    var apps = new List<App>()
    {
      new(),
    };

    var firstPage = new ApiResponse<GetPagedAppsResponse>()
    {
      StatusCode = HttpStatusCode.OK,
      Value = new()
      {
        PageNumber = 1,
        TotalPages = 2,
        TotalRecords = 2,
        Items = apps
      }
    };

    var secondPage = ApiResponseFactory.GetApiResponse<GetPagedAppsResponse>(HttpStatusCode.BadRequest, "Bad Request");

    _mockClient
      .SetupSequence(m => m.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ReturnsAsync(firstPage)
      .ReturnsAsync(secondPage!);

    var result = await _onspringService.GetApps(It.IsAny<string>());

    result.Should().HaveCount(1);

    _mockClient.Verify(m => m.GetAppsAsync(
      It.IsAny<PagingRequest>()), 
      Times.Exactly(2)
    );
  }

  [Fact]
  public async Task GetApps_WhenCalledAndExceptionIsThrown_ItShouldReturnEmptyList()
  {
    _mockClient
      .Setup(m => m.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ThrowsAsync(new Exception());

    var result = await _onspringService.GetApps(It.IsAny<string>());

    result.Should().BeEmpty();
  }
}
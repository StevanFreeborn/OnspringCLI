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
    var apiResponse = FieldDataFactory.GetSuccessfulPagedFieldResponse(
      new List<Field>(),
      0,
      0,
      1
    );

    _mockClient
    .Setup(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      apiResponse
    );

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
  [MemberData(
    nameof(FieldDataFactory.GetOnePageOfFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetAllFields_WhenCalledAndOnePageOfFieldsAreFound_ItShouldReturnAListOfFields(
    List<Field> fields
  )
  {
    var apiResponse = FieldDataFactory.GetSuccessfulPagedFieldResponse(
      fields,
      1,
      3,
      1
    );

    _mockClient
    .Setup(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      )
    )
    .ReturnsAsync(
      apiResponse
    );

    var result = await _onspringService.GetAllFields(
      It.IsAny<string>(),
      It.IsAny<int>()
    );

    result.Should().HaveCount(fields.Count);
    result.Should().BeOfType<List<Field>>();
    result.Should().BeEquivalentTo(fields);

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Once
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.GetTwoPagesOfFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetAllFields_WhenCalledAndMultiplePagesOfFieldsAreFound_ItShouldReturnAListOfFields(
    List<Field> fieldsPageOne,
    List<Field> fieldsPageTwo
  )
  {
    var pageOneRes = FieldDataFactory.GetSuccessfulPagedFieldResponse(
      fieldsPageOne,
      2,
      6,
      1
    );

    var pageTwoRes = FieldDataFactory.GetSuccessfulPagedFieldResponse(
      fieldsPageTwo,
      2,
      6,
      2
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

    result.Should().HaveCount(
      fieldsPageOne.Count + fieldsPageTwo.Count
    );
    result.Should().BeOfType<List<Field>>();
    result.Should().BeEquivalentTo(
      fieldsPageOne.Concat(fieldsPageTwo)
    );

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(
        It.IsAny<int>(),
        It.IsAny<PagingRequest>()
      ),
      Times.Exactly(2)
    );
  }

  [Theory]
  [MemberData(
    nameof(FieldDataFactory.GetOnePageOfFields),
    MemberType = typeof(FieldDataFactory)
  )]
  public async Task GetAllFields_WhenCalledAndMultiplePagesOfFieldsAreFoundAndOnePageReturnsAnError_ItShouldReturnAListOfFieldsAfterRetryingFailedPageThreeTimes(
    List<Field> fieldsPageOne
  )
  {
    var pageOneRes = FieldDataFactory.GetSuccessfulPagedFieldResponse(
      fieldsPageOne,
      2,
      6,
      1
    );

    var pageTwoRes = FieldDataFactory.GetFailedPagedFieldResponse(
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

    result.Should().HaveCount(fieldsPageOne.Count);
    result.Should().BeOfType<List<Field>>();
    result.Should().BeEquivalentTo(fieldsPageOne);

    _mockClient.Verify(
      m => m.GetFieldsForAppAsync(It.IsAny<int>(), It.IsAny<PagingRequest>()),
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
  [MemberData(
    nameof(RecordDataFactory.GetOnePageOfRecords),
    MemberType = typeof(RecordDataFactory)
  )]
  public async Task GetAPageOfRecords_WhenCalledAndOnePageOfRecordsAreFound_ItShouldAPagedResponseWithAListOfRecords(
    List<ResultRecord> records
  )
  {
    var totalPages = 1;
    var totalRecords = 2;
    var pageNumber = 1;

    var apiResponse = RecordDataFactory.GetSuccessfulPagedRecordResponse(
      records,
      totalPages,
      totalRecords,
      pageNumber
    );

    _mockClient
    .Setup(
      m => m.GetRecordsForAppAsync(
        It.IsAny<GetRecordsByAppRequest>()
      )
    )
    .ReturnsAsync(
      apiResponse
    );

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
    result.Items.Should().HaveCount(records.Count);
    result.Items.Should().BeOfType<List<ResultRecord>>();
    result.Items.Should().BeEquivalentTo(records);

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
    var apiResponse = RecordDataFactory.GetFailedPagedRecordResponse(
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

  [Fact]
  public async Task GetAPageOfRecords_WhenCalledAndNoRecordsAreFound_ItShouldReturnAPagedResponseWithAnEmptyListOfRecords()
  {
    var totalPages = 1;
    var totalRecords = 0;
    var pageNumber = 1;

    var apiResponse = RecordDataFactory.GetSuccessfulPagedRecordResponse(
      new List<ResultRecord>(),
      totalPages,
      totalRecords,
      pageNumber
    );

    _mockClient
    .Setup(
      m => m.GetRecordsForAppAsync(
        It.IsAny<GetRecordsByAppRequest>()
      )
    )
    .ReturnsAsync(
      apiResponse
    );

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
}
namespace OnspringCLI.Tests.TestData;

public class ApiResponseFactory
{
  public static ApiResponse<T?> GetApiResponse<T>(
    HttpStatusCode statusCode,
    string message,
    T? value = default
  )
  {
    return new ApiResponse<T?>
    {
      StatusCode = statusCode,
      Message = message,
      Value = value
    };
  }

  public static ApiResponse GetApiResponse(
    HttpStatusCode statusCode,
    string message
  )
  {
    return new ApiResponse
    {
      StatusCode = statusCode,
      Message = message
    };
  }

  public static IEnumerable<object[]> GetResponsesThatNeedToBeRetried =>
  new List<object[]>
  {
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.InternalServerError,
        Message = "Internal Server Error"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.BadGateway,
        Message = "Bad Gateway"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.ServiceUnavailable,
        Message = "Service Unavailable"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.GatewayTimeout,
        Message = "Gateway Timeout"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.RequestTimeout,
        Message = "Request Timeout"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.TooManyRequests,
        Message = "Too Many Requests"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = (HttpStatusCode) 524,
        Message = "Timeout Occurred"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = (HttpStatusCode) 499,
        Message = "Client Closed Request"
      }
    },
  };

  public static IEnumerable<object[]> GetResponsesThatShouldNotBeRetried =>
  new List<object[]>
  {
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.OK,
        Message = "OK"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.Created,
        Message = "Created"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.NoContent,
        Message = "No Content"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.BadRequest,
        Message = "Bad Request"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.Unauthorized,
        Message = "Unauthorized"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.Forbidden,
        Message = "Forbidden"
      }
    },
    new object[]
    {
      new ApiResponse
      {
        StatusCode = HttpStatusCode.NotFound,
        Message = "Not Found"
      }
    },
  };
}
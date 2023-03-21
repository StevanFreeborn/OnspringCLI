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
}
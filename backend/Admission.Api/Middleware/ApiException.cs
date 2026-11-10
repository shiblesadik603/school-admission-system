namespace Admission.Api.Middleware;

// Thrown from services when a request can't be completed (no seats, expired, not found...).
public class ApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    public ApiException(int statusCode, string errorCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

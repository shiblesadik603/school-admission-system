using Admission.Api.Middleware;

namespace Admission.Api.Helpers;

public static class StudentIdHelper
{
    public const string HeaderName = "X-Student-Id";

    // Student identity = 17 digit birth registration number sent in the X-Student-Id header.
    public static string ReadFromHeader(HttpRequest request)
    {
        var value = request.Headers[HeaderName].ToString().Trim();

        if (value == "")
            throw new ApiException(400, "MISSING_STUDENT_ID", "Please enter the student's birth registration number.");

        if (value.Length != 17 || !value.All(char.IsAsciiDigit))
            throw new ApiException(400, "INVALID_STUDENT_ID", "Birth registration number must be exactly 17 digits.");

        return value;
    }
}

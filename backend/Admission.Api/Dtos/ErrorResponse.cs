namespace Admission.Api.Dtos;

// "error" is a fixed code the UI can check, "message" is shown to the user.
public record ErrorResponse(string Error, string Message);

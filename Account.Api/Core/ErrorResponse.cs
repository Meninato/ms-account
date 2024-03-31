namespace Account.Api.Core;

public class ErrorResponse
{
    private ErrorResponse() { }

    public required int Status { get; set; }
    public required string Message { get; set; }
    public List<InvalidModelError> Errors { get; set; } = new List<InvalidModelError>();

    public static ErrorResponse From(AppException e)
    {
        return new ErrorResponse()
        {
            Status = e.StatusCode,
            Message = e.Message,
            Errors = e.Errors
        };
    }

    public static ErrorResponse From(Exception e) => new ErrorResponse()
    {
        Status = StatusCodes.Status500InternalServerError,
        Message = "Internal server error"
    };
}
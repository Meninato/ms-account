namespace Account.Api.Core;

public class AppErrorResponse
{
    private AppErrorResponse() { }

    public required int Status { get; set; }
    public required string Message { get; set; }
    public List<InvalidModelError> Errors { get; set; } = new List<InvalidModelError>();

    public static AppErrorResponse From(AppException e)
    {
        return new AppErrorResponse()
        {
            Status = e.StatusCode,
            Message = e.Message,
            Errors = e.Errors
        };
    }

    public static AppErrorResponse From(Exception e) => new AppErrorResponse()
    {
        Status = StatusCodes.Status500InternalServerError,
        Message = "Something went wrong."
    };
}
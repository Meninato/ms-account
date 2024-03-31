namespace Account.Api.Core;

public class InvalidModelError
{
    public required string FieldName { get; set; }
    public required string Message { get; set; }
}
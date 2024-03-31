using System.Globalization;

namespace Account.Api.Core;

public class AppException : Exception
{
    public int StatusCode { get; private set; } = StatusCodes.Status400BadRequest;
    public List<InvalidModelError> Errors { get; private set; } = new List<InvalidModelError>();

    public AppException(IDictionary<string, string[]> validationErrors) : base("One or more validation errors occured.")
    {
        foreach (var error in validationErrors)
        {
            foreach (var subError in error.Value)
            {
                var errorModel = new InvalidModelError
                {
                    FieldName = error.Key,
                    Message = subError
                };

                Errors.Add(errorModel);
            }
        }
    }

    public AppException(string message) : base(message) { }

    public AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public AppException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}

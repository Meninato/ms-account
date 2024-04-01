namespace Account.Api.V1.Models.Requests;

public class AuthenticateRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
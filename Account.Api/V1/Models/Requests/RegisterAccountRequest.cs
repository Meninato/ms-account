namespace Account.Api.V1.Models.Requests;

public class RegisterAccountRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public List<string> Roles { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public bool? AcceptTerms { get; set; }
}

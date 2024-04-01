﻿using System.Text.Json.Serialization;

namespace Account.Api.V1.Models.Responses;

public class AuthenticateResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public List<string> Roles { get; set; } = default!;
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public bool IsVerified { get; set; }
    public string JwtToken { get; set; } = default!;

    [JsonIgnore] // refresh token is returned in http only cookie
    public string RefreshToken { get; set; } = default!;
}
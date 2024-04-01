using Account.Api.Core;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Account.Api.V1.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/accounts")]
public class AccountController : ControllerBase
{

}
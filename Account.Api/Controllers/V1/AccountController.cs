using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Account.Api.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/accounts")]
public class AccountController : ControllerBase
{

}
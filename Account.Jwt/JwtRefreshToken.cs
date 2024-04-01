using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Jwt;

public class JwtRefreshToken
{
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedByIp { get; set; }
}

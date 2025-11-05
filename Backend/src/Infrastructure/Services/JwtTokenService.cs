using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Industrial.AlertService.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Industrial.AlertService.Infrastructure.Services;

public class JwtOptions
{
    public string Issuer { get; set; } = "Industrial.AlertService";
    public string Audience { get; set; } = "Industrial.AlertService.Clients";
    public string Secret { get; set; } = "CHANGE_ME_SUPER_SECRET";
    public int ExpirationHours { get; set; } = 8;
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(string username, IEnumerable<KeyValuePair<string, string>>? claims = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.Secret);
        var claimsList = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };
        if (claims != null)
        {
            claimsList.AddRange(claims.Select(c => new Claim(c.Key, c.Value)));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claimsList),
            Expires = DateTime.UtcNow.AddHours(_options.ExpirationHours),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}



using System.Security.Claims;

namespace Server.Services;

public interface IAccessTokenService
{
    string Generate(IEnumerable<Claim> claims);
    
    ClaimsPrincipal GetPrincipalFromToken(string token);
}
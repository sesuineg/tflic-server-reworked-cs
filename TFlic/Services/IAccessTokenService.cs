using System.Security.Claims;

namespace TFlic.Services;

public interface IAccessTokenService
{
    string Generate(IEnumerable<Claim> claims);
    
    ClaimsPrincipal GetPrincipalFromToken(string token);
}
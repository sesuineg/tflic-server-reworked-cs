using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Server.Constants;
using Server.Options;

namespace Server.Services;

public class JwtService : IAccessTokenService
{
    public JwtService(IConfiguration appConfiguration)
    {
        _appConfiguration = appConfiguration;
        _jwtOptions = GetJwtOptionsFromAppConfiguration();
    }
    
    public string Generate(IEnumerable<Claim> claims)
    {
        var tokenExpires = DateTime.UtcNow.Add(_jwtOptions.TokenLifetime);
        var signingCredentials = ConstructSigningCredentials();

        var accessToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            claims: claims,
            expires: tokenExpires,
            signingCredentials: signingCredentials
        );

        var serializedAccessToken = SerializeToken();
        return serializedAccessToken;



        SigningCredentials ConstructSigningCredentials() =>
            new SigningCredentials(_jwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256);

        string SerializeToken() =>
            new JwtSecurityTokenHandler().WriteToken(accessToken);
    }

    public ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenValidationParameters = ConstructTokenValidationParameters();
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
        if (securityToken is null)
            throw new SecurityTokenException("Cannot get principal from token: Invalid JWT");

        return principal;



        TokenValidationParameters ConstructTokenValidationParameters() =>
            new TokenValidationParameters
            {
                ValidateIssuer = DefaultJwtOptions.ValidateIssuer,
                ValidIssuer = _jwtOptions.Issuer,
                IssuerSigningKey = _jwtOptions.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = DefaultJwtOptions.ValidateIssuerSigningKey,
                ValidateAudience = DefaultJwtOptions.ValidateAudience, 
                ValidateLifetime = false, // because we extract principal from any (expired and not) jwt
                ValidAlgorithms = _jwtOptions.ValidAlgorithms,
            };
    }
    
    private JwtOptions GetJwtOptionsFromAppConfiguration() =>
        JwtOptions.GetJwtOptionsFromAppConfiguration(_appConfiguration);
    
    private readonly IConfiguration _appConfiguration;
    private readonly JwtOptions _jwtOptions;
}
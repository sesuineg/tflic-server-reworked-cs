using System.Security.Cryptography;
using TFlic.Models.Domain.Authentication;
using TFlic.Models.Services.Contexts;
using TFlic.Options;

namespace TFlic.Services;

public class RefreshTokenService : IRefreshTokenService
{
    public RefreshTokenService(IConfiguration appConfiguration)
    {
        _appConfiguration = appConfiguration;
        _refreshTokenOptions = GetRefreshTokenOptionsFromAppConfiguration();
    }
    
    public (string token, DateTime expirationTime) Generate()
    {
        var refreshToken = new byte[32];
        Gen.GetBytes(refreshToken);

        var serializedToken = SerializeToken(refreshToken);
        var expirationTime = CalculateTokenExpirationTime();

        return (serializedToken, expirationTime);
        
        
        
        string SerializeToken(byte[] token) =>
            Convert.ToBase64String(refreshToken);
        
        DateTime CalculateTokenExpirationTime() =>
            DateTime.UtcNow.Add(_refreshTokenOptions.TokenLifetime);
    }

    public bool IsTokenValid(string token, ulong tokenOwnerId)
    {
        var authInfo = GetAuthInfo();
        return authInfo.RefreshToken == token && authInfo.RefreshTokenExpirationTime < DateTime.UtcNow;



        AuthInfo GetAuthInfo()
        {
            var authInfoContext = DbContexts.Get<AuthInfoContext>();
            var accountAuthInfo = authInfoContext.Info.First(info => info.AccountId == tokenOwnerId);

            return accountAuthInfo;
        }
    }

    private RefreshTokenOptions GetRefreshTokenOptionsFromAppConfiguration() =>
        RefreshTokenOptions.GetRefreshTokenOptionsFromAppConfiguration(_appConfiguration);
    
    private readonly IConfiguration _appConfiguration;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    
    private static readonly RandomNumberGenerator Gen = RandomNumberGenerator.Create();
}
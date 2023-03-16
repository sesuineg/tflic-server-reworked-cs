namespace Server.Services;

public interface IRefreshTokenService
{
    (string token, DateTime expirationTime) Generate();
    
    bool IsTokenValid(string token, ulong tokenOwnerId);
}
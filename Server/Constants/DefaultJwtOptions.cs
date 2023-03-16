namespace Server.Constants;

public static class DefaultJwtOptions
{
    public const bool ValidateIssuer = true;
    public const bool ValidateIssuerSigningKey = true; 
    public const bool ValidateAudience = false; // todo валидировать аудиенцию токена
    public const bool ValidateLifetime = true;
    public static readonly TimeSpan ClockSkew = TimeSpan.Zero;
}

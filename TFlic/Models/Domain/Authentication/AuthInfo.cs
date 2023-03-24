using TFlic.Models.Domain.Organization.Accounts;

namespace TFlic.Models.Domain.Authentication;

public class AuthInfo
{
    /// <summary>
    /// Уникальный идентификатор аккаунта, к которому относится информация
    /// </summary>
    public ulong AccountId { get; init; }
    
    /// <summary>
    /// Логин аккаунта
    /// </summary>
    public required string Login { get; init; }
    
    /// <summary>
    /// Хеш пароля
    /// </summary>
    /// <remarks>Хеш закодирован в base64</remarks>
    public required string PasswordHash { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpirationTime { get; set; } 

    public Account Account { get; set; } = null!;
}
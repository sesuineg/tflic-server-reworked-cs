using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFlic.Models.Domain.Organization.Accounts;

namespace TFlic.Models.Domain.Authentication;

[Table("auth_info")]
public class AuthInfo
{
    /// <summary>
    /// Уникальный идентификатор аккаунта, к которому относится информация
    /// </summary>
    [Key]
    [Column("account_id")]
    public ulong AccountId { get; set; }
    
    /// <summary>
    /// Логин аккаунта
    /// </summary>
    [Column("login"), MaxLength(50)]
    public required string Login { get; init; }
    
    /// <summary>
    /// Хеш пароля
    /// </summary>
    /// <remarks>Хеш закодирован в base64</remarks>
    [Column("password_hash"), MaxLength(44)]
    public required string PasswordHash { get; set; }
    
    [Column("refresh_token"), MaxLength(44)]
    public string? RefreshToken { get; set; }
    
    [Column("refresh_token_expiration_time")]
    public DateTime? RefreshTokenExpirationTime { get; set; } 

    public Account Account { get; set; } = null!;
}
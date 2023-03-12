using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Server.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "default_issuer";
    public string SecurityKey { get; set; } = "fake_password";
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromSeconds(3600);
    public string[] ValidAlgorithms { get; set; } = {"HS256"};

    public SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(SecurityKey));
}
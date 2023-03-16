namespace TFlic.Options;

public class RefreshTokenOptions
{
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromSeconds(604800); // 604800 secs = 7 days

    public static RefreshTokenOptions GetRefreshTokenOptionsFromAppConfiguration(IConfiguration config)
    {
        var options = config
            .GetSection(nameof(RefreshTokenOptions))
            .Get<RefreshTokenOptions>();
        
        if (options is null) 
            throw new ApplicationException("Refresh token is not configured for application");

        return options;
    }
}
using Server.Models.Constants;

namespace Server.Models.Config;

public class Configurator
{
    private readonly IConfigurationRoot _config;
    
    public Configurator()
    {
        var builder = new ConfigurationBuilder()
                .SetBasePath(FilesystemPaths.ConfigDir)
                .AddJsonFile(FilesystemPaths.ConfigName);
        _config = builder.Build(); 
    }
    
    public string? this[string key] => _config[key];
}
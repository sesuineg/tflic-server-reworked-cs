using TFlic.Models.Constants;

namespace TFlic.Models.Config;

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
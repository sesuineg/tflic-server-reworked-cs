namespace Server.Models.Config;

// todo избавиться от самопальной конфигурации
public static class ConfigurationUtils
{
    public static Configurator FromConfiguration { get; } = new();
}
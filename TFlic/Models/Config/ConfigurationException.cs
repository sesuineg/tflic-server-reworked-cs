﻿namespace TFlic.Models.Config;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
}
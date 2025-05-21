namespace ShredleApi.Helpers;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Default application port when no environment variables are set
    /// </summary>
    public const string DEFAULT_PORT = "5001";
    
    /// <summary>
    /// Environment variable name for port override
    /// </summary>
    public const string PORT_ENV_VAR = "PORT";
    
    /// <summary>
    /// Environment variable name for application port override (takes precedence over PORT)
    /// </summary>
    public const string APP_PORT_ENV_VAR = "APP_PORT";
}

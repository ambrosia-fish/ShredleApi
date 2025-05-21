namespace ShredleApi.Helpers;

/// <summary>
/// Helper class to detect and provide environment-specific settings
/// </summary>
public static class EnvironmentHelper
{
    // Flag to indicate if we're running on Heroku
    public static bool IsRunningOnHeroku => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DYNO"));
    
    // Flag to indicate if we're in development mode
    public static bool IsDevelopment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    
    // Environment name string for logging
    public static string EnvironmentName => IsRunningOnHeroku ? "Production (Heroku)" : 
                                           IsDevelopment ? "Development" : "Production";
    
    // Get port based on environment
    public static string GetPort()
    {
        // First check for manual port override
        var manualPort = Environment.GetEnvironmentVariable(AppConstants.APP_PORT_ENV_VAR);
        if (!string.IsNullOrEmpty(manualPort))
        {
            return manualPort;
        }
        
        // Check for environment-provided PORT variable
        var envPort = Environment.GetEnvironmentVariable(AppConstants.PORT_ENV_VAR);
        if (!string.IsNullOrEmpty(envPort))
        {
            return envPort;
        }
        
        // Default port from constants
        return AppConstants.DEFAULT_PORT;
    }

    // Helper to get configuration values with proper precedence:
    // 1. Environment variables (for Heroku)
    // 2. User secrets (automatically loaded into Configuration in development)
    // 3. appsettings.json
    public static string GetConfigValue(IConfiguration configuration, string envVarName, string configPath)
    {
        // First try environment variable (highest priority, for Heroku)
        var value = Environment.GetEnvironmentVariable(envVarName);
        
        // If not found, value will come from either user secrets or appsettings.json
        // (user secrets are automatically loaded with higher precedence than appsettings.json)
        if (string.IsNullOrEmpty(value))
        {
            value = configuration[configPath];
        }

        // For development, allow empty values (will use fallback implementations)
        if (string.IsNullOrEmpty(value) && IsDevelopment)
        {
            Console.WriteLine($"Development mode: No {envVarName}/{configPath} found, will use fallback implementation if available");
            return string.Empty;
        }

        // For production, log warning if key is missing
        if (string.IsNullOrEmpty(value) && !IsDevelopment)
        {
            Console.WriteLine($"WARNING: No {envVarName}/{configPath} found in Production environment!");
        }
        
        return value ?? string.Empty;
    }
}

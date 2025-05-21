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
        // On Heroku, we must use the dynamic PORT env var
        if (IsRunningOnHeroku)
        {
            return Environment.GetEnvironmentVariable("PORT") ?? "8080";
        }
        
        // Otherwise, use the standard port for local development
        return "8080";
    }

    // Helper to get API keys with environment-specific fallbacks
    public static string GetApiKey(IConfiguration configuration, string keyName, string configPath)
    {
        // First try environment variable
        var key = Environment.GetEnvironmentVariable(keyName);
        
        // If not found, try configuration
        if (string.IsNullOrEmpty(key))
        {
            key = configuration[configPath];
        }

        // For development, allow empty keys (will use fallback implementations)
        if (string.IsNullOrEmpty(key) && IsDevelopment)
        {
            return string.Empty;
        }

        // For production, log warning if key is missing
        if (string.IsNullOrEmpty(key) && !IsDevelopment)
        {
            Console.WriteLine($"WARNING: No {keyName} found in Production environment!");
        }
        
        return key ?? string.Empty;
    }
}

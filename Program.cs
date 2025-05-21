using ShredleApi.Services;
using ShredleApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Detect environment and configure accordingly
var isHeroku = EnvironmentHelper.IsRunningOnHeroku;
var isDevelopment = EnvironmentHelper.IsDevelopment;
var environmentName = EnvironmentHelper.EnvironmentName;

// Log the detected environment for debugging
Console.WriteLine($"Application starting in {environmentName} environment");

// Get port from helper (which checks APP_PORT and then uses 5000)
var port = EnvironmentHelper.GetPort();
Console.WriteLine($"Using PORT: {port}");

// Log any environment variables that might be interfering
var envPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(envPort))
{
    Console.WriteLine($"WARNING: Found PORT environment variable set to {envPort}");
}

// Configure Kestrel to listen on the correct port
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});

// Get configuration values with proper precedence:
// 1. Environment variables (for Heroku)
// 2. User secrets (automatically loaded in development)
// 3. appsettings.json
var supabaseUrl = EnvironmentHelper.GetConfigValue(
    builder.Configuration, 
    "SUPABASE_URL", 
    "Supabase:Url");

var supabaseKey = EnvironmentHelper.GetConfigValue(
    builder.Configuration, 
    "SUPABASE_KEY", 
    "Supabase:Key");

var openAiKey = EnvironmentHelper.GetConfigValue(
    builder.Configuration, 
    "OPENAI_API_KEY", 
    "OpenAI:ApiKey");

// Update configuration for services to use
if (!string.IsNullOrEmpty(supabaseUrl))
{
    builder.Configuration["Supabase:Url"] = supabaseUrl;
}
if (!string.IsNullOrEmpty(supabaseKey))
{
    builder.Configuration["Supabase:Key"] = supabaseKey;
}
if (!string.IsNullOrEmpty(openAiKey))
{
    builder.Configuration["OpenAI:ApiKey"] = openAiKey;
}

// Add development-specific services and configuration
if (isDevelopment)
{
    Console.WriteLine("Development mode: Using fallback implementations for missing API keys");
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Debug); // More verbose in development
}
else
{
    // Production settings
    Console.WriteLine("Production mode: API keys required for full functionality");
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// REMOVED: Entity Framework DbContext registration
// This simplifies our approach to use just Supabase REST API

// Use CORS configuration from appsettings.json
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] 
            {
                "http://localhost:5173",
                "https://shredle.feztech.io",
                "https://shredle-app.vercel.app"
            };
        
        Console.WriteLine($"CORS: Configuring allowed origins: {string.Join(", ", allowedOrigins)}");
        
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<SupabaseService>();
builder.Services.AddScoped<OpenAiService>();

var app = builder.Build();

// Log configuration information
Console.WriteLine($"Environment: {environmentName}");
Console.WriteLine($"Supabase URL: {builder.Configuration["Supabase:Url"]}");

// Don't log the full key, just the length and first few characters
var key = builder.Configuration["Supabase:Key"] ?? string.Empty;
Console.WriteLine($"Supabase key length: {key.Length}");
if (key.Length > 10)
{
    Console.WriteLine($"Supabase key starts with: {key.Substring(0, 10)}...");
}

// Enable Swagger in all environments for easier debugging
app.UseSwagger();
app.UseSwaggerUI();

// Add a simple health check endpoint at the root path
app.MapGet("/", () => 
{
    Console.WriteLine("Root endpoint accessed");
    return $"Shredle API is running in {environmentName} mode on port {port}. Go to /swagger for API documentation.";
});

// Also add a health check endpoint
app.MapGet("/health", () => 
{
    Console.WriteLine("Health endpoint accessed");
    return "Healthy";
});

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

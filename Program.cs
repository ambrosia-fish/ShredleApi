using ShredleApi.Services;
using ShredleApi.Helpers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Remove ALL existing configuration
builder.WebHost.ConfigureAppConfiguration((ctx, config) => {
    var dict = config.Sources.OfType<Microsoft.Extensions.Configuration.Json.JsonConfigurationSource>()
        .Where(source => source.Path.EndsWith("appsettings.json") || source.Path.EndsWith("appsettings.Development.json"))
        .ToList();
        
    foreach (var source in dict)
    {
        Console.WriteLine($"Removing configuration source: {source.Path}");
    }
});

// Detect environment and configure accordingly
var isHeroku = EnvironmentHelper.IsRunningOnHeroku;
var isDevelopment = EnvironmentHelper.IsDevelopment;
var environmentName = EnvironmentHelper.EnvironmentName;

// Log the detected environment for debugging
Console.WriteLine($"Application starting in {environmentName} environment");

// Get port from helper (which checks APP_PORT and then uses 5001)
var port = EnvironmentHelper.GetPort();
Console.WriteLine($"Using PORT: {port}");

// COMPLETELY REPLACE KESTREL CONFIGURATION
builder.WebHost.ConfigureKestrel(options =>
{
    Console.WriteLine($"IMPORTANT: Configuring Kestrel to use ONLY port {port}");
    
    // Clear all endpoints
    options.ListenOptions.Clear();
    
    // Add just one endpoint
    options.Listen(IPAddress.Any, int.Parse(port));
});

// Explicitly override URLs
builder.WebHost.UseUrls($"http://*:{port}");

// Get configuration values with proper precedence
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

// Use CORS configuration from appsettings.json or defaults
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = new[] 
        {
            "http://localhost:5173",
            "http://localhost:3000",
            "https://shredle.feztech.io",
            "https://shredle-app.vercel.app",
            "https://shredle-app-git-main-ambrosia-fishs-projects.vercel.app",
            "https://shredle-app-ambrosia-fishs-projects.vercel.app"
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

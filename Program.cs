using System.Numerics;
using Microsoft.EntityFrameworkCore;
using ShredleApi.Data;
using ShredleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// CRITICAL - Force use of PORT environment variable for Heroku
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Using PORT: {port}");
builder.WebHost.UseUrls($"http://+:{port}");

// Override configuration with environment variables
var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") 
    ?? builder.Configuration["Supabase:Url"];
var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") 
    ?? builder.Configuration["Supabase:Key"];
var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
    ?? builder.Configuration["OpenAI:ApiKey"];

// Update configuration with environment variables
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

// Add logging for debugging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://shredle.feztech.io",
            "https://8bde-68-0-249-64.ngrok-free.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<SupabaseService>();
builder.Services.AddScoped<OpenAiService>();

var app = builder.Build();

// Make sure we log the connection string being used (with password redacted)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "No connection string found";
var redactedConnectionString = connectionString.Contains("Password=") 
    ? connectionString.Replace(connectionString.Split(new[] { "Password=" }, StringSplitOptions.None)[1].Split(';')[0], "REDACTED") 
    : connectionString;
Console.WriteLine($"Using connection string: {redactedConnectionString}");

// Enable Swagger in all environments for easier debugging
app.UseSwagger();
app.UseSwaggerUI();

// Add a simple health check endpoint at the root path
app.MapGet("/", () => 
{
    Console.WriteLine("Root endpoint accessed");
    return "Shredle API is running. Go to /swagger for API documentation.";
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

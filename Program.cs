using ShredleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// CRITICAL - Force use of PORT environment variable for Heroku
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Using PORT: {port}");

// Configure Kestrel to listen on the correct port
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});

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

// Log Supabase configuration information
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

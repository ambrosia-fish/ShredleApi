using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using ShredleApi.Data;
using ShredleApi.Data.Repositories;
using ShredleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use Heroku's PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShredlePolicy", policy =>
    {
        policy.WithOrigins(
                "https://shredle-app.vercel.app", 
                "https://shredle.feztech.io", 
                "https://ca11-68-0-249-64.ngrok-free.app", 
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:8080"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Register HttpClient
builder.Services.AddHttpClient();

// Configure Entity Framework with PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string not configured");
}

// Parse DATABASE_URL if it's in Heroku format
if (connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ShredleDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure OpenAI
var openAiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not configured");
builder.Services.AddScoped<OpenAI.Chat.ChatClient>(provider => 
    new OpenAI.OpenAIClient(openAiKey).GetChatClient("gpt-3.5-turbo"));

// Register repositories
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ISoloRepository, SoloRepository>();

// Register services
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<SoloService>();
builder.Services.AddScoped<GuessValidationService>();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShredleDbContext>();
    dbContext.Database.Migrate();
}

// Debug CORS middleware
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].FirstOrDefault();
    var method = context.Request.Method;
    var path = context.Request.Path;
    
    Console.WriteLine($"=== CORS DEBUG ===");
    Console.WriteLine($"Method: {method}");
    Console.WriteLine($"Path: {path}");
    Console.WriteLine($"Origin: {origin}");
    Console.WriteLine($"Request Headers: {string.Join(", ", context.Request.Headers.Keys)}");
    Console.WriteLine($"==================");
    
    await next();
    
    var responseHeaders = string.Join(", ", context.Response.Headers.Keys);
    Console.WriteLine($"=== CORS RESPONSE ===");
    Console.WriteLine($"Response Headers: {responseHeaders}");
    Console.WriteLine($"Access-Control-Allow-Origin: {context.Response.Headers["Access-Control-Allow-Origin"]}");
    Console.WriteLine($"====================");
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: CORS must come BEFORE HTTPS redirect
app.UseCors("ShredlePolicy");

// Only use HTTPS redirect in production and when we actually have HTTPS configured
if (!app.Environment.IsDevelopment() && Environment.GetEnvironmentVariable("PORT") == null)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseIpRateLimiting();
app.UseAuthorization();
app.MapControllers();

app.Run();

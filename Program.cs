using System.Numerics;
using Microsoft.EntityFrameworkCore;
using ShredleApi.Data;
using ShredleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Heroku dynamic port
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

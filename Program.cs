using ShredleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Get allowed origins from configuration
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? 
            new[] 
            {
                "http://localhost:5173",
                "http://localhost:3000",
                "https://shredle.feztech.io",
                "https://shredle-app.vercel.app",
                "https://shredle-app-git-main-ambrosia-fishs-projects.vercel.app",
                "https://shredle-app-ambrosia-fishs-projects.vercel.app"
            };
        
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Register services
builder.Services.AddScoped<SupabaseService>();
builder.Services.AddScoped<OpenAiService>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.Logger.LogInformation("Running in Development mode");
}
else
{
    app.Logger.LogInformation($"Running in {app.Environment.EnvironmentName} mode");
}

// Enable Swagger in all environments for easier debugging
app.UseSwagger();
app.UseSwaggerUI();

// Add health endpoints
app.MapGet("/", () => $"Shredle API is running in {app.Environment.EnvironmentName} mode. Go to /swagger for API documentation.");
app.MapGet("/health", () => "Healthy");

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

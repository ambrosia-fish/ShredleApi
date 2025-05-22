using AspNetCoreRateLimit;
using ShredleApi.Data;
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
                "http://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
This removes the c

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Register HttpClient
builder.Services.AddHttpClient();

// Configure Supabase
var supabaseUrl = builder.Configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL not configured");
var supabaseKey = builder.Configuration["Supabase:Key"] ?? throw new InvalidOperationException("Supabase Key not configured");
builder.Services.AddScoped<Supabase.Client>(provider => 
    new Supabase.Client(supabaseUrl, supabaseKey));

// Configure OpenAI
var openAiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not configured");
builder.Services.AddScoped<OpenAI.Chat.ChatClient>(provider => 
    new OpenAI.OpenAIClient(openAiKey).GetChatClient("gpt-3.5-turbo"));

// Register our services
builder.Services.AddScoped<SupabaseRepository>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<SoloService>();
builder.Services.AddScoped<GuessValidationService>();

var app = builder.Build();

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
    Console.WriteLine($"=====================");
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("ShredlePolicy");
app.UseIpRateLimiting();
app.UseAuthorization();
app.MapControllers();

app.Run();
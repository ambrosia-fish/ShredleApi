using AspNetCoreRateLimit;
using ShredleApi.Data;
using ShredleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShredlePolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                           ?? new[] { "https://shredle-app.vercel.app", "http://localhost:5173" };
        
        policy.WithOrigins(allowedOrigins)
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
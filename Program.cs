using System.Numerics;
using Microsoft.EntityFrameworkCore;
using ShredleApi.Data;
using ShredleApi.Services;

var builder = WebApplication.CreateBuilder(args);

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
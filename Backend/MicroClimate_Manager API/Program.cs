using Data;
using Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<SensorReadingsService>();
builder.Services.AddScoped<SensorReadingsRepository>();
builder.Services.AddScoped<ActuatorLogsService>();
builder.Services.AddScoped<ActuatorLogsRepository>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<SettingsRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MicroClimateManager"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    db.Database.Migrate();

    SeedData.SeedAll(db);
}

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
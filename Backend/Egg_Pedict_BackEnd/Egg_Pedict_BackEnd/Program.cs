using Egg_Pedict_BackEnd.Data;
using Egg_Pedict_BackEnd.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TimescaleDb")));

// Register background services
builder.Services.AddHostedService<MqttService>();
builder.Services.AddHostedService<NotificationBackgroundService>();
builder.Services.AddHostedService<HenAgeUpdaterService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

// Enable CORS globally
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();

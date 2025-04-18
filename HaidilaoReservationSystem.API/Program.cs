using HaidilaoReservationSystem.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using HaidilaoReservationSystem.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<QueueWebSocketManager>();


// Add JSON options to use camel case for property names
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Register the DbContext with MariaDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MariaDbConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MariaDbConnection"))
    ));

// ✅ CORS must be added BEFORE builder.Build()
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowAll");

// Add WebSocket middleware
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws/queue")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var queueManager = context.RequestServices.GetRequiredService<QueueWebSocketManager>();
            await queueManager.HandleConnection(webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

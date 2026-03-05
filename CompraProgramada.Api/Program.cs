using CompraProgramada.Api.Data;
using CompraProgramada.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
builder.Services.AddScoped<IParserServ, ParserServ>();
builder.Services.AddScoped<IKafkaServ, KafkaServ>();
builder.Services.AddScoped<MotorServ>();
builder.Services.AddHostedService<DispMotorServ>();
builder.Services.AddScoped<RebalanceServ>();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("PermitirAngular");


app.Use(async (context, next) => {
    
    var requestId = Guid.NewGuid().ToString();
    context.Response.Headers.Append("X-Request-Id", requestId);
    
    await next();
});


if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
using Microsoft.OpenApi.Models;
using ProblemApp.Scripts;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<GarbageCollectionStressScript>();
builder.Services.AddSingleton<DeadlockOnThreadPoolScript>();
builder.Services.AddSingleton<DeadlockedWithThreadsScript>();
builder.Services.AddSingleton<MemoryLeakScript>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PropblemApp",
        Description = "ASP.NET Core application to emulate different problems like thread starvation, deadlocks, memory leaks etc.",
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.OpenApi.Models;
using ProblemApp.Scripts;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Scan(scan => scan
    .FromAssemblyOf<MemoryLeakScript>()
        .AddClasses(classes => classes.AssignableTo(typeof(IScript<>)))
            .AsSelf()
            .WithSingletonLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(IStartOnlyScript<>)))
            .AsSelf()
            .WithSingletonLifetime());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = nameof(ProblemApp),
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

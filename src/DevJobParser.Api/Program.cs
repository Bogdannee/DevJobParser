using DevJobParser.Api;
using DevJobParser.Core.Abstractions;
using DevJobParser.Infrastructure;
using DevJobParser.Parsers.WorkUa;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddWorkUaParser(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddRequestTimeouts();

var app = builder.Build();

app.UseExceptionHandler(); 
app.UseRequestTimeouts();

app.MapGet("/", () => "Hello");
app.MapGet("/api/jobs/workua", 
    async ([FromKeyedServices("workua")]IJobParser parser, CancellationToken token) =>
    {
        var jobCards = await parser.GetJobCardList(token);
        return Results.Ok(jobCards);
    })
    .WithRequestTimeout(TimeSpan.FromMinutes(3));

app.Run();

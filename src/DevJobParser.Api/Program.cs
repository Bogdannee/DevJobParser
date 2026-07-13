using DevJobParser.Core.Abstractions;
using DevJobParser.Infrastructure;
using DevJobParser.Parsers.WorkUa;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddWorkUaParser();

var app = builder.Build();

app.MapGet("/", () => "Hello");
app.MapGet("/api/jobs/workua", 
    async ([FromKeyedServices("workua")]IJobParser parser, CancellationToken token) =>
    {
        try
        {
            var jobCards = await parser.GetJobCardList(token);
            return Results.Ok(jobCards);
        }
        catch(Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    });

app.Run();

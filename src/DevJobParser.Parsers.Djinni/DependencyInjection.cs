using Microsoft.Extensions.DependencyInjection;
using DevJobParser.Parsers.Djinni.DetailsParser;
using DevJobParser.Parsers.Djinni.LinkParser;
using DevJobParser.Parsers.Djinni.Options;
using Microsoft.Extensions.Configuration;
using DevJobParser.Core.Abstractions;

namespace DevJobParser.Parsers.Djinni;

public static class DependencyInjection
{
    public static IServiceCollection AddDjinniParser(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DjinniJobLinkParser>();
        services.AddSingleton<DjinniJobDetailsParser>();
        services.AddKeyedSingleton<IJobParser, DjinniJobParser>("workua");
        services.Configure<DjinniParserOptions>(configuration.GetSection("WorkUaParser"));

        return services;
    }
}

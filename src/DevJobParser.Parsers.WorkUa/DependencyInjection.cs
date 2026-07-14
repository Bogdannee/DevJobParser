using DevJobParser.Core.Abstractions;
using DevJobParser.Parsers.WorkUa.DetailsParser;
using DevJobParser.Parsers.WorkUa.Options;
using DevJobParser.Parsers.WorkUa.LinkParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DevJobParser.Parsers.WorkUa
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWorkUaParser(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<WorkUaJobLinkParser>();
            services.AddSingleton<WorkUaJobDetailsParser>();
            services.AddKeyedSingleton<IJobParser, WorkUaJobParser>("workua");
            services.Configure<WorkUaParserOptions>(configuration.GetSection("WorkUaParser"));
            
            return services;
        }
    }
}
using DevJobParser.Core.Abstractions;
using DevJobParser.Parsers.WorkUa.DetailsParser;
using DevJobParser.Parsers.WorkUa.LinkParser;
using Microsoft.Extensions.DependencyInjection;

namespace DevJobParser.Parsers.WorkUa
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWorkUaParser(this IServiceCollection services)
        {
            services.AddSingleton<WorkUaJobLinkParser>();
            services.AddSingleton<WorkUaJobDetailsParser>();
            services.AddKeyedSingleton<IJobParser, WorkUaJobParser>("workua");
            services.Configure<WorkUaParserOptions>(options =>
            {
                options.SearchLink = "https://www.work.ua/jobs-remote-it-.net/";
                options.MaxPages = 50;
                options.SourceName = "work.ua .NET jobs";
            });
            
            return services;
        }
    }
}
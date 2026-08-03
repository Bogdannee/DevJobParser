using DevJobParser.Infrastructure.Builders;
using DevJobParser.Infrastructure.HtmlExtractors;
using DevJobParser.Infrastructure.Loading;
using Microsoft.Extensions.DependencyInjection;

namespace DevJobParser.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<HttpClient>(provider =>
            {
                var client = new HttpClient();
                
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");

                return client;
            });
            
            services.AddSingleton<HtmlExtractController>();
            services.AddSingleton<JobCardBuilder>();

            services.AddSingleton<IHttpContentLoader, HttpContentLoader>();
            return services;
        }
    }
}
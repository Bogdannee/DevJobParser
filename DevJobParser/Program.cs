using DevJobParser.DTO;
using DevJobParser.Parsers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp.Html.Parser;

namespace DevJobParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Warning);
                builder.AddConsole();
                builder.AddDebug();
            });

            services.AddSingleton<HttpClient>();
            services.AddSingleton<HtmlParser>();
            services.AddSingleton<WorkUaHtmlLoader>();
            services.AddSingleton<WorkUaJobLinkParser>();
            services.AddSingleton<WorkUaJobDetailsParser>();
            services.AddSingleton<WorkUaJobParser>();
            services.Configure<WorkUaParserOptions>(options =>
            {
                options.SearchLink = "https://www.work.ua/jobs-remote-it-.net/?days=124&page=";
            });

            using var serviceProvider = services.BuildServiceProvider();

            var programLogger = serviceProvider.GetRequiredService<ILogger<Program>>();

            using CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                programLogger.LogInformation("Парсинг Work.ua начат.");

                var workUaParser = serviceProvider.GetRequiredService<WorkUaJobParser>();
                List<JobCard> jobCards = await workUaParser.GetJobCardList(cts.Token);

                programLogger.LogInformation($"Парсинг завершен. Найдено {jobCards.Count} вакансий.");
            }
            catch (OperationCanceledException)
            {
                programLogger.LogWarning("Операция парсинга была отменена.");
            }
            catch (HtmlPageLoadingException ex)
            {
                programLogger.LogError(ex, $"Ошибка загрузки HTML-страницы по URL: {ex.Url}");
            }
            catch (JobParsingException ex)
            {
                programLogger.LogError(ex, $"Ошибка парсинга вакансии по URL: {ex.JobUrl}");
            }
            catch (Exception ex)
            {
                programLogger.LogCritical(ex, "Непредвиденная ошибка");
            }
        }
    }
}

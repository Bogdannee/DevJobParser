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

            services.AddSingleton<HttpClient>(provider =>
            {
                var client = new HttpClient();

                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");

                return client;
            });
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

            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            Console.CancelKeyPress +=(sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                programLogger.LogWarning("Отмена выполнения (Ctrl+C). Завершение работы парсера...");
                cts.Cancel();
            };

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

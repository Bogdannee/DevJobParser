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
                options.SearchLink = "https://www.work.ua/jobs-remote-it-.net/";
                options.MaxPages = 50;
            });
            using var serviceProvider = services.BuildServiceProvider();

            var programLogger = serviceProvider.GetRequiredService<ILogger<Program>>();

            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var manuallyCancelled = false;
            Console.CancelKeyPress +=(sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                programLogger.LogWarning("Cancellation requested (Ctrl+C). Shutting down the parser...");
                manuallyCancelled = true;
                cts.Cancel();
            };

            try
            {
                programLogger.LogInformation("Work.ua parsing started.");

                var workUaParser = serviceProvider.GetRequiredService<WorkUaJobParser>();
                List<JobCard> jobCards = await workUaParser.GetJobCardList(cts.Token);

                programLogger.LogInformation($"Parsing completed. Found {jobCards.Count} jobs.");
            }
            catch (OperationCanceledException)
            {
                var reason = manuallyCancelled ? "By user" : "timeout";
                programLogger.LogWarning($"The parsing operation was cancelled. {reason}.");
            }
            catch (HtmlPageLoadingException ex)
            {
                programLogger.LogError(ex, $"Error loading HTML page for URL: {ex.Url}");
            }
            catch (JobParsingException ex)
            {
                programLogger.LogError(ex, $"Error parsing job details for URL: {ex.JobUrl}");
            }
            catch (ArgumentException ex)
            {
                programLogger.LogError($"Configuration error: {ex.Message}");
            }
            catch (Exception ex)
            {
                programLogger.LogCritical(ex, "An unexpected error occurred");
            }
        }
    }
}

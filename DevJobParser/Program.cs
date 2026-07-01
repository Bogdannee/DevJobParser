using DevJobParser.DTO;
using DevJobParser.Parsers;
using Microsoft.Extensions.Logging;

namespace DevJobParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Warning);
                builder.AddConsole();
                builder.AddDebug();
            });

            ILogger<WorkUaJobParser> workUaLogger = loggerFactory.CreateLogger<WorkUaJobParser>();
            ILogger<Program> programLogger = loggerFactory.CreateLogger<Program>();

            using CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                programLogger.LogInformation("Парсинг Work.ua начат.");

                var workUaParser = new WorkUaJobParser("https://www.work.ua/jobs-remote-it-.net/?days=124&page=", workUaLogger);
                List<JobCard> jobCards = await workUaParser.GetJobCardList(cts.Token);

                programLogger.LogInformation($"Парсинг завершен. Найдено {jobCards.Count} вакансий.");
            }
            catch (OperationCanceledException)
            {
                programLogger.LogWarning("Операция парсинга была отменена.");
            }
            catch (Exception ex)
            {
                programLogger.LogError(ex, "Произошла непредсказуемая ошибка в Program.cs");
            }
        }
    }
}

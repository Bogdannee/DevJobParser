using DevJobParser.DTO;
using DevJobParser.Parsers;

namespace DevJobParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            var workUaParser = new WorkUaJobParser("https://www.work.ua/jobs-remote-it-.net/?days=124&page=");

            try
            {
                List<JobCard> jobCards = await workUaParser.GetJobCardList(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Операция парсинга была отменена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
            //var djinniJobs = DjinniParser.Start();
            //RobotaUaParser.Start();
        }
    }
}

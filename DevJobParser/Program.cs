using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using DevJobParser.Parsers;

namespace DevJobParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var workUaParser = new WorkUaJobParser("https://www.work.ua/jobs-remote-it-.net/?days=124&page=");
            workUaParser.GetJobCardList();
            //var djinniJobs = DjinniParser.Start();
            //RobotaUaParser.Start();
        }
    }
}

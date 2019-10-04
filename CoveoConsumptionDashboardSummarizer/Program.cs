using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = System.Console;

namespace CoveoConsumptionDashboardSummarizer
{
    class Program
    {
        static void Main(string[] args)
        {

            DateTime started = DateTime.Now;

            Console.WriteLine("Making the summarizer");

            Summarizer summarizer = new Summarizer();

            Console.WriteLine("Running the summarizer");

            summarizer.Run();

            TimeSpan took = DateTime.Now - started;

            Console.WriteLine($"Done summarizing, took {took.Minutes}:{took.Seconds}");
            Console.WriteLine("finished summarizing, hit any key to exit please");
            Console.WriteLine("-----------------------------------------");
            Console.ReadKey();
        }
    }
}

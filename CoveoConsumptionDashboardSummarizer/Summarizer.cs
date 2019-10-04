using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoveoConsumptionDashboardSummarizer
{
    public class Summarizer
    {
        private FileInfo _queriesFileInfo;
        private List<QueryLogItem> _logItems;

        public void Run()
        {

            _queriesFileInfo= GetQueriesFile();

            _logItems = GetLogItems();

            Summarize();


        }

        private void Summarize()
        {
            
            Console.WriteLine();
            Console.WriteLine(new string('-', 27));

            Console.WriteLine($"Query Log Entries: {_logItems.Count:N0}");

            //ShowDateOfFirstPersistentQuery();

            SummarizeDailyQueries();

            //SummarizeImprovementSincePersistentQueriesWereIntroduced();

            //SummarizeImprovementSinceLargerPageSizesWereIntroduced();

            Console.WriteLine(new string('-', 27));
            Console.WriteLine();
        }

        private void ShowDateOfFirstPersistentQuery()
        {
            Console.WriteLine();

            var dateOfFirstPersistentQuery = _logItems.OrderBy(x => x.DateTime.Date).First(x => x.IsPersistentQuery);

            Console.WriteLine($"First Persistent Query: {dateOfFirstPersistentQuery.DateTime.ToShortDateString()}");

        }

        private void SummarizeDailyQueries()
        {

            string someSeperatorcharacters = new string('%', 13);

            Console.WriteLine();
            Console.WriteLine($"{someSeperatorcharacters} Daily Query Count {someSeperatorcharacters} ");

            var queriesGroupedByDate = _logItems.GroupBy(x => x.DateTime.Date).OrderBy(g => g.Key);

            foreach (var curQueryGroup in queriesGroupedByDate)
            {
                Console.WriteLine($"{curQueryGroup.Key.ToShortDateString()}: {curQueryGroup.Count():N0}");
            }

            Console.WriteLine($"{someSeperatorcharacters} End Daily Query Count {someSeperatorcharacters} ");
            Console.WriteLine();
        }

        private void SummarizeImprovementSincePersistentQueriesWereIntroduced()
        {
            string someSeperatorcharacters = new string('+', 7);

            DateTime persistentQueriesInPlace = new DateTime(2019, 9, 17);

            Console.WriteLine();
            Console.WriteLine($"{someSeperatorcharacters} Improvments Since Listing Pages Went with Persistent Queries {someSeperatorcharacters}");

            Console.WriteLine();
            Console.WriteLine($"First Day With Persistent Queries: {persistentQueriesInPlace.ToShortDateString()}");

            Console.WriteLine($"Average Query Count Per Day Before: {GetAverageQueryCountPerDayInRange(DateTime.MinValue, persistentQueriesInPlace):N0}");
            Console.WriteLine($"Average Query Count Per Day After: {GetAverageQueryCountPerDayInRange(persistentQueriesInPlace, DateTime.MaxValue):N0}");

            Console.WriteLine();
            Console.WriteLine($"{someSeperatorcharacters} End Improvments Since Listing Pages Went with Persistent Queries {someSeperatorcharacters}");
            Console.WriteLine();

        }

        private void SummarizeImprovementSinceLargerPageSizesWereIntroduced()
        {
            string someSeperatorcharacters = new string('=', 7);

            DateTime largerPageSizeInPlace = new DateTime(2019, 9, 20);

            Console.WriteLine();
            Console.WriteLine($"{someSeperatorcharacters} Improvments Since Listing Search Page Had It's Page Size Increased {someSeperatorcharacters}");

            Console.WriteLine();
            Console.WriteLine($"First Day With Page Size Increase: {largerPageSizeInPlace.ToShortDateString()}");

            Console.WriteLine($"Average Query Count Per Day Before: {GetAverageQueryCountPerDayInRange(DateTime.MinValue, largerPageSizeInPlace):N0}");
            Console.WriteLine($"Average Query Count Per Day After: {GetAverageQueryCountPerDayInRange(largerPageSizeInPlace, DateTime.MaxValue):N0}");

            Console.WriteLine();
            Console.WriteLine($"{someSeperatorcharacters} End Improvments Since Listing Search Page Had It's Page Size Increased {someSeperatorcharacters}");
            Console.WriteLine();

        }

        private int GetAverageQueryCountPerDayInRange(DateTime firstDayOfRange, DateTime lastDayOfRange)
        {
            int count = (int) _logItems.Where(x => x.DateTime.Date >= firstDayOfRange && x.DateTime.Date <= lastDayOfRange)
                .GroupBy(x => x.DateTime.Date)
                .Average(g => g.Count());

            return count;
        }

        private List<QueryLogItem> GetLogItems()
        {
            List<QueryLogItem> logItems;

            using (var reader = new StreamReader(_queriesFileInfo.FullName))
            using (var csv = new CsvReader(reader))
            {
                logItems = csv.GetRecords<QueryLogItem>().ToList();
            }

            return logItems;
        }
        
        private FileInfo GetQueriesFile()
        {

            //const string fileName = "Queries_mohawkindustriesproductionwwrtu1cs___2019-09-01_2019-09-30.csv";

            const string fileName = "Queries_mohawkindustriesproductionwwrtu1cs_Daltile_All_Products_2019-10-01_2019-10-31.csv";

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string fullQueriesFilePath = Path.Combine(desktopPath, fileName);

            FileInfo queriesFile = new FileInfo(fullQueriesFilePath);

            return queriesFile;
        }
    }
}

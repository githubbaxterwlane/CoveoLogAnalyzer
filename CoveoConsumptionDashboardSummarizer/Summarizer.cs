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
        private FileInfo[] _queriesFileInfoArray;
        private List<QueryLogItem> _logItems;

        public void Run()
        {

            _queriesFileInfoArray= GetQueriesFiles();

            _logItems = GetLogItems();

            Summarize();


        }

        private void Summarize()
        {

            Console.WriteLine($"Trimming the log entries to only include queries from this month: {DateTime.Now:MMMM}");
            _logItems = _logItems.Where(i => i.DateTime.Month == DateTime.Now.Month).ToList();

            Console.WriteLine();
            Console.WriteLine(new string('-', 27));

            Console.WriteLine($"Query Log Entries: {_logItems.Count:N0}");

            //ShowDateOfFirstPersistentQuery();

            SummarizeDailyQueries();

            SummarizeDailyPersistentQueries();

            SummarizeDailyNormalQueries();

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

        private void SummarizeDailyPersistentQueries()
        {

            string someSeperatorcharacters = new string('%', 13);

            Console.WriteLine();
            Console.WriteLine($"{someSeperatorcharacters} Daily Persistent Query Count {someSeperatorcharacters} ");

            var queriesGroupedByDate = _logItems.Where(x => x.IsPersistentQuery).GroupBy(x => x.DateTime.Date).OrderBy(g => g.Key);

            foreach (var curQueryGroup in queriesGroupedByDate)
            {
                Console.WriteLine($"{curQueryGroup.Key.ToShortDateString()}: {curQueryGroup.Count():N0}");
            }

            Console.WriteLine($"{someSeperatorcharacters} End Daily Persistent Query Count {someSeperatorcharacters} ");
            Console.WriteLine();
        }

        private void SummarizeDailyNormalQueries()
        {

            string someSeperatorcharacters = new string('%', 13);

            Console.WriteLine();
            Console.WriteLine($"{someSeperatorcharacters} Normal Daily Query Count {someSeperatorcharacters} ");

            var queriesGroupedByDate =
                _logItems.Where(x => !x.IsPersistentQuery)
                    .GroupBy(x => new {QueryDate = x.DateTime.Date, QuerySearchHub = x.SearchHub})
                    .OrderBy(g => g.Key.QueryDate)
                    .ThenBy(g => g.Key.QuerySearchHub);

            foreach (var curQueryGroup in queriesGroupedByDate)
            {
                Console.WriteLine($"{curQueryGroup.Key.QueryDate.ToShortDateString()}/{ curQueryGroup.Key.QuerySearchHub }: {curQueryGroup.Count():N0}");
            }

            Console.WriteLine($"{someSeperatorcharacters} End Normal Daily Query Count {someSeperatorcharacters} ");
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
            List<QueryLogItem> logItems = new List<QueryLogItem>();

            foreach (var curCSVFileInfo in _queriesFileInfoArray)
            {
                using (var reader = new StreamReader(curCSVFileInfo.FullName))
                using (var csv = new CsvReader(reader))
                {
                    logItems.AddRange(csv.GetRecords<QueryLogItem>().ToList());
                }
            }

            return logItems;
        }
        
        private FileInfo[] GetQueriesFiles()
        {
            const string logFolderName = "Logs";
            const string queryConsuptionFolderName = "QueryConsumption";
            const string fileExtention = "csv";
            
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string fullQueriesFolderPath = Path.Combine(desktopPath, logFolderName, queryConsuptionFolderName);

            DirectoryInfo queriesDirectoryInfo = new DirectoryInfo(fullQueriesFolderPath);

            var queriesFiles = queriesDirectoryInfo.GetFiles("*." + fileExtention); 
            
            return queriesFiles;
        }
    }
}

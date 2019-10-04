using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace CoveoConsumptionDashboardSummarizer
{
    public class QueryLogItem
    {
        [Name("datetime")]
        public DateTime DateTime { get; set; }

        [Name("searchId")]
        public Guid SearchId { get; set; }

        [Name("basicExpression")]
        public string BasicExpression { get; set; }

        [Name("advancedExpression")]
        public string AvancedExpression { get; set; }
        
        [Name("largeExpression")]
        public string LargeExpression { get; set; }
        
        [Name("constantExpression")]
        public string ConstantExpression { get; set; }

        [Name("disjunctionExpression")]
        public string DisjunctionExpression { get; set; }
        
        [Name("mandatoryExpression")]
        public string MandatoryExpression { get; set; }

        [Name("sortCriteria")]
        public string SortCriteria { get; set; }
        
        [Name("identities")]
        public string Identities { get; set; }
        
        [Name("pipeline")]
        public string Pipeline { get; set; }

        [Name("searchHub")]
        public string SearchHub { get; set; }
        
        [Name("tab")]
        public string Tab { get; set; }
        
        [Name("recommendation")]
        public string Recommendation { get; set; }

        [Name("isPersistentQuery")]
        public bool IsPersistentQuery { get; set; }

    }
}

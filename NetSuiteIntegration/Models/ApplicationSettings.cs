using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class ApplicationSettings
    {
        public int Id { get; set; }

        public string? Enviroment { get; set; }

        public bool? Enabled { get; set; }

        public string? NetSuiteAccountID { get; set; }

        public string? NetSuiteURL { get; set; }

        public string? NetSuiteConsumerKey { get; set; }

        public string? NetSuiteTokenID { get; set; }

        public string? NetSuiteConsumerSecret { get; set; }

        public string? NetSuiteTokenSecret { get; set; }

        public string? UniteBaseURL { get; set; }

        public string? UniteTokenURL { get; set; }

        public string? UniteAPIKey { get; set; }
    }
}
